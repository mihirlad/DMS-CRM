using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary1
{
    public class RecordList
    {
        public List<EntityAttributes> lstHeaderColumn { get; set; }
        public List<DataRow> lstDataRow { get; set; }
        public List<IncidentData> lstData { get; set; }
        //public List<NotesData> lstNotesData { get; set; }
        public int statecode { get; set; }
        public string SearchKeyword { get; set; }
        public int Usercode { get; set; }
    }
}
