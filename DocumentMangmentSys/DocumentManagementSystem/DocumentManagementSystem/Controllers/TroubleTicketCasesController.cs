using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.Xrm.Sdk;
using BL = ClassLibrary1;
using ActivityTest.Utility;
using ActivityTest.Models;
using Microsoft.Xrm.Sdk.Query;
using System.ServiceModel;
using System.Text;
using System.IO;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Messages;



namespace ActivityTest.Controllers
{
    public class TroubleTicketCasesController : Controller
    {
        //
        // GET: /TroubleTicketCases/
        static IOrganizationService Service = null;
        static List<BL.EntityAttributes> lstTroubleTicket = null;
        static List<SelectListItem> lstSubject = null;
        static List<SelectListItem> lstSite = null;
        static List<BL.IncidentData> lstTroubleTicket1 = null;
        static List<BL.NotesData> lstTroubleTicketNotes = null;
        //static int savestatecode = -1;



        public string GetFieldValue(Entity enty, string FieldName, string FieldType)
        {
            if (FieldType.Equals("String") || FieldType.Equals("Memo") || FieldType.Equals("DateTime") || FieldType.Equals("Double")
                || FieldType.Equals("Decimal") || FieldType.Equals("Integer") || FieldType.Equals("Uniqueidentifier"))
                return Convert.ToString(enty.Attributes[FieldName]);
            else if (FieldType.Equals("Lookup") || FieldType.Equals("Customer") || FieldType.Equals("Owner"))
                return ((EntityReference)enty.Attributes[FieldName]).Name;
            else if (FieldType.Equals("State") || FieldType.Equals("Money") || FieldType.Equals("Boolean") || FieldType.Equals("Picklist"))
                return enty.FormattedValues[FieldName];
            else
                return "";
        }



        public ActionResult GetAllCasesForIncident(BL.RecordList Model)
        {
            if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty)
            {
                BL.RecordList AllCases = new BL.RecordList() { lstDataRow = null, lstHeaderColumn = null, statecode = 0, Usercode = 0 };
                if (Service == null)
                    Service = BL.OrganizationUtility.GetCRMService();

                //savestatecode = Model.statecode;

                if (lstTroubleTicket == null || lstTroubleTicket.Count <= 0)
                    lstTroubleTicket = PortalRepository.GetFieldAttribute("incident", Service);
                lstTroubleTicket = lstTroubleTicket.OrderBy(x => x.Index).ToList();
                if (lstTroubleTicket != null && lstTroubleTicket.Count > 0)
                {
                    List<BL.DataRow> lstDataRowValue = new List<BL.DataRow>();
                    EntityCollection Col = PortalRepository.GetEntityRecords("incident", lstTroubleTicket, Service, BL.CurrentUser.Instance.VerifiedUser.UserId, new Guid(BL.CurrentUser.Instance.VerifiedUser.CompanyName), Model);
                    if (Col != null && Col.Entities.Count > 0)
                    {
                        foreach (Entity enty in Col.Entities)
                        {
                            List<string> OneDataRowValue = new List<string>();
                            for (int i = 0; i < lstTroubleTicket.Count; i++)
                            {
                                if (enty.Contains(lstTroubleTicket[i].FieldName) && enty.Attributes[lstTroubleTicket[i].FieldName] != null)
                                    OneDataRowValue.Add(GetFieldValue(enty, lstTroubleTicket[i].FieldName, lstTroubleTicket[i].FieldType));
                                else
                                    OneDataRowValue.Add("");
                            }
                            lstDataRowValue.Add(new BL.DataRow() { DataRoValue = OneDataRowValue.ToArray() });
                        }
                    }
                    if (lstDataRowValue.Count > 0 && lstTroubleTicket.Count > 0)
                        AllCases = new BL.RecordList() { lstDataRow = lstDataRowValue, lstHeaderColumn = lstTroubleTicket, statecode = Model.statecode };
                    else
                        AllCases = new BL.RecordList() { lstDataRow = null, lstHeaderColumn = null };
                }
                ViewBag.LiDashboard = "class=";
                ViewBag.LiGetAllCasesForIncident = "class=active";
                ViewBag.LiCreateCases = "class=";
                ViewBag.LiPassword = "class=";
                return View("TroubleTicket", AllCases);
            }
            else
                return RedirectToAction("../Activity/Login");

        }


