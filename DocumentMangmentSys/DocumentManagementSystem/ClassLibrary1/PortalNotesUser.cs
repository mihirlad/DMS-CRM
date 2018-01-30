using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary1
{
    public class PortalNotesUser
    {
        public Boolean mhl_allowtoapprove { get; set; }
        public string mhl_approvalstatus { get; set; }
        public Guid mhl_approvedby { get; set; }
        public string mhl_documentnotesid { get; set; }
        public string mhl_name { get; set; }
        public Guid mhl_portalnotesharedid { get; set; }
        public Guid mhl_portaluser { get; set; }
        public string PortalUserName { get; set; }
    }
}
