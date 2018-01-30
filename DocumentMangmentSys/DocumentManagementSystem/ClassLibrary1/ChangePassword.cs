using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;


namespace ClassLibrary1
{
    public class ChangePassword
    {
        public ChangePassword()
        { }
        //public Guid UserId { get; set; }
        //public string UserName { get; set; }
        //public string Password { get; set; }
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string CompareNewPassword { get; set; }

        public string OldPassword { get; set; }
        //public string PageMessage { get; set; }
    }
}