        public List<SelectListItem> GetSubject()
        {
            if (lstSubject == null)
            {
                if (Service == null)
                    Service = BL.OrganizationUtility.GetCRMService();

                QueryExpression query = new QueryExpression("subject");
                query.ColumnSet = new ColumnSet("title");
                query.Criteria.AddCondition(new ConditionExpression("title", ConditionOperator.NotNull));
                EntityCollection Col = Service.RetrieveMultiple(query);
                if (Col != null && Col.Entities.Count > 0)
                    lstSubject = Col.Entities.Select(x => new SelectListItem { Text = Convert.ToString(x.Attributes["title"]), Value = x.Id.ToString() }).ToList();
            }
            return lstSubject;
        }

        public List<SelectListItem> GetSiteOnCustomer(string CustId)
        {
            if (lstSite == null)
            {
                if (Service == null)
                    Service = BL.OrganizationUtility.GetCRMService();

                QueryExpression query = new QueryExpression("new_projectsite");
                query.ColumnSet = new ColumnSet("new_name");
                query.Criteria.AddCondition(new ConditionExpression("new_account", ConditionOperator.Equal, CustId));
                EntityCollection Col = Service.RetrieveMultiple(query);
                if (Col != null && Col.Entities.Count > 0)
                    lstSite = Col.Entities.Select(x => new SelectListItem { Text = Convert.ToString(x.Attributes["new_name"]), Value = x.Id.ToString() }).ToList();
            }
            return lstSite;
        }

