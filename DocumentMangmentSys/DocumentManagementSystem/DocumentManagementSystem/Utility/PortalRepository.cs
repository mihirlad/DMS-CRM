using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using BL = ClassLibrary1;

namespace ActivityTest.Utility
{
    public static class PortalRepository
    {
        public static BL.User AuthenticateUser(string pUserName, string pPassword, string DocumentType, IOrganizationService CrmService)
        {
            Entity PortalUser = GetAuthenticateUser(pUserName, pPassword, CrmService);
            if (PortalUser != null)
            {
                string TimeZoneCode = "";
                if (PortalUser.Attributes.Contains("mhl_timezone"))
                {
                    TimeZoneCode = Convert.ToString(PortalUser.Attributes["mhl_timezone"]);
                }
                EntityCollection TimeZoneDef = null;
                if (TimeZoneCode != string.Empty)
                {
                    var qe = new QueryExpression("timezonedefinition");
                    qe.ColumnSet = new ColumnSet("standardname");
                    qe.Criteria.AddCondition("timezonecode", ConditionOperator.Equal, TimeZoneCode);
                    TimeZoneDef = CrmService.RetrieveMultiple(qe);
                }

                BL.User _User = new BL.User()
                {
                    Password = pPassword,
                    UserId = PortalUser.Id,
                    FullName = PortalUser.Attributes.Contains("new_fullname") ? Convert.ToString(PortalUser.Attributes["new_fullname"]) : "",
                    UserName = PortalUser.Attributes.Contains("new_name") ? Convert.ToString(PortalUser.Attributes["new_name"]) : "",
                    CompanyName = PortalUser.Attributes.Contains("parentcustomerid") ? Convert.ToString(((EntityReference)PortalUser.Attributes["parentcustomerid"]).Id) : "",
                    timezone = TimeZoneDef != null ? TimeZoneDef.Entities[0].Attributes["standardname"].ToString() : "",
                    FirstName = PortalUser.Attributes.Contains("new_name") ? Convert.ToString(PortalUser.Attributes["new_name"]) : "",
                    LastName = PortalUser.Attributes.Contains("mhl_usertype") ? Convert.ToString(((OptionSetValue)PortalUser["mhl_usertype"]).Value) : "",
                    Documenttyp = string.Empty//DocumentType

                    //Email = PortalUser.Contains("new_email") ? Convert.ToString(PortalUser["new_email"]) : ""
                };
                //int value = ((OptionSetValue)PortalUser["mhl_usertype"]).Value;
                //var optionList = (from o in BL.CurrentUser.Instance.VerifiedUser.LastName.OptionSet.Options select new { Value = o.Value, Text = o.Label.UserLocalizedLabel.Label }).ToList();
                //Guid timezoneid = new Guid(_User.timezone);
                //EntityReference TimezoneId = new EntityReference("subject", timezoneid);




                //TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                //DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);

                return _User;
            }
            return null;
        }

        public static string ChangePWDAuthenticateUser(string pUserId, string pPassword, IOrganizationService CrmService)
        {
            QueryExpression Query = new QueryExpression("new_portaluser");
            Query.Criteria.AddCondition(new ConditionExpression("new_portaluserid", ConditionOperator.Equal, pUserId));
            Query.Criteria.AddCondition(new ConditionExpression("new_password", ConditionOperator.Equal, pPassword));
            //Query.Criteria.AddCondition(new ConditionExpression("adx_logonenabled", ConditionOperator.Equal, true));
            Query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));


            Query.ColumnSet = new ColumnSet(true);
            EntityCollection UserPortalCol = CrmService.RetrieveMultiple(Query);

