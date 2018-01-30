using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
          
            //QueryExpression qe = new QueryExpression();
            //qe.EntityName = "account";
            //qe.ColumnSet = new ColumnSet();
            //qe.ColumnSet.Columns.Add("name");

            //qe.LinkEntities.Add(new LinkEntity("account", "contact", "primarycontactid", "contactid", JoinOperator.Inner));
            //qe.LinkEntities[0].Columns.AddColumns("firstname", "lastname");
            //qe.LinkEntities[0].EntityAlias = "primarycontact";

            //EntityCollection ec = _service.RetrieveMultiple(qe);
        }
    }
}
