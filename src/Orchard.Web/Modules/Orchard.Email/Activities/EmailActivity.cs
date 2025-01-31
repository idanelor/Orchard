﻿using System.Collections.Generic;
using Orchard.Email.Services;
using Orchard.Environment.Extensions;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Messaging.Services;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;

namespace Orchard.Email.Activities {
    public interface IJobsQueueService : IEventHandler {
        void Enqueue(string message, object parameters, int priority);
    }

    [OrchardFeature("Orchard.Email.Workflows")]
    public class EmailActivity : Task {
        private readonly IMessageService _messageService;
        private readonly IJobsQueueService _jobsQueueService;
        public ILogger Logger { get; set; }

        public EmailActivity(
            IMessageService messageService,
            IJobsQueueService jobsQueueService
            ) {
            _messageService = messageService;
            _jobsQueueService = jobsQueueService;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Done"), T("Failed") };
        }

        public override string Form {
            get {
                return "EmailActivity";
            }
        }

        public override LocalizedString Category {
            get { return T("Messaging"); }
        }

        public override string Name {
            get { return "SendEmail"; }
        }

        public override LocalizedString Description {
            get { return T("Sends an email to a specific user."); }
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var body = activityContext.GetState<string>("Body");
            var subject = activityContext.GetState<string>("Subject");
            var recipients = activityContext.GetState<string>("Recipients");
            var replyTo = activityContext.GetState<string>("ReplyTo");
            var bcc = activityContext.GetState<string>("Bcc");
            var cc = activityContext.GetState<string>("CC");
            var notifyReadEmail = activityContext.GetState<bool>("NotifyReadEmail");

            var parameters = new Dictionary<string, object> {
                {"Subject", subject},
                {"Body", body},
                {"Recipients", recipients},
                {"ReplyTo", replyTo},
                {"Bcc", bcc},
                {"CC", cc},
                {"NotifyReadEmail", notifyReadEmail }
            };

            if (string.IsNullOrWhiteSpace(recipients)) {
                Logger.Error("Email message doesn't have any recipient for Workflow {0}", workflowContext.Record.WorkflowDefinitionRecord.Name);
                yield return T("Failed");
            }
            else {
                var queued = activityContext.GetState<bool>("Queued");

                if (!queued) {
                    _messageService.Send(SmtpMessageChannel.MessageType, parameters);
                }
                else {
                    var priority = activityContext.GetState<int>("Priority");
                    _jobsQueueService.Enqueue("IMessageService.Send", new { type = SmtpMessageChannel.MessageType, parameters = parameters }, priority);
                }

                yield return T("Done");
            }
        }
    }
}