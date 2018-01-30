using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;


namespace ClassLibrary1
{
    public class TicketData
    {
        public Guid incidentid { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string subjectid { get; set; }
        public string customerid { get; set; }
        public string new_name { get; set; }
        public string primarycontactid { get; set; }
        public List<SelectListItem> lstSubject { get; set; }
        public List<SelectListItem> lstSite { get; set; }
        public string PageMessage { get; set; }
        public HttpPostedFileBase Attachment { get; set; }
    }
}
