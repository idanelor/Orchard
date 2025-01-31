﻿using Orchard.ContentManagement;
using Orchard.Environment.Extensions;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Users.ViewModels
{
    [OrchardFeature("Orchard.Users.EditPasswordByAdmin")]
    public class UserEditPasswordViewModel2 {
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        public IContent User { get; set; }
    }
}