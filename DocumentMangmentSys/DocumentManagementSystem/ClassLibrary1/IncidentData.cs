using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary1
{
    public class IncidentData
    {
        public string incidentid { get; set; }
        public string ticketnumber { get; set; }
        public string statecode { get; set; }
        public string title { get; set; }
        public string statuscode { get; set; }
        public string createdon { get; set; }
        public string description { get; set; }
        public Guid new_sitename { get; set; }
        public Guid customerid { get; set; }
        public Guid primarycontactid { get; set; }
        public Guid subjectid { get; set; }

    }
}