            if (UserPortalCol != null && UserPortalCol.Entities.Count > 0)
                return "Access";
            else
                return null;
        }

        public static Entity GetAuthenticateUser(string UserName, string Password, IOrganizationService _service)
        {
            QueryExpression Query = new QueryExpression("new_portaluser");
            Query.Criteria.AddCondition(new ConditionExpression("new_name", ConditionOperator.Equal, UserName));
            Query.Criteria.AddCondition(new ConditionExpression("new_password", ConditionOperator.Equal, Password));
            //Query.Criteria.AddCondition(new ConditionExpression("adx_logonenabled", ConditionOperator.Equal, true));
            Query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));


            Query.ColumnSet = new ColumnSet(true);
            EntityCollection UserPortalCol = _service.RetrieveMultiple(Query);

            if (UserPortalCol != null && UserPortalCol.Entities.Count > 0)
                return UserPortalCol.Entities[0];
            else
                return null;
        }

        public static Entity GetActivityUser(IOrganizationService _service)
        {
            QueryExpression Query = new QueryExpression("new_portaluser");
            Query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));


            Query.ColumnSet = new ColumnSet(true);
            EntityCollection UserPortalCol = _service.RetrieveMultiple(Query);

            if (UserPortalCol != null && UserPortalCol.Entities.Count > 0)
                return UserPortalCol.Entities[0];
            else
                return null;
        }

        public static List<BL.NotesData> GetDocumentNotesData(string EntityName, IOrganizationService _service, Guid Id, string Search, string UserType, string ApprovalStatus, string PortalUser, DateTime StartDate, DateTime EndDate)
        {
            List<BL.NotesData> lst = new List<BL.NotesData>();

            //List<BL.User>



            Entity ConfigEntity = null;
            EntityCollection Cols = null;


            bool AllowApproveFlag = false;
            EntityCollection ColsShared = new EntityCollection();
            if (UserType != "125970001")
            {
                #region Document For User
                QueryExpression QueryShared = new QueryExpression("mhl_portalnoteshared");
                QueryShared.ColumnSet = new ColumnSet(true);
                QueryShared.Criteria.AddCondition(new ConditionExpression("mhl_portaluser", ConditionOperator.Equal, BL.CurrentUser.Instance.VerifiedUser.UserId));
                ColsShared = _service.RetrieveMultiple(QueryShared);

                var SharedDocument = ColsShared.Entities.Select(x => x.Attributes["mhl_documentnotesid"]).ToArray();
                //if (ColsShared.Entities.Count > 0)
                //{
                //    var ApprovedFlag = ColsShared.Entities.Select(x => x.Attributes["mhl_allowtoapprove"]).ToArray();
                //    if (ApprovedFlag[0].ToString().ToLower() == "true")
                //    {
                //        AllowApproveFlag = true;
                //    }


                //}
                if (SharedDocument.Length > 0)
                {

                    QueryExpression Query = new QueryExpression(EntityName);
                    Query.ColumnSet = new ColumnSet(new[] { "subject", "notetext", "objectid", "annotationid", "createdby", "createdon", "filename", "documentbody", "mimetype" });
                    FilterExpression filter = new FilterExpression(LogicalOperator.And);

                    FilterExpression filter1 = new FilterExpression(LogicalOperator.And);
                    filter1.Conditions.Add(new ConditionExpression("annotationid", ConditionOperator.In, ColsShared.Entities.Select(x => x.Attributes["mhl_documentnotesid"]).ToArray()));

                    if (Id != Guid.Empty)
                    {
                        filter1.Conditions.Add(new ConditionExpression("objectid", ConditionOperator.Equal, Id));
                    }
                    else
                    {
                        QueryExpression query = new QueryExpression("new_documentmanagementsystem");
                        query.ColumnSet = new ColumnSet(true);
                        EntityCollection Col = _service.RetrieveMultiple(query);
                        if (Col != null && Col.Entities.Count > 0)
                            filter1.Conditions.Add(new ConditionExpression("objectid", ConditionOperator.In, Col.Entities.Select(x => x.Attributes["new_documentmanagementsystemid"]).ToArray()));

                    }

                    //filter1.Conditions.Add(new ConditionExpression("createdon", ConditionOperator.OnOrAfter, Convert.ToDateTime(StartDate).ToString("yyyy-MM-dd")));
                    //filter1.Conditions.Add(new ConditionExpression("createdon", ConditionOperator.OnOrBefore, Convert.ToDateTime(EndDate).ToString("yyyy-MM-dd")));
                    if (PortalUser != null)
                    {
                        filter1.Conditions.Add(new ConditionExpression("subject", ConditionOperator.Like, "%" + PortalUser + "%"));
                    }

                    FilterExpression filter2 = new FilterExpression(LogicalOperator.Or);
                    if (Search != null)
                    {
                        var splitted = Search.Split(' ');
                        for (int i = 0; i < splitted.Count(); i++)
                        {
                            filter2.Conditions.Add(new ConditionExpression("notetext", ConditionOperator.Like, "%" + splitted[i].ToString() + "%"));
                        }
                    }


                    filter.AddFilter(filter1);
                    filter.AddFilter(filter2);
                    Query.Criteria = filter;
                    Query.AddOrder("createdon", OrderType.Descending);
                    Cols = _service.RetrieveMultiple(Query);
                }
                #endregion
            }
            else
            {
                #region Document For Admin
                QueryExpression Query = new QueryExpression(EntityName);
                Query.ColumnSet = new ColumnSet(new[] { "subject", "notetext", "objectid", "annotationid", "createdby", "createdon", "filename", "documentbody", "mimetype" });

                //Query.LinkEntities.Add(new LinkEntity(EntityName, "mhl_portalnoteshared", "annotationid", "mhl_documentnotesid", JoinOperator.Natural));
                //Query.LinkEntities[0].Columns.AddColumns("mhl_approvalstatus", "mhl_approvedby");

                FilterExpression filter = new FilterExpression(LogicalOperator.And);

                FilterExpression filter1 = new FilterExpression(LogicalOperator.And);

                if (Id != Guid.Empty)
                {
                    filter1.Conditions.Add(new ConditionExpression("objectid", ConditionOperator.Equal, Id));
                }
                else
                {
                    QueryExpression query = new QueryExpression("new_documentmanagementsystem");
                    query.ColumnSet = new ColumnSet(true);
                    EntityCollection Col = _service.RetrieveMultiple(query);
                    if (Col != null && Col.Entities.Count > 0)
                        filter1.Conditions.Add(new ConditionExpression("objectid", ConditionOperator.In, Col.Entities.Select(x => x.Attributes["new_documentmanagementsystemid"]).ToArray()));

                }
                //filter1.Conditions.Add(new ConditionExpression("createdon", ConditionOperator.OnOrAfter, Convert.ToDateTime(StartDate).ToString("yyyy-MM-dd")));
                //filter1.Conditions.Add(new ConditionExpression("createdon", ConditionOperator.OnOrBefore, Convert.ToDateTime(EndDate).ToString("yyyy-MM-dd")));
                if (PortalUser != null)
                {
                    filter1.Conditions.Add(new ConditionExpression("subject", ConditionOperator.Like, "%" + PortalUser + "%"));
                }
                FilterExpression filter2 = new FilterExpression(LogicalOperator.Or);
                if (Search != null)
                {
                    var splitted = Search.Split(' ');
                    for (int i = 0; i < splitted.Count(); i++)
                    {
                        filter2.Conditions.Add(new ConditionExpression("notetext", ConditionOperator.Like, "%" + splitted[i].ToString() + "%"));
                    }
                }


                filter.AddFilter(filter1);
                filter.AddFilter(filter2);
                Query.Criteria = filter;
                Query.AddOrder("createdon", OrderType.Descending);
                Cols = _service.RetrieveMultiple(Query);
                #endregion
            }
            if (Cols != null && Cols.Entities.Count > 0)
            {
                for (int i = 0; i < Cols.Entities.Count; i++)
                {
                    ConfigEntity = Cols.Entities[i];

                    if (ConfigEntity != null)
                    {
                        //EntityReference objectid = new EntityReference("incident", (Guid)ConfigEntity.Attributes["objectid"]);
                        BL.NotesData Data = new BL.NotesData();
                        if (ConfigEntity.Attributes.Contains("subject"))
                        {
                            Data.subject = ConfigEntity.Attributes["subject"].ToString();// ToString();
                        }
                        else
                        {
                            Data.subject = null;
                        }
                        if (ConfigEntity.Attributes.Contains("notetext"))
                        {
                            Data.notetext = ConfigEntity.Attributes["notetext"].ToString();
                        }
                        else
                        {
                            Data.notetext = string.Empty;
                        }

                        Data.IncidentId = (Guid)((EntityReference)ConfigEntity.Attributes["objectid"]).Id;

                        Data.annotationid = (Guid)(ConfigEntity.Attributes["annotationid"]);

                        if (ConfigEntity.Attributes.Contains("createdby"))
                        {
                            Data.createdby = ((EntityReference)ConfigEntity.Attributes["createdby"]).Name.ToString();
                        }
                        else
                        {
                            Data.createdby = null;
                        }

                        if (ConfigEntity.Attributes.Contains("createdon"))
                        {
                            Data.createdon = (DateTime)(ConfigEntity.Attributes["createdon"]);
                        }
                        else
                        {
                            Data.createdon = DateTime.Now;
                        }

                        if (ConfigEntity.Attributes.Contains("mimetype"))
                        {
                            Data.mimetype = ConfigEntity.Attributes["mimetype"].ToString();
                        }
                        else
                        {
                            Data.mimetype = null;
                        }

                        if (ConfigEntity.Attributes.Contains("filename"))
                        {
                            Data.filename = ConfigEntity.Attributes["filename"].ToString();
                        }
                        else
                        {
                            Data.filename = null;
                        }

                        if (ConfigEntity.Attributes.Contains("documentbody"))
                        {
                            //using (FileStream fileStream = new FileStream(ConfigEntity.Attributes["filename"].ToString(), FileMode.OpenOrCreate))
                            //{
                            //    byte[] fileContent = Convert.FromBase64String(ConfigEntity.Attributes["documentbody"].ToString());
                            //    fileStream.Write(fileContent, 0, fileContent.Length);
                            //    Data.Attachment = (HttpPostedFileBase)fileContent;
                            //}
                            Data.filecontent = ConfigEntity.Attributes["documentbody"].ToString();
                        }
                        else
                        {
                            Data.filecontent = null;
                        }

                        if (ColsShared.Entities.Count > 0)
                        {
                            foreach (Entity entityshar in ColsShared.Entities)
                            {
                                if (Convert.ToString(entityshar.Attributes["mhl_documentnotesid"]) == ConfigEntity.Attributes["annotationid"].ToString().Replace("{", "").Replace("}", ""))
                                {
                                    AllowApproveFlag = Convert.ToBoolean(entityshar.Attributes["mhl_allowtoapprove"]);
                                }
                            }
                            //var ApprovedFlag = ColsShared.Entities.Where(y => y.Attributes["mhl_documentnotesid"] == ConfigEntity.Attributes["annotationid"].ToString().Replace("{", "").Replace("}", ""));//.Select(x => x.Attributes["mhl_allowtoapprove"]).ToArray();
                            //if (ApprovedFlag[0].ToString().ToLower() == "true")
                            //{
                            //    AllowApproveFlag = true;
                            //}
                        }
                        Data.AllowApprove = AllowApproveFlag;

                        QueryExpression QueryShared = new QueryExpression("mhl_portalnoteshared");
                        QueryShared.ColumnSet = new ColumnSet(true);
                        QueryShared.Criteria.AddCondition(new ConditionExpression("mhl_documentnotesid", ConditionOperator.Equal, new Guid(ConfigEntity.Attributes["annotationid"].ToString()).ToString().Replace("{", "").Replace("}", "")));
                        QueryShared.Criteria.AddCondition(new ConditionExpression("mhl_name", ConditionOperator.NotNull));
                        EntityCollection ColsSharedApprove = _service.RetrieveMultiple(QueryShared);

                        if (ColsSharedApprove.Entities.Count > 0)
                        {
                            Entity ConfigEntityNew = new Entity();
                            ConfigEntityNew = ColsSharedApprove.Entities[0];

                            if (ConfigEntityNew.Attributes.Contains("mhl_name"))
                            {

                                QueryExpression Query = new QueryExpression("new_portaluser");
                                Query.Criteria.AddCondition(new ConditionExpression("new_portaluserid", ConditionOperator.Equal, ((EntityReference)(ConfigEntityNew["mhl_portaluser"])).Id.ToString()));
                                Query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
                                Query.ColumnSet = new ColumnSet(true);
                                EntityCollection UserPortalCol = _service.RetrieveMultiple(Query);

                                if (UserPortalCol != null)
                                {
                                    if (UserPortalCol.Entities.Count > 0)
                                    {
                                        Data.PortalUsercreatedby = UserPortalCol.Entities[0]["new_fullname"].ToString();
                                    }
                                }
                            }


                            if (ConfigEntityNew.Attributes.Contains("mhl_approvedby"))
                            {
                                Data.ApprovedBy = ((EntityReference)(ConfigEntityNew["mhl_approvedby"])).Id.ToString();

                                QueryExpression Query = new QueryExpression("new_portaluser");
                                Query.Criteria.AddCondition(new ConditionExpression("new_portaluserid", ConditionOperator.Equal, Data.ApprovedBy));
                                Query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
                                Query.ColumnSet = new ColumnSet(true);
                                EntityCollection UserPortalCol = _service.RetrieveMultiple(Query);
                                if (UserPortalCol != null)
                                {
                                    if (UserPortalCol.Entities.Count > 0)
                                    {
                                        Data.ApprovedByName = UserPortalCol.Entities[0]["new_fullname"].ToString();
                                    }
                                }
                            }

                            if (ConfigEntityNew.Attributes.Contains("mhl_processedby"))
                            {
                                Data.ProcessedBy = ((EntityReference)(ConfigEntityNew["mhl_processedby"])).Id.ToString();

                                QueryExpression Query = new QueryExpression("new_portaluser");
                                Query.Criteria.AddCondition(new ConditionExpression("new_portaluserid", ConditionOperator.Equal, Data.ProcessedBy));
                                Query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
                                Query.ColumnSet = new ColumnSet(true);
                                EntityCollection UserPortalCol = _service.RetrieveMultiple(Query);
                                if (UserPortalCol != null)
                                {
                                    if (UserPortalCol.Entities.Count > 0)
                                    {
                                        Data.ProcessedByName = UserPortalCol.Entities[0]["new_fullname"].ToString();
                                    }
                                }
                            }

                            if (ConfigEntityNew.Attributes.Contains("mhl_approvalstatus"))
                            {
                                Data.ApprovalStatus = ((OptionSetValue)(ConfigEntityNew["mhl_approvalstatus"])).Value.ToString();
                            }
                        }



                        lst.Add(Data);
                    }
                }
            }
            if (ApprovalStatus != null)
            {
                if (ApprovalStatus != "0")
                {
                    return lst.Where(x => x.ApprovalStatus == ApprovalStatus).ToList();
                }
                else
                {
                    return lst;
                }
            }
            else
            {
                return lst;
            }
        }

        public static EntityCollection GetNameWiseDocumentId(string EntityName, IOrganizationService _Service, Guid UserId, string DocumentType)
        {
            try
            {
                //Note 0 For Active , 1 For Resloved and 2 For Cancel 

                //Guid ConfigEntityId = lst.Select(x => x.EntityId).FirstOrDefault();
                //List<Guid> SharedRecordlst = GetSharedRecord(EntityName, ConfigEntityId, _Service, UserId);
                //if (SharedRecordlst != null && SharedRecordlst.Count > 0)
                //{
                //string[] AllField = lst.Select(x => x.FieldName).ToArray();
                QueryExpression QueryInvoice = new QueryExpression(EntityName);
                QueryInvoice.ColumnSet = new ColumnSet(true);
                if (DocumentType != string.Empty)
                {
                    QueryInvoice.Criteria.AddCondition(new ConditionExpression("new_documentmanagementsystemid", ConditionOperator.Equal, DocumentType));
                }

                //QueryInvoice.AddOrder("createdon", OrderType.Descending);               
                return _Service.RetrieveMultiple(QueryInvoice);
                //}
                //else


            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static List<BL.NotesData> GetAnnotationIdWiseNotesData(string EntityName, IOrganizationService _service, Guid Id)
        {
            List<BL.NotesData> lst = new List<BL.NotesData>();

            Entity ConfigEntity = null;
            EntityCollection Cols = null;

            QueryExpression Query = new QueryExpression(EntityName);
            Query.ColumnSet = new ColumnSet(new[] { "subject", "notetext", "objectid", "annotationid", "createdby", "createdon", "filename", "documentbody", "mimetype" });
            Query.Criteria.AddCondition(new ConditionExpression("annotationid", ConditionOperator.Equal, Id));
            Query.AddOrder("createdon", OrderType.Descending);


            Cols = _service.RetrieveMultiple(Query);

            if (Cols != null && Cols.Entities.Count > 0)

                for (int i = 0; i < Cols.Entities.Count; i++)
                {
                    ConfigEntity = Cols.Entities[i];

                    if (ConfigEntity != null)
                    {
                        //EntityReference objectid = new EntityReference("incident", (Guid)ConfigEntity.Attributes["objectid"]);
                        BL.NotesData Data = new BL.NotesData();
                        if (ConfigEntity.Attributes.Contains("subject"))
                        {
                            Data.subject = ConfigEntity.Attributes["subject"].ToString();// ToString();
                        }
                        else
                        {
                            Data.subject = null;
                        }
                        if (ConfigEntity.Attributes.Contains("notetext"))
                        {
                            Data.notetext = ConfigEntity.Attributes["notetext"].ToString();
                        }
                        else
                        {
                            Data.notetext = string.Empty;
                        }

                        Data.IncidentId = (Guid)((EntityReference)ConfigEntity.Attributes["objectid"]).Id;

                        Data.annotationid = (Guid)(ConfigEntity.Attributes["annotationid"]);

                        if (ConfigEntity.Attributes.Contains("createdby"))
                        {
                            Data.createdby = ((EntityReference)ConfigEntity.Attributes["createdby"]).Name.ToString();
                        }
                        else
                        {
                            Data.createdby = null;
                        }

                        if (ConfigEntity.Attributes.Contains("createdon"))
                        {
                            Data.createdon = (DateTime)(ConfigEntity.Attributes["createdon"]);
                        }
                        else
                        {
                            Data.createdon = DateTime.Now;
                        }

                        if (ConfigEntity.Attributes.Contains("mimetype"))
                        {
                            Data.mimetype = ConfigEntity.Attributes["mimetype"].ToString();
                        }
                        else
                        {
                            Data.mimetype = null;
                        }

                        if (ConfigEntity.Attributes.Contains("filename"))
                        {
                            Data.filename = ConfigEntity.Attributes["filename"].ToString();
                        }
                        else
                        {
                            Data.filename = null;
                        }

                        if (ConfigEntity.Attributes.Contains("documentbody"))
                        {
                            //using (FileStream fileStream = new FileStream(ConfigEntity.Attributes["filename"].ToString(), FileMode.OpenOrCreate))
                            //{
                            //    byte[] fileContent = Convert.FromBase64String(ConfigEntity.Attributes["documentbody"].ToString());
                            //    fileStream.Write(fileContent, 0, fileContent.Length);
                            //    Data.Attachment = (HttpPostedFileBase)fileContent;
                            //}
                            Data.filecontent = ConfigEntity.Attributes["documentbody"].ToString();
                        }
                        else
                        {
                            Data.filecontent = null;
                        }


                        QueryExpression QueryShared = new QueryExpression("mhl_portalnoteshared");
                        QueryShared.ColumnSet = new ColumnSet(true);
                        QueryShared.Criteria.AddCondition(new ConditionExpression("mhl_documentnotesid", ConditionOperator.Equal, new Guid(ConfigEntity.Attributes["annotationid"].ToString()).ToString().Replace("{", "").Replace("}", "")));
                        QueryShared.Criteria.AddCondition(new ConditionExpression("mhl_name", ConditionOperator.NotNull));
                        EntityCollection ColsSharedApprove = _service.RetrieveMultiple(QueryShared);

                        if (ColsSharedApprove.Entities.Count > 0)
                        {
                            Entity ConfigEntityNew = new Entity();
                            ConfigEntityNew = ColsSharedApprove.Entities[0];

                            if (ConfigEntityNew.Attributes.Contains("mhl_name"))
                            {


                                QueryExpression Query1 = new QueryExpression("new_portaluser");
                                Query1.Criteria.AddCondition(new ConditionExpression("new_portaluserid", ConditionOperator.Equal, ((EntityReference)(ConfigEntityNew["mhl_portaluser"])).Id.ToString()));
                                Query1.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
                                Query1.ColumnSet = new ColumnSet(true);
                                EntityCollection UserPortalCol = _service.RetrieveMultiple(Query1);

                                if (UserPortalCol != null)
                                {
                                    if (UserPortalCol.Entities.Count > 0)
                                    {
                                        Data.PortalUsercreatedby = UserPortalCol.Entities[0]["new_fullname"].ToString();
                                    }
                                }
                            }


                            if (ConfigEntityNew.Attributes.Contains("mhl_approvedby"))
                            {
                                Data.ApprovedBy = ((EntityReference)(ConfigEntityNew["mhl_approvedby"])).Id.ToString();

                                QueryExpression Query1 = new QueryExpression("new_portaluser");
                                Query1.Criteria.AddCondition(new ConditionExpression("new_portaluserid", ConditionOperator.Equal, Data.ApprovedBy));
                                Query1.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
                                Query1.ColumnSet = new ColumnSet(true);
                                EntityCollection UserPortalCol = _service.RetrieveMultiple(Query1);
                                if (UserPortalCol != null)
                                {
                                    if (UserPortalCol.Entities.Count > 0)
                                    {
                                        Data.ApprovedByName = UserPortalCol.Entities[0]["new_fullname"].ToString();
                                    }
                                }
                            }
                            if (ConfigEntityNew.Attributes.Contains("mhl_approvalstatus"))
                            {
                                Data.ApprovalStatus = ((OptionSetValue)(ConfigEntityNew["mhl_approvalstatus"])).Value.ToString();
                            }
                        }

                        lst.Add(Data);
                    }
                }



            return lst;
        }

        public static BL.DashboardData GetCounterDocumentNotesData(string EntityName, IOrganizationService _service, Guid Id, string Search, string UserType, string ApprovalStatus, string PortalUser, DateTime StartDate, DateTime EndDate)
        {
            List<BL.NotesData> lst = new List<BL.NotesData>();

            Entity ConfigEntity = null;
            EntityCollection Cols = null;



            if (UserType != "125970001")
            {
                QueryExpression QueryShared = new QueryExpression("mhl_portalnoteshared");
                QueryShared.ColumnSet = new ColumnSet(true);
                QueryShared.Criteria.AddCondition(new ConditionExpression("mhl_portaluser", ConditionOperator.Equal, BL.CurrentUser.Instance.VerifiedUser.UserId));
                EntityCollection ColsShared = _service.RetrieveMultiple(QueryShared);

                var SharedDocument = ColsShared.Entities.Select(x => x.Attributes["mhl_documentnotesid"]).ToArray();

                if (SharedDocument.Length > 0)
                {

                    QueryExpression Query = new QueryExpression(EntityName);
                    Query.ColumnSet = new ColumnSet(new[] { "subject", "notetext", "objectid", "annotationid", "createdby", "createdon", "filename", "documentbody", "mimetype" });
                    FilterExpression filter = new FilterExpression(LogicalOperator.And);

                    FilterExpression filter1 = new FilterExpression(LogicalOperator.And);
                    filter1.Conditions.Add(new ConditionExpression("annotationid", ConditionOperator.In, ColsShared.Entities.Select(x => x.Attributes["mhl_documentnotesid"]).ToArray()));
                    if (Id != Guid.Empty)
                    {
                        filter1.Conditions.Add(new ConditionExpression("objectid", ConditionOperator.Equal, Id));
                    }
                    else
                    {
                        QueryExpression query = new QueryExpression("new_documentmanagementsystem");
                        query.ColumnSet = new ColumnSet(true);
                        EntityCollection Col = _service.RetrieveMultiple(query);
                        if (Col != null && Col.Entities.Count > 0)
                            filter1.Conditions.Add(new ConditionExpression("objectid", ConditionOperator.In, Col.Entities.Select(x => x.Attributes["new_documentmanagementsystemid"]).ToArray()));

                    }
                   // filter1.Conditions.Add(new ConditionExpression("createdon", ConditionOperator.OnOrAfter, Convert.ToDateTime(StartDate).ToString("yyyy-MM-dd")));
                   // filter1.Conditions.Add(new ConditionExpression("createdon", ConditionOperator.OnOrBefore, Convert.ToDateTime(EndDate).ToString("yyyy-MM-dd")));
                    if (PortalUser != null)
                    {
                        filter1.Conditions.Add(new ConditionExpression("subject", ConditionOperator.Like, "%" + PortalUser + "%"));
                    }

                    FilterExpression filter2 = new FilterExpression(LogicalOperator.Or);
                    if (Search != null)
                    {
                        var splitted = Search.Split(' ');
                        for (int i = 0; i < splitted.Count(); i++)
                        {
                            filter2.Conditions.Add(new ConditionExpression("notetext", ConditionOperator.Like, "%" + splitted[i].ToString() + "%"));
                        }
                    }


                    filter.AddFilter(filter1);
                    filter.AddFilter(filter2);
                    Query.Criteria = filter;
                    Query.AddOrder("createdon", OrderType.Descending);
                    Cols = _service.RetrieveMultiple(Query);
                }
            }
            else
            {
                QueryExpression Query = new QueryExpression(EntityName);
                Query.ColumnSet = new ColumnSet(new[] { "subject", "notetext", "objectid", "annotationid", "createdby", "createdon", "filename", "documentbody", "mimetype" });

                //Query.LinkEntities.Add(new LinkEntity(EntityName, "mhl_portalnoteshared", "annotationid", "mhl_documentnotesid", JoinOperator.Natural));
                //Query.LinkEntities[0].Columns.AddColumns("mhl_approvalstatus", "mhl_approvedby");

                FilterExpression filter = new FilterExpression(LogicalOperator.And);

                FilterExpression filter1 = new FilterExpression(LogicalOperator.And);
                if (Id != Guid.Empty)
                {
                    filter1.Conditions.Add(new ConditionExpression("objectid", ConditionOperator.Equal, Id));
                }
                else
                {
                    QueryExpression query = new QueryExpression("new_documentmanagementsystem");
                    query.ColumnSet = new ColumnSet(true);
                    EntityCollection Col = _service.RetrieveMultiple(query);
                    if (Col != null && Col.Entities.Count > 0)
                        filter1.Conditions.Add(new ConditionExpression("objectid", ConditionOperator.In, Col.Entities.Select(x => x.Attributes["new_documentmanagementsystemid"]).ToArray()));

                }
               // filter1.Conditions.Add(new ConditionExpression("createdon", ConditionOperator.OnOrAfter, Convert.ToDateTime(StartDate).ToString("yyyy-MM-dd")));
              //  filter1.Conditions.Add(new ConditionExpression("createdon", ConditionOperator.OnOrBefore, Convert.ToDateTime(EndDate).ToString("yyyy-MM-dd")));
                if (PortalUser != null)
                {
                    filter1.Conditions.Add(new ConditionExpression("subject", ConditionOperator.Like, "%" + PortalUser + "%"));
                }
                FilterExpression filter2 = new FilterExpression(LogicalOperator.Or);
                if (Search != null)
                {
                    var splitted = Search.Split(' ');
                    for (int i = 0; i < splitted.Count(); i++)
                    {
                        filter2.Conditions.Add(new ConditionExpression("notetext", ConditionOperator.Like, "%" + splitted[i].ToString() + "%"));
                    }
                }


                filter.AddFilter(filter1);
                filter.AddFilter(filter2);
                Query.Criteria = filter;
                Query.AddOrder("createdon", OrderType.Descending);
                Cols = _service.RetrieveMultiple(Query);
            }
            if (Cols != null && Cols.Entities.Count > 0)

                for (int i = 0; i < Cols.Entities.Count; i++)
                {
                    ConfigEntity = Cols.Entities[i];

                    if (ConfigEntity != null)
                    {
                        //EntityReference objectid = new EntityReference("incident", (Guid)ConfigEntity.Attributes["objectid"]);
                        BL.NotesData Data = new BL.NotesData();

                        QueryExpression QueryShared = new QueryExpression("mhl_portalnoteshared");
                        QueryShared.ColumnSet = new ColumnSet(true);
                        QueryShared.Criteria.AddCondition(new ConditionExpression("mhl_documentnotesid", ConditionOperator.Equal, new Guid(ConfigEntity.Attributes["annotationid"].ToString()).ToString().Replace("{", "").Replace("}", "")));
                        EntityCollection ColsShared = _service.RetrieveMultiple(QueryShared);

                        if (ColsShared.Entities.Count > 0)
                        {
                            Entity ConfigEntityNew = new Entity();
                            ConfigEntityNew = ColsShared.Entities[0];
                            if (ConfigEntityNew.Attributes.Contains("mhl_approvedby"))
                            {
                                Data.ApprovedBy = ((EntityReference)(ConfigEntityNew["mhl_approvedby"])).Id.ToString();
                            }
                            if (ConfigEntityNew.Attributes.Contains("mhl_approvalstatus"))
                            {
                                Data.ApprovalStatus = ((OptionSetValue)(ConfigEntityNew["mhl_approvalstatus"])).Value.ToString();
                            }
                        }
                        lst.Add(Data);
                    }
                }

            BL.DashboardData DashData = new BL.DashboardData();

            DashData.Approved = lst.Where(x => x.ApprovalStatus == "125970000").Count().ToString();
            DashData.Reject = lst.Where(x => x.ApprovalStatus == "125970001").Count().ToString();
            DashData.Pending = lst.Where(x => x.ApprovalStatus == "125970002").Count().ToString();
            DashData.Processed = lst.Where(x => x.ApprovalStatus == "125970003").Count().ToString();
            //125970000
            //125970001
            //    125970002
            return DashData;

        }

    }
}