        public ActionResult CreateCases()
        {
            if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty)
            {
                //Guid customerid = new Guid(BL.CurrentUser.Instance.VerifiedUser.CompanyName);
                //EntityReference CustomerId = new EntityReference("contact", customerid);
                lstSite = GetSiteOnCustomer(BL.CurrentUser.Instance.VerifiedUser.CompanyName);
                BL.TicketData Model;
                if (lstSite.Count > 0)
                {

                    Model = new BL.TicketData() { lstSubject = GetSubject(), lstSite = GetSiteOnCustomer(BL.CurrentUser.Instance.VerifiedUser.CompanyName) };
                }
                else
                {
                    Model = new BL.TicketData() { lstSubject = GetSubject() };
                }
                ViewBag.LiDashboard = "class=";
                ViewBag.LiGetAllCasesForIncident = "class=";
                ViewBag.LiCreateCases = "class=active";
                ViewBag.LiPassword = "class=";
                return View("CreateCases", Model);
            }
            else
            {
                return RedirectToAction("../Activity/Login");
            }

        }

        public ActionResult SaveCases(BL.TicketData Model)
        {
            try
            {

                if (Service == null)
                    Service = BL.OrganizationUtility.GetCRMService();

                if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty)
                {
                    BL.User us = new BL.User();

                    //QueryExpression query = new QueryExpression("contact");
                    //query.ColumnSet = new ColumnSet("contactid", "parentcustomerid");
                    ////query.Criteria.AddCondition(new ConditionExpression("new_account", ConditionOperator.Equal, CustId));
                    //EntityCollection Col = Service.RetrieveMultiple(query);

                    // Entity entity = context.InputParameters["account"] as Entity;

                    if (Model.incidentid == null || Model.incidentid == new Guid("00000000-0000-0000-0000-000000000000"))
                    {

                        Entity entity = new Entity("incident");
                        //entity.Id = "";
                        entity.LogicalName = "incident";
                        //entity.Attributes["incidentid"] = ;
                        entity.Attributes["title"] = Model.title;
                        Guid subjectid = new Guid(Model.subjectid);
                        EntityReference SubjectId = new EntityReference("subject", subjectid);
                        entity.Attributes["subjectid"] = SubjectId;
                        entity.Attributes["description"] = Model.description;
                        Guid customerid = new Guid(BL.CurrentUser.Instance.VerifiedUser.CompanyName);
                        EntityReference CustomerId = new EntityReference("account", customerid);
                        entity.Attributes["customerid"] = CustomerId;

                        Guid sitename = new Guid(Model.new_name);
                        EntityReference SiteName = new EntityReference("new_projectsite", sitename);
                        entity.Attributes["new_sitename"] = SiteName;
                        Guid contactid = new Guid(BL.CurrentUser.Instance.VerifiedUser.UserId.ToString());
                        EntityReference ContactId = new EntityReference("contact", contactid);
                        entity.Attributes["primarycontactid"] = ContactId;//new_sitename
                        entity.Attributes["createdby"] = ContactId;//new_sitename 
                        var incId = Service.Create(entity);

                        #region SaveNotes
                        Entity entity_notes = new Entity("annotation");
                        entity_notes["subject"] = " by " + BL.CurrentUser.Instance.VerifiedUser.UserName;
                        entity_notes["notetext"] = "*WEB*" + Model.description;
                        if (Model.Attachment != null)
                        {
                            if (Model.Attachment.ContentLength > 0)
                            {
                                var fname = Path.GetFileNameWithoutExtension(Model.Attachment.FileName) + (DateTime.Now).ToString("ddMMyyhhmmss");
                                var FExtension = Path.GetExtension(Model.Attachment.FileName);
                                var fileName = fname + FExtension;//
                                //var fileName = Path.GetFileName(ModelNotes.Attachment.FileName);// + (DateTime.Now).ToString("dd-MM-yyyy"));

                                var path = Path.Combine(Server.MapPath("~/App_Data/"), fileName);
                                Model.Attachment.SaveAs(path);
                                FileStream stream = new FileStream(path, FileMode.Open);
                                byte[] byteData = new byte[stream.Length];
                                stream.Read(byteData, 0, byteData.Length);
                                stream.Close();
                                entity_notes["filename"] = fileName;
                                entity_notes["mimetype"] = Model.Attachment.ContentType;
                                entity_notes["documentbody"] = System.Convert.ToBase64String(byteData);
                            }
                        }
                        EntityReference IncidentIdNew = new EntityReference("incident", incId);
                        entity_notes["objectid"] = IncidentIdNew;
                        EntityReference User = new EntityReference("contact", BL.CurrentUser.Instance.VerifiedUser.UserId);
                        //entity["createdby"] = User.Id;
                        Service.Create(entity_notes);
                        #endregion

                        Model.PageMessage = "Case Successfully Created";
                    }
                    else
                    {
                        Entity entity = new Entity("incident");
                        //entity.Id = "";
                        entity.LogicalName = "incident";
                        entity.Attributes["incidentid"] = Model.incidentid;
                        entity.Attributes["title"] = Model.title;
                        Guid subjectid = new Guid(Model.subjectid);
                        EntityReference SubjectId = new EntityReference("subject", subjectid);
                        entity.Attributes["subjectid"] = SubjectId;
                        entity.Attributes["description"] = Model.description;
                        Guid customerid = new Guid(BL.CurrentUser.Instance.VerifiedUser.CompanyName);
                        EntityReference CustomerId = new EntityReference("account", customerid);
                        entity.Attributes["customerid"] = CustomerId;

                        Guid sitename = new Guid(Model.new_name);
                        EntityReference SiteName = new EntityReference("new_projectsite", sitename);
                        entity.Attributes["new_sitename"] = SiteName;
                        Guid contactid = new Guid(BL.CurrentUser.Instance.VerifiedUser.UserId.ToString());
                        EntityReference ContactId = new EntityReference("contact", contactid);
                        entity.Attributes["primarycontactid"] = ContactId;//new_sitename
                        Service.Update(entity);

                        Model.PageMessage = "Case Successfully Updated";
                    }



                }

                //EditCases(Model.incidentid);
                return RedirectToAction("GetAllCasesForIncident");

            }
            catch (Exception ex)
            {
                return RedirectToAction("../Activity/Login");
            }

        }

        [HttpGet]
        public ActionResult EditCases(Guid id)
        {

            if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty)
            {
                BL.RecordList AllCases = new BL.RecordList() { lstData = null };
                lstSite = GetSiteOnCustomer(BL.CurrentUser.Instance.VerifiedUser.CompanyName);
                BL.TicketData Model;
                if (lstSite.Count > 0)
                {

                    Model = new BL.TicketData() { lstSubject = GetSubject(), lstSite = GetSiteOnCustomer(BL.CurrentUser.Instance.VerifiedUser.CompanyName) };
                }
                else
                {
                    Model = new BL.TicketData() { lstSubject = GetSubject() };
                }

                lstTroubleTicket1 = PortalRepository.GetIdWiseIncidentData("incident", Service, id);
                if (lstTroubleTicket1.Count > 0)
                    AllCases = new BL.RecordList() { lstData = lstTroubleTicket1 };
                else
                    AllCases = new BL.RecordList() { lstData = null };


                Model.incidentid = id;
                Model.description = AllCases.lstData[0].description;


                EntityReference CustomerId = new EntityReference("account", AllCases.lstData[0].customerid);
                Model.customerid = CustomerId.Id.ToString();


                EntityReference SiteName = new EntityReference("new_projectsite", AllCases.lstData[0].new_sitename);
                Model.new_name = SiteName.Id.ToString();

                EntityReference ContactId = new EntityReference("contact", AllCases.lstData[0].primarycontactid);
                Model.primarycontactid = ContactId.Id.ToString();

                EntityReference SubjectId = new EntityReference("subject", AllCases.lstData[0].subjectid);
                Model.subjectid = SubjectId.Id.ToString();
                Model.title = AllCases.lstData[0].title;

                Model.PageMessage = "";

                return View("CreateCases", Model);
            }
            else
            {
                return RedirectToAction("../Activity/Login");
            }

        }

        [HttpGet]
        public ActionResult ReopenCase(Guid id)
        {
            try
            {
                var cols = new ColumnSet(new[] { "statecode", "statuscode" });

                //Check if it is Inactive or not
                var entity = Service.Retrieve("incident", id, cols);

                if (entity != null && entity.GetAttributeValue<OptionSetValue>("statecode").Value == 1)
                {
                    //StateCode = 0 and StatusCode = 1 for activating Account or Contact
                    SetStateRequest setStateRequest = new SetStateRequest()
                    {
                        EntityMoniker = new EntityReference
                        {
                            Id = id,
                            LogicalName = "incident",
                        },
                        State = new OptionSetValue(0),
                        Status = new OptionSetValue(1)
                    };
                    Service.Execute(setStateRequest);
                }

                //Entity entity = new Entity("incident");
                ////entity.Id = "";
                //entity.LogicalName = "incident";
                //entity.Attributes["incidentid"] = id;
                //entity.Attributes["statecode"] = "Active";
                //entity.Attributes["statuscode"] = "In Progress";
                //Service.Update(entity);

                return RedirectToAction("GetAllCasesForIncident");

            }
            catch (Exception)
            {
                return RedirectToAction("../Activity/Login");
            }
        }

        public ActionResult TicketsNotes()
        {
            try
            {
                if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty && !string.IsNullOrWhiteSpace(Request.QueryString["Id"]))
                {

                    BL.NotesData Model = new BL.NotesData() { lstDataRow = null, lstHeaderColumn = null };
                    Model.IncidentId = new Guid(Request.QueryString["Id"].ToString());

                    #region Incident Data
                    // BL.NotesData AllCases = new BL.NotesData() { lstDataRow = null, lstHeaderColumn = null };
                    if (Service == null)
                        Service = BL.OrganizationUtility.GetCRMService();

                    //savestatecode = Model.statecode;

                    if (lstTroubleTicket == null || lstTroubleTicket.Count <= 0)
                        lstTroubleTicket = PortalRepository.GetFieldAttribute("incident", Service);
                    lstTroubleTicket = lstTroubleTicket.OrderBy(x => x.Index).ToList();
                    if (lstTroubleTicket != null && lstTroubleTicket.Count > 0)
                    {
                        List<BL.DataRow> lstDataRowValue = new List<BL.DataRow>();
                        EntityCollection Col = PortalRepository.GetIdWiseEntityRecords("incident", lstTroubleTicket, Service, BL.CurrentUser.Instance.VerifiedUser.UserId, new Guid(BL.CurrentUser.Instance.VerifiedUser.CompanyName), Model.IncidentId);
                        if (Col != null && Col.Entities.Count > 0)
                        {
                            foreach (Entity enty in Col.Entities)
                            {
                                List<string> OneDataRowValue = new List<string>();
                                for (int i = 0; i < lstTroubleTicket.Count; i++)
                                {
                                    if (enty.Contains(lstTroubleTicket[i].FieldName) && enty.Attributes[lstTroubleTicket[i].FieldName] != null)
                                        OneDataRowValue.Add(GetFieldValue(enty, lstTroubleTicket[i].FieldName, lstTroubleTicket[i].FieldType));
                                    else
                                        OneDataRowValue.Add("");
                                }
                                lstDataRowValue.Add(new BL.DataRow() { DataRoValue = OneDataRowValue.ToArray() });
                            }
                        }
                        if (lstDataRowValue.Count > 0 && lstTroubleTicket.Count > 0)
                            Model = new BL.NotesData() { lstDataRow = lstDataRowValue, lstHeaderColumn = lstTroubleTicket, IncidentId = new Guid(Request.QueryString["Id"].ToString()) };
                        else
                            Model = new BL.NotesData() { lstDataRow = null, lstHeaderColumn = null };
                    }
                    #endregion

                    //if (lstTroubleTicketNotes == null || lstTroubleTicketNotes.Count <= 0)
                    lstTroubleTicketNotes = PortalRepository.GetNotesData("annotation", Service, Model.IncidentId);

                    if (lstTroubleTicketNotes.Count > 0 && lstTroubleTicketNotes != null)
                    {
                        Model.lstNotesData = lstTroubleTicketNotes;
                        //FileStream stream = new FileStream(Model.lstNotesData.filename, FileMode.OpenOrCreate);
                        //byte[] byteData = new byte[stream.Length];
                        //stream.Write(byteData, 0, byteData.Length);
                        //stream.Close();
                        //System.Convert.FromBase64CharArray(byteData);
                    }
                    //TimeZoneInfo cstZone = TimeZoneInfo.(190);
                    //DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);

                    ViewBag.GetTimeZone = new Func<DateTime, string>(LocalTime);
                    ViewBag.LiDashboard = "class=";
                    ViewBag.LiGetAllCasesForIncident = "class=active";
                    ViewBag.LiCreateCases = "class=";
                    ViewBag.LiPassword = "class=";
                    return View("TicketsNotes", Model);

                }
                else
                {
                    return RedirectToAction("../Activity/Login");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("../Activity/Login");
            }
        }

        public ActionResult ViewTicketsNotes()
        {
            try
            {
                if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty && !string.IsNullOrWhiteSpace(Request.QueryString["Id"]))
                {

                    BL.NotesData Model = new BL.NotesData();

                    Model.IncidentId = new Guid(Request.QueryString["Id"].ToString());

                    //if (lstTroubleTicketNotes == null || lstTroubleTicketNotes.Count <= 0)
                    lstTroubleTicketNotes = PortalRepository.GetNotesData("annotation", Service, Model.IncidentId);

                    if (lstTroubleTicketNotes.Count > 0 && lstTroubleTicketNotes != null)
                    {
                        Model.lstNotesData = lstTroubleTicketNotes;
                        //FileStream stream = new FileStream(Model.lstNotesData.filename, FileMode.OpenOrCreate);
                        //byte[] byteData = new byte[stream.Length];
                        //stream.Write(byteData, 0, byteData.Length);
                        //stream.Close();
                        //System.Convert.FromBase64CharArray(byteData);
                    }
                    //TimeZoneInfo cstZone = TimeZoneInfo.(190);
                    //DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, cstZone);

                    ViewBag.GetTimeZone = new Func<DateTime, string>(LocalTime);
                    ViewBag.LiDashboard = "class=";
                    ViewBag.LiGetAllCasesForIncident = "class=active";
                    ViewBag.LiCreateCases = "class=";
                    ViewBag.LiPassword = "class=";
                    return View("ViewTicketsNotes", Model);

                }
                else
                {
                    return RedirectToAction("../Activity/Login");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("../Activity/Login");
            }
        }

        public ActionResult SaveNotes(BL.NotesData ModelNotes)
        {
            try
            {

                if (Service == null)
                    Service = BL.OrganizationUtility.GetCRMService();

                if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty)
                {
                    Entity entity = new Entity("annotation");
                    entity["subject"] = " by " + BL.CurrentUser.Instance.VerifiedUser.UserName;
                    entity["notetext"] = "*WEB*" + ModelNotes.notetext;
                    if (ModelNotes.Attachment != null)
                    {
                        if (ModelNotes.Attachment.ContentLength > 0)
                        {
                            var fname = Path.GetFileNameWithoutExtension(ModelNotes.Attachment.FileName) + (DateTime.Now).ToString("ddMMyyhhmmss");
                            var FExtension = Path.GetExtension(ModelNotes.Attachment.FileName);
                            var fileName = fname + FExtension;//
                            //var fileName = Path.GetFileName(ModelNotes.Attachment.FileName);// + (DateTime.Now).ToString("dd-MM-yyyy"));

                            var path = Path.Combine(Server.MapPath("~/App_Data/"), fileName);
                            ModelNotes.Attachment.SaveAs(path);
                            FileStream stream = new FileStream(path, FileMode.Open);
                            byte[] byteData = new byte[stream.Length];
                            stream.Read(byteData, 0, byteData.Length);
                            stream.Close();
                            entity["filename"] = fileName;
                            entity["mimetype"] = ModelNotes.Attachment.ContentType;
                            entity["documentbody"] = System.Convert.ToBase64String(byteData);
                        }
                    }
                    EntityReference IncidentIdNew = new EntityReference("incident", ModelNotes.IncidentId);
                    entity["objectid"] = IncidentIdNew;
                    EntityReference User = new EntityReference("contact", BL.CurrentUser.Instance.VerifiedUser.UserId);
                    //entity["createdby"] = User.Id;
                    Service.Create(entity);

                    //return View("TicketsNotes");
                }
                else
                {
                    return RedirectToAction("../Activity/Login");
                }
                ViewBag.LiDashboard = "class=";
                ViewBag.LiGetAllCasesForIncident = "class=active";
                ViewBag.LiCreateCases = "class=";
                ViewBag.LiPassword = "class=";
                return Redirect("TicketsNotes?Id=" + ModelNotes.IncidentId);
            }
            catch (Exception)
            {
                return RedirectToAction("../Activity/Login");
            }
        }

        public FileResult Download(Guid fileId)
        {
            var NewlstTroubleTicketNotes = lstTroubleTicketNotes.Where(x => x.annotationid == fileId);

            if (NewlstTroubleTicketNotes.Count() > 0)
            {
                var path = Path.Combine(Server.MapPath("~/App_Data/Download/"), NewlstTroubleTicketNotes.ElementAtOrDefault(0).filename);
                using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    byte[] fileContent = Convert.FromBase64String(NewlstTroubleTicketNotes.ElementAtOrDefault(0).filecontent);
                    fileStream.Write(fileContent, 0, fileContent.Length);
                }

                //byte[] fileBytes = System.IO.File.ReadAllBytes(NewlstTroubleTicketNotes.ElementAtOrDefault(0).filename);
                //var response = new FileContentResult(fileBytes, NewlstTroubleTicketNotes.ElementAtOrDefault(0).mimetype);
                //response.FileDownloadName = "MHL_" + NewlstTroubleTicketNotes.ElementAtOrDefault(0).filename;

                //var FileVirtualPath = "~/App_Data/uploads/" + ImageName;
                return File(path, "application/force-download", Path.GetFileName(path));
                //return response;
            }
            else
            {
                return null;
            }

        }

        protected string LocalTime(DateTime utcTime)//Mihir Lad (19-Apr-2017)
        {
            //string utcTime = Contact.Adx_TimeZone.Value.ToString();
            if (BL.CurrentUser.Instance.VerifiedUser.timezone != "")
            {
                TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById(BL.CurrentUser.Instance.VerifiedUser.timezone);
                DateTime cstTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, cstZone);

                return cstTime.ToString("MM'/'dd'/'yyyy h:mm:ss tt");
            }
            else
            {
                return utcTime.ToString("MM'/'dd'/'yyyy h:mm:ss tt");
            }

        }

        public ActionResult TicketsEscalationNotes()
        {
            try
            {
                if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty && !string.IsNullOrWhiteSpace(Request.QueryString["Id"]))
                {

                    BL.NotesData Model = new BL.NotesData() { lstDataRow = null, lstHeaderColumn = null };

                    Model.IncidentId = new Guid(Request.QueryString["Id"].ToString());


                    #region Incident Data
                    // BL.NotesData AllCases = new BL.NotesData() { lstDataRow = null, lstHeaderColumn = null };
                    if (Service == null)
                        Service = BL.OrganizationUtility.GetCRMService();

                    //savestatecode = Model.statecode;

                    if (lstTroubleTicket == null || lstTroubleTicket.Count <= 0)
                        lstTroubleTicket = PortalRepository.GetFieldAttribute("incident", Service);
                    lstTroubleTicket = lstTroubleTicket.OrderBy(x => x.Index).ToList();
                    if (lstTroubleTicket != null && lstTroubleTicket.Count > 0)
                    {
                        List<BL.DataRow> lstDataRowValue = new List<BL.DataRow>();
                        EntityCollection Col = PortalRepository.GetIdWiseEntityRecords("incident", lstTroubleTicket, Service, BL.CurrentUser.Instance.VerifiedUser.UserId, new Guid(BL.CurrentUser.Instance.VerifiedUser.CompanyName), Model.IncidentId);
                        if (Col != null && Col.Entities.Count > 0)
                        {
                            foreach (Entity enty in Col.Entities)
                            {
                                List<string> OneDataRowValue = new List<string>();
                                for (int i = 0; i < lstTroubleTicket.Count; i++)
                                {
                                    if (enty.Contains(lstTroubleTicket[i].FieldName) && enty.Attributes[lstTroubleTicket[i].FieldName] != null)
                                        OneDataRowValue.Add(GetFieldValue(enty, lstTroubleTicket[i].FieldName, lstTroubleTicket[i].FieldType));
                                    else
                                        OneDataRowValue.Add("");
                                }
                                lstDataRowValue.Add(new BL.DataRow() { DataRoValue = OneDataRowValue.ToArray() });
                            }
                        }
                        if (lstDataRowValue.Count > 0 && lstTroubleTicket.Count > 0)
                            Model = new BL.NotesData() { lstDataRow = lstDataRowValue, lstHeaderColumn = lstTroubleTicket, IncidentId = new Guid(Request.QueryString["Id"].ToString()) };
                        else
                            Model = new BL.NotesData() { lstDataRow = null, lstHeaderColumn = null };
                    }
                    #endregion

                    lstTroubleTicketNotes = PortalRepository.GetNotesData("annotation", Service, Model.IncidentId);

                    if (lstTroubleTicketNotes.Count > 0 && lstTroubleTicketNotes != null)
                    {
                        Model.lstNotesData = lstTroubleTicketNotes;
                    }
                    ViewBag.GetTimeZone = new Func<DateTime, string>(LocalTime);
                    ViewBag.LiDashboard = "class=";
                    ViewBag.LiGetAllCasesForIncident = "class=active";
                    ViewBag.LiCreateCases = "class=";
                    ViewBag.LiPassword = "class=";
                    return View("TicketsEscalationNotes", Model);

                }
                else
                {
                    return RedirectToAction("../Activity/Login");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("../Activity/Login");
            }
        }

        public ActionResult EscalationSaveNotes(BL.NotesData ModelNotes)
        {
            try
            {

                if (Service == null)
                    Service = BL.OrganizationUtility.GetCRMService();

                if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty)
                {
                    Entity entity = new Entity("annotation");
                    entity["subject"] = " escalation From " + BL.CurrentUser.Instance.VerifiedUser.UserName;
                    entity["notetext"] = "*WEB*" + ModelNotes.notetext;
                    if (ModelNotes.Attachment != null)
                    {
                        if (ModelNotes.Attachment.ContentLength > 0)
                        {
                            var fname = Path.GetFileNameWithoutExtension(ModelNotes.Attachment.FileName) + (DateTime.Now).ToString("ddMMyyhhmmss");
                            var FExtension = Path.GetExtension(ModelNotes.Attachment.FileName);
                            var fileName = fname + FExtension;// Path.GetFileName(ModelNotes.Attachment.FileName);// + (DateTime.Now).ToString("ddMMyyhhmmss"));

                            var path = Path.Combine(Server.MapPath("~/App_Data/"), fileName);
                            ModelNotes.Attachment.SaveAs(path);
                            FileStream stream = new FileStream(path, FileMode.Open);
                            byte[] byteData = new byte[stream.Length];
                            stream.Read(byteData, 0, byteData.Length);
                            stream.Close();
                            entity["filename"] = fileName;
                            entity["mimetype"] = ModelNotes.Attachment.ContentType;
                            entity["documentbody"] = System.Convert.ToBase64String(byteData);
                        }
                    }
                    EntityReference IncidentIdNew = new EntityReference("incident", ModelNotes.IncidentId);
                    entity["objectid"] = IncidentIdNew;
                    EntityReference User = new EntityReference("contact", BL.CurrentUser.Instance.VerifiedUser.UserId);
                    //entity["createdby"] = User.Id;
                    Service.Create(entity);

                    //return View("TicketsNotes");
                }
                else
                {
                    return RedirectToAction("../Activity/Login");
                }
                ViewBag.LiDashboard = "class=";
                ViewBag.LiGetAllCasesForIncident = "class=active";
                ViewBag.LiCreateCases = "class=";
                ViewBag.LiPassword = "class=";
                return Redirect("TicketsEscalationNotes?Id=" + ModelNotes.IncidentId);
            }
            catch (Exception)
            {
                return RedirectToAction("../Activity/Login");
            }
        }
    }
}
