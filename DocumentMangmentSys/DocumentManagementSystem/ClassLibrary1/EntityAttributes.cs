using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClassLibrary1
{
    public class EntityAttributes
    {
        public int Index { get; set; }
        public string DisplayName { get; set; }
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public Guid EntityId { get; set; }       
    }
}
