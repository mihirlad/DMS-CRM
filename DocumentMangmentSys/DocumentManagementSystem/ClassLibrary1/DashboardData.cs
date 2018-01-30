using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary1
{
    public class DashboardData
    {
        public DashboardData()
        { }
        public string Approved { get; set; }
        public string Reject { get; set; }
        public string Pending { get; set; }
        public string Processed { get; set; }
    }
}
