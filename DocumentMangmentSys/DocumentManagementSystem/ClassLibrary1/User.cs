using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace ClassLibrary1
{
    public class User
    {
        public User()
        { }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
        public string timezone { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<SelectListItem> lstDocumentType { get; set; }
        public string Documenttyp { get; set; }
        public string FullName { get; set; }
    }
}
