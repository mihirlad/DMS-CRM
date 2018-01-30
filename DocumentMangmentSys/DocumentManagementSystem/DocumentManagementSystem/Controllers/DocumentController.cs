using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Tesseract;

namespace ActivityTest.Controllers
{
    public class DocumentController : Controller
    {
        //
        // GET: /Document/

        static IOrganizationService Service = null;
        List<BL.NotesData> lstTroubleTicketNotes = null;
        List<SelectListItem> lstPortalUser = null;
        List<SelectListItem> lstPortalSharedUser = null;
        List<SelectListItem> lstDocumentType = null;

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SaveNotes(BL.NotesData ModelNotes)
        {
            try
            {
                BL.NotesData Mod = new BL.NotesData();
                Mod = ModelNotes;
                TempData["SearchKeyword"] = ModelNotes.SearchKeyword;
                TempData["Documenttyp"] = ModelNotes.Documenttyp;
                TempData["PortalUser"] = ModelNotes.PortalUser;
                TempData["ApprovalStatus"] = ModelNotes.ApprovalStatus;
                TempData["Start_Date"] = ModelNotes.Start_Date;
                TempData["End_Date"] = ModelNotes.End_Date;

                Guid DocumentId = new Guid();

                EntityCollection Col = PortalRepository.GetNameWiseDocumentId("new_documentmanagementsystem", Service, BL.CurrentUser.Instance.VerifiedUser.UserId, Mod.DocumenttypSave);
                if (Col != null && Col.Entities.Count > 0)
                {
                    foreach (Entity enty in Col.Entities)
                    {
                        DocumentId = new Guid(enty.Attributes["new_documentmanagementsystemid"].ToString());
                    }
                }

                if (DocumentId != Guid.Empty)
                {
                    var Ext = System.IO.Path.GetExtension(ModelNotes.Attachment.FileName).ToLower();
                    if (Ext == ".pdf" || Ext == ".doc" || Ext == ".docx" || Ext == ".txt")
                    {
                        Mod.MessageOnView = "";
                    }
                    else
                    {
                        Mod.MessageOnView = "Only Pdf, Word & Text files allowed.";
                    }
                    if (Mod.MessageOnView == "")
                    {
                        if (Service == null)
                            Service = BL.OrganizationUtility.GetCRMService();

                        if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty)
                        {
                            Entity entity = new Entity("annotation");
                            entity["subject"] = " by " + BL.CurrentUser.Instance.VerifiedUser.UserName;
                            //entity["notetext"] = "*Document*" + ModelNotes.notetext;
                            if (ModelNotes.Attachment != null)
                            {
                                if (ModelNotes.Attachment.ContentLength > 0)
                                {
                                    var fname = System.IO.Path.GetFileNameWithoutExtension(ModelNotes.Attachment.FileName) + (DateTime.Now).ToString("ddMMyyhhmmss");
                                    var FExtension = System.IO.Path.GetExtension(ModelNotes.Attachment.FileName);
                                    var fileName = fname + FExtension;//
                                    //var fileName = Path.GetFileName(ModelNotes.Attachment.FileName);// + (DateTime.Now).ToString("dd-MM-yyyy"));

                                    var path = System.IO.Path.Combine(Server.MapPath("~/App_Data/"), fileName);
                                    ModelNotes.Attachment.SaveAs(path);
                                    string notetext = "";
                                    if (ModelNotes.AppendNotesText != null)
                                    {
                                        notetext += "[ " + ModelNotes.AppendNotesText + " ]";
                                    }
                                    if (FExtension.ToLower() == ".pdf")
                                    {
                                        notetext += GetTextFromPDF(path);
                                    }
                                    else if (FExtension.ToLower() == ".doc" || FExtension.ToLower() == ".docx")
                                    {
                                        notetext += GetTextFromWord(path);
                                    }
                                    else if (FExtension.ToLower() == ".txt")
                                    {
                                        notetext += GetTextFromText(path);
                                    }


                                    entity["notetext"] = "*Document*" + notetext;
                                    FileStream stream = new FileStream(path, FileMode.Open);
                                    byte[] byteData = new byte[stream.Length];
                                    stream.Read(byteData, 0, byteData.Length);
                                    stream.Close();
                                    entity["filename"] = fileName;
                                    entity["mimetype"] = ModelNotes.Attachment.ContentType;
                                    entity["documentbody"] = System.Convert.ToBase64String(byteData);

                                    if ((System.IO.File.Exists(path)))
                                    {
                                        System.IO.File.Delete(path);
                                    }
                                }
                            }
                            EntityReference IncidentIdNew = new EntityReference("new_documentmanagementsystem", DocumentId);
                            entity["objectid"] = IncidentIdNew;
                            EntityReference User = new EntityReference("new_portaluser", BL.CurrentUser.Instance.VerifiedUser.UserId);
                            //entity["createdby"] = User.Id;
                            Guid Id = Service.Create(entity);

                            Entity EntityShared = new Entity("mhl_portalnoteshared");
                            OptionSetValue Approved = new OptionSetValue(125970002);
                            EntityShared["mhl_approvalstatus"] = Approved;
                            EntityShared["mhl_name"] = BL.CurrentUser.Instance.VerifiedUser.UserName;
                            EntityShared["mhl_documentnotesid"] = Id.ToString();
                            EntityShared["mhl_portaluser"] = User;
                            EntityShared["mhl_allowtoapprove"] = false;
                            Service.Create(EntityShared);
                            //return View("TicketsNotes");
                        }
                        else
                        {
                            return RedirectToAction("../Activity/Login");
                        }

                    }

                }
                else
                {
                    Mod.MessageOnView = "Please Select Document Type";
                }
                ViewBag.LiDashboard = "class=";
                ViewBag.LiGetAllCasesForIncident = "class=active";
                ViewBag.LiCreateCases = "class=";
                ViewBag.LiPassword = "class=";
                //return Redirect("TicketsNotes?Id=" + ModelNotes.IncidentId);
                // return View("DocumentNotes", Mod);
                return Redirect("DocumentNotes?MessageOnView=" + ModelNotes.MessageOnView);// return Redirect("DocumentNotes");
            }
            catch (Exception)
            {
                return Redirect("~/theme/ErrorPage.html");
            }
        }

        public string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '~' || c == '`' || c == '!' || c == '@' || c == '#' || c == '$' || c == '%' || c == '%' || c == '^' || c == '&' || c == '*' || c == '(' || c == ')' || c == '_' || c == '+' || c == '-' || c == '=' || c == '{' || c == '}' || c == '[' || c == ']' || c == ';' || c == ':' || c == '"' || c == ',' || c == '.' || c == '?' || c == '/' || c == ' ')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private string GetTextFromPDF(string FileSourcePath)
        {
            StringBuilder text = new StringBuilder();
            using (PdfReader reader = new PdfReader(FileSourcePath))
            {
                for (int i = 1; i <= reader.NumberOfPages; i++)
                {
                    text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                }
            }

            return RemoveSpecialCharacters(text.ToString());
        }

        /// <summary>
        /// Reading Text from Word document
        /// </summary>
        /// <returns></returns>
        private string GetTextFromWord(string FileSourcePath)
        {
            StringBuilder text = new StringBuilder();
            Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application();
            object miss = System.Reflection.Missing.Value;
            object path = FileSourcePath;
            object readOnly = true;
            Microsoft.Office.Interop.Word.Document docs = word.Documents.Open(ref path, ref miss, ref readOnly, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss, ref miss);
            for (int i = 0; i < docs.Paragraphs.Count; i++)
            {
                text.Append(" \r\n " + docs.Paragraphs[i + 1].Range.Text.ToString());
            }

            return RemoveSpecialCharacters(text.ToString());
        }

        /// <summary>
        /// Reading text from text files
        /// </summary>
        /// <returns></returns>
        private string GetTextFromText(string FileSourcePath)
        {
            string text = System.IO.File.ReadAllText(FileSourcePath);

            return RemoveSpecialCharacters(text.ToString());
        }

        public FileResult Download(Guid fileId)
        {
            var NewlstTroubleTicketNotes = PortalRepository.GetAnnotationIdWiseNotesData("annotation", Service, fileId);

            // lstTroubleTicketNotes.Where(x => x.annotationid == fileId);

            if (NewlstTroubleTicketNotes.Count() > 0)
            {
                var path = System.IO.Path.Combine(Server.MapPath("~/App_Data/Download/"), NewlstTroubleTicketNotes.ElementAtOrDefault(0).filename);
                using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate))
                {
                    byte[] fileContent = Convert.FromBase64String(NewlstTroubleTicketNotes.ElementAtOrDefault(0).filecontent);
                    fileStream.Write(fileContent, 0, fileContent.Length);
                }

                //byte[] fileBytes = System.IO.File.ReadAllBytes(NewlstTroubleTicketNotes.ElementAtOrDefault(0).filename);
                //var response = new FileContentResult(fileBytes, NewlstTroubleTicketNotes.ElementAtOrDefault(0).mimetype);
                //response.FileDownloadName = "MHL_" + NewlstTroubleTicketNotes.ElementAtOrDefault(0).filename;

                //var FileVirtualPath = "~/App_Data/uploads/" + ImageName;
                return File(path, "application/force-download", System.IO.Path.GetFileName(path));
                //return response;
            }
            else
            {
                return null;
            }

        }

        public List<SelectListItem> GetPortalUser(Guid Id)
        {


            if (Service == null)
                Service = BL.OrganizationUtility.GetCRMService();

            QueryExpression QueryShared = new QueryExpression("mhl_portalnoteshared");
            QueryShared.ColumnSet = new ColumnSet(true);
            QueryShared.Criteria.AddCondition(new ConditionExpression("mhl_documentnotesid", ConditionOperator.Equal, Id.ToString().Replace("{", "").Replace("}", "")));
            EntityCollection ColsShared = Service.RetrieveMultiple(QueryShared);

            var SharedUser = ColsShared.Entities.Select(x => x.FormattedValues["mhl_portaluser"]).ToArray();

            QueryExpression query = new QueryExpression("new_portaluser");
            query.ColumnSet = new ColumnSet("new_name", "new_fullname");
            query.Criteria.AddCondition(new ConditionExpression("new_name", ConditionOperator.NotNull));
            query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            query.Criteria.AddCondition(new ConditionExpression("new_name", ConditionOperator.NotIn, SharedUser));
            query.Criteria.AddCondition(new ConditionExpression("mhl_usertype", ConditionOperator.NotEqual, "125970001"));
            query.AddOrder("new_fullname", OrderType.Ascending);
            EntityCollection Col = Service.RetrieveMultiple(query);
            if (Col != null && Col.Entities.Count > 0)
                lstPortalUser = Col.Entities.Select(x => new SelectListItem { Text = Convert.ToString(x.Attributes["new_fullname"]), Value = x.Id.ToString() }).ToList();


            return lstPortalUser;
        }

        public List<SelectListItem> GetAllPortalUser()
        {

            if (Service == null)
                Service = BL.OrganizationUtility.GetCRMService();

            QueryExpression query = new QueryExpression("new_portaluser");
            query.ColumnSet = new ColumnSet("new_name", "new_fullname");
            query.Criteria.AddCondition(new ConditionExpression("new_name", ConditionOperator.NotNull));
            query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            query.Criteria.AddCondition(new ConditionExpression("mhl_usertype", ConditionOperator.NotEqual, "125970001"));
            query.AddOrder("new_fullname", OrderType.Ascending);
            EntityCollection Col = Service.RetrieveMultiple(query);
            if (Col != null && Col.Entities.Count > 0)
                lstPortalUser = Col.Entities.Select(x => new SelectListItem { Text = Convert.ToString(x.Attributes["new_fullname"]), Value = Convert.ToString(x.Attributes["new_name"]) }).ToList();

            return lstPortalUser;
        }

        public List<SelectListItem> GetSharedPortalUser(Guid Id)
        {

            if (Service == null)
                Service = BL.OrganizationUtility.GetCRMService();

            QueryExpression QueryShared = new QueryExpression("mhl_portalnoteshared");
            QueryShared.ColumnSet = new ColumnSet(true);
            QueryShared.Criteria.AddCondition(new ConditionExpression("mhl_documentnotesid", ConditionOperator.Equal, Id.ToString().Replace("{", "").Replace("}", "")));
            EntityCollection ColsShared = Service.RetrieveMultiple(QueryShared);

            var SharedUser = ColsShared.Entities.Select(x => x.FormattedValues["mhl_portaluser"]).ToArray();

            QueryExpression query = new QueryExpression("new_portaluser");
            query.ColumnSet = new ColumnSet("new_name", "new_fullname");
            query.Criteria.AddCondition(new ConditionExpression("new_name", ConditionOperator.NotNull));
            query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            query.Criteria.AddCondition(new ConditionExpression("new_name", ConditionOperator.In, SharedUser));
            query.Criteria.AddCondition(new ConditionExpression("mhl_usertype", ConditionOperator.NotEqual, "125970001"));
            query.AddOrder("new_fullname", OrderType.Ascending);
            EntityCollection Col = Service.RetrieveMultiple(query);

            if (Col != null && Col.Entities.Count > 0)
                lstPortalSharedUser = Col.Entities.Select(x => new SelectListItem { Text = Convert.ToString(x.Attributes["new_fullname"]), Value = x.Id.ToString() }).ToList();

            return lstPortalSharedUser;
        }

        public List<BL.PortalNotesUser> GetSharedPortalNotesUser(Guid Id)
        {

            if (Service == null)
                Service = BL.OrganizationUtility.GetCRMService();

            List<BL.PortalNotesUser> lstNew = new List<BL.PortalNotesUser>();



            QueryExpression QueryShared = new QueryExpression("mhl_portalnoteshared");
            QueryShared.ColumnSet = new ColumnSet(true);
            QueryShared.Criteria.AddCondition(new ConditionExpression("mhl_documentnotesid", ConditionOperator.Equal, Id.ToString().Replace("{", "").Replace("}", "")));
            EntityCollection ColsShared = Service.RetrieveMultiple(QueryShared);

            var SharedUser = ColsShared.Entities.Select(x => x.FormattedValues["mhl_portaluser"]).ToArray();

            QueryExpression query = new QueryExpression("new_portaluser");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition(new ConditionExpression("new_name", ConditionOperator.NotNull));
            query.Criteria.AddCondition(new ConditionExpression("statecode", ConditionOperator.Equal, 0));
            query.Criteria.AddCondition(new ConditionExpression("new_name", ConditionOperator.In, SharedUser));
            query.Criteria.AddCondition(new ConditionExpression("mhl_usertype", ConditionOperator.NotEqual, "125970001"));
            EntityCollection Col = Service.RetrieveMultiple(query);



            if (Col != null && Col.Entities.Count > 0)
            {
                for (int i = 0; i < Col.Entities.Count; i++)
                {

                    BL.PortalNotesUser Data = new BL.PortalNotesUser();
                    bool UserAllowFlag = false;
                    Guid portalnotesharedid = new Guid();

                    Data.mhl_portaluser = new Guid(Col.Entities[i].Attributes["new_portaluserid"].ToString());
                    //var inner=ColsShared.Entities.Select
                    EntityReference ContactId = new EntityReference("new_portaluser", new Guid("{" + Col.Entities[i].Attributes["new_portaluserid"].ToString() + "}"));
                    //EntityReference ContactIdNew = new EntityReference("new_portaluser", new Guid(Col.Entities[i].Attributes["mhl_portaluser"].ToString()));
                    Data.PortalUserName = Col.Entities[i].Attributes["new_fullname"].ToString();
                    //var SharedUserloop = ColsShared.Entities.Select(x => x.Attributes["mhl_portaluser"]).ToArray();

                    foreach (Entity entity in ColsShared.Entities)
                    {
                        Guid NotesUser = new Guid(((Microsoft.Xrm.Sdk.EntityReference)(entity.Attributes["mhl_portaluser"])).Id.ToString());
                        //foreach (KeyValuePair<String, Object> attribute in entity.Attributes)
                        //{
                        //    Console.WriteLine(attribute.Key + ": " + attribute.Value);
                        //}
                        if (NotesUser == ContactId.Id)
                        {
                            UserAllowFlag = Convert.ToBoolean(entity.Attributes["mhl_allowtoapprove"]);
                            portalnotesharedid = new Guid(entity.Attributes["mhl_portalnotesharedid"].ToString());

                            break;
                        }
                    }

                    //for (int j = 0; j < SharedUserloop.Length; j++)
                    //{
                    //    //Guid MhlUser = new Guid(Convert.ToString(ColsShared.Entities[j].Attributes["mhl_portaluser"]));
                    //    Guid outGuid = new Guid();
                    //    Guid.TryParse(Convert.ToString(SharedUserloop[j]), out outGuid);
                    //    string NotesUser = ((Microsoft.Xrm.Sdk.EntityReference)(SharedUserloop[j])).Id.ToString();
                    //    EntityReference ContactIdNew = new EntityReference("new_portaluser", new Guid(SharedUserloop[j].ToString()));
                    //    if (ContactIdNew.Id == ContactId.Id)
                    //    {
                    //        var F = ColsShared.Entities[j].Attributes["mhl_allowtoapprove"];
                    //    }
                    //}

                    //var ml = ColsShared.Entities.Select(x => x.Attributes["mhl_portaluser"] == ContactId);

                    //var m = ColsShared.Entities.Where(x => x.Attributes["mhl_portaluser"] == Col.Entities[i].Attributes["new_portaluserid"]).Select(y => y.Attributes["mhl_allowtoapprove"]);
                    Data.mhl_portalnotesharedid = portalnotesharedid;
                    Data.mhl_allowtoapprove = UserAllowFlag;
                    lstNew.Add(Data);
                }

            }

            return lstNew;
        }

        public List<BL.DashboardData> GetDashboardData(Guid Id, string Search, string UserType, string ApprovalStatus, string PortalUser, DateTime StartDate, DateTime EndDate)
        {
            try
            {
                if (Service == null)
                    Service = BL.OrganizationUtility.GetCRMService();

                List<BL.DashboardData> lstNew = new List<BL.DashboardData>();

                BL.DashboardData CounterData = PortalRepository.GetCounterDocumentNotesData("annotation", Service, Id, Search, UserType, ApprovalStatus, PortalUser, StartDate, EndDate);

                BL.DashboardData DashData = new BL.DashboardData();

                DashData.Approved = CounterData.Approved;
                DashData.Reject = CounterData.Reject;
                DashData.Pending = CounterData.Pending;
                lstNew.Add(DashData);
                return lstNew;
            }
            catch (Exception)
            {
                return null;
                // return Redirect("~/ErrorPage.html");
            }
        }

        public ActionResult DocumentNotes(BL.NotesData Mod)
        {
            try
            {
                if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty)
                {
                    if (Service == null)
                        Service = BL.OrganizationUtility.GetCRMService();


                    Session["Uname"] = BL.CurrentUser.Instance.VerifiedUser.FullName;//     +" " + BL.CurrentUser.Instance.VerifiedUser.LastName;

                    BL.NotesData Model = new BL.NotesData() { lstPortalUser = null, lstAssignedPortalUser = null, lstDataRow = null, lstHeaderColumn = null, lstDocumentType = null };
                    Guid DocumentId = new Guid();

                    if (TempData["ApprovalStatus"] != null)
                    {
                        Model.ApprovalStatus = TempData["ApprovalStatus"].ToString();
                        Mod.ApprovalStatus = Model.ApprovalStatus;
                    }
                    else
                    {
                        Model.ApprovalStatus = Mod.ApprovalStatus;
                    }

                    if (TempData["Documenttyp"] != null)
                    {
                        Model.Documenttyp = TempData["Documenttyp"].ToString();
                        Mod.Documenttyp = Model.Documenttyp;
                    }
                    else
                    {
                        Model.Documenttyp = Mod.Documenttyp;
                    }

                    if (TempData["PortalUser"] != null)
                    {
                        Model.PortalUser = TempData["PortalUser"].ToString();
                        Mod.PortalUser = Model.PortalUser;
                    }
                    else
                    {
                        Model.PortalUser = Mod.PortalUser;
                    }

                    if (TempData["SearchKeyword"] != null)
                    {
                        Model.SearchKeyword = TempData["SearchKeyword"].ToString();
                        Mod.SearchKeyword = Model.SearchKeyword;
                    }
                    else
                    {
                        Model.SearchKeyword = Mod.SearchKeyword;
                    }

                    if (TempData["Start_Date"] != null)
                    {
                        if (Convert.ToDateTime(TempData["Start_Date"]) != Convert.ToDateTime("1-Jan-0001"))
                        {
                            Model.Start_Date = Convert.ToDateTime(TempData["Start_Date"].ToString());
                            Mod.Start_Date = Model.Start_Date;
                        }
                        else
                        {
                            Mod.Start_Date = DateTime.Now;
                            Model.Start_Date = DateTime.Now;
                        }
                    }
                    else
                    {
                        if (Mod.Start_Date == Convert.ToDateTime("1-Jan-0001"))
                        {

                            Mod.Start_Date = DateTime.Now;
                            Model.Start_Date = DateTime.Now;
                        }
                        else
                        {
                            Model.Start_Date = Mod.Start_Date;
                        }

                    }

                    if (TempData["End_Date"] != null)
                    {
                        if (Convert.ToDateTime(TempData["End_Date"]) != Convert.ToDateTime("1-Jan-0001"))
                        {
                            Model.End_Date = Convert.ToDateTime(TempData["End_Date"].ToString());
                            Mod.End_Date = Model.End_Date;
                        }
                        else
                        {
                            Mod.End_Date = DateTime.Now;
                            Model.End_Date = DateTime.Now;
                        }
                    }
                    else
                    {
                        if (Mod.End_Date == Convert.ToDateTime("1-Jan-0001"))
                        {

                            Mod.End_Date = DateTime.Now;
                            Model.End_Date = DateTime.Now;
                        }
                        else
                        {
                            Model.End_Date = Mod.End_Date;
                        }

                    }


                    TempData["ApprovalStatus"] = null;
                    TempData["Documenttyp"] = null;
                    TempData["PortalUser"] = null;
                    TempData["SearchKeyword"] = null;
                    TempData["Start_Date"] = null;
                    TempData["End_Date"] = null;

                    //BL.CurrentUser.Instance.VerifiedUser.Documenttyp = Mod.Documenttyp;

                    EntityCollection Col = PortalRepository.GetNameWiseDocumentId("new_documentmanagementsystem", Service, BL.CurrentUser.Instance.VerifiedUser.UserId, Mod.Documenttyp);
                    if (Col != null && Col.Entities.Count > 0)
                    {
                        foreach (Entity enty in Col.Entities)
                        {
                            DocumentId = new Guid(enty.Attributes["new_documentmanagementsystemid"].ToString());
                        }
                    }

                    //if (!string.IsNullOrWhiteSpace(Request.QueryString["MessageOnView"]))
                    //{
                    //    if (Request.QueryString["MessageOnView"].ToString() != string.Empty)
                    //    {
                    Model.MessageOnView = Mod.MessageOnView;
                    //}
                    //}
                    //else
                    //{
                    //   Model.MessageOnView = "";
                    //}
                    Model.IncidentId = DocumentId;

                    //if (DocumentId != Guid.Empty)
                    //{
                    lstTroubleTicketNotes = PortalRepository.GetDocumentNotesData("annotation", Service, Model.IncidentId, Mod.SearchKeyword, BL.CurrentUser.Instance.VerifiedUser.LastName, Mod.ApprovalStatus, Mod.PortalUser, Mod.Start_Date, Mod.End_Date);
                    //}
                    if (lstTroubleTicketNotes != null)
                    {
                        if (lstTroubleTicketNotes.Count > 0)
                        {
                            Model.lstNotesData = lstTroubleTicketNotes;
                        }
                    }
                    Model.lstDocumentType = GetDocumentType();
                    //Model.Documenttyp = BL.CurrentUser.Instance.VerifiedUser.Documenttyp;
                    Model.SharedOwnerUser = BL.CurrentUser.Instance.VerifiedUser.LastName;
                    Model.DashData = GetDashboardData(DocumentId, Mod.SearchKeyword, BL.CurrentUser.Instance.VerifiedUser.LastName, Mod.ApprovalStatus, Mod.PortalUser, Mod.Start_Date, Mod.End_Date);
                    Model.lstPortalUser = GetAllPortalUser();


                    //if (string.IsNullOrWhiteSpace(Request.QueryString["Flag"]))
                    //{
                    //Session["SearchKeyword"] = Mod.SearchKeyword;
                    //Session["Documenttyp"] = Mod.Documenttyp;
                    //Session["ApprovalStatus"] = Mod.ApprovalStatus;
                    //Session["PortalUser"] = Mod.PortalUser;

                    //    //Model.Documenttyp = Mod.Documenttyp;
                    //    //Model.SearchKeyword = Mod.SearchKeyword;
                    //    //Model.ApprovalStatus = Mod.ApprovalStatus;
                    //    //Model.PortalUser = Mod.PortalUser;
                    //}
                    //else
                    //{
                    //    if (Request.QueryString["Flag"].ToString() == "1")
                    //    {
                    //        if (Session["SearchKeyword"] != null)
                    //        {
                    //            Model.SearchKeyword = Session["SearchKeyword"].ToString();
                    //        }
                    //        if (Session["Documenttyp"] != null)
                    //        {
                    //            //BL.CurrentUser.Instance.VerifiedUser.Documenttyp = Session["Documenttyp"].ToString();
                    //            Model.Documenttyp = Session["Documenttyp"].ToString();
                    //        }
                    //        if (Session["ApprovalStatus"] != null)
                    //        {
                    //            Model.ApprovalStatus = Session["ApprovalStatus"].ToString();
                    //        }
                    //        if (Session["PortalUser"] != null)
                    //        {
                    //            Model.PortalUser = Session["PortalUser"].ToString();
                    //        }
                    //    }
                    //}

                    //if (ColsShared.Entities.Count > 0)
                    //{
                    //    Entity entity = ColsShared.Entities[0];
                    //    Model.SharedOwnerUser = entity.Attributes["mhl_name"].ToString();
                    //}
                    //else
                    //{
                    //    if (BL.CurrentUser.Instance.VerifiedUser.LastName.ToString() == "125970001")
                    //    {
                    //        Model.SharedOwnerUser = "Admin";
                    //    }
                    //}

                    //Model.SearchKeyword = Mod.SearchKeyword;
                    ViewBag.GetTimeZone = new Func<DateTime, string>(LocalTime);
                    ViewBag.LiDashboard = "class=";
                    ViewBag.LiGetAllCasesForIncident = "class=active";
                    ViewBag.LiCreateCases = "class=";
                    ViewBag.LiPassword = "class=";
                    return View("DocumentNotes", Model);

                }
                else
                {
                    return RedirectToAction("../Activity/Login");
                }
            }
            catch (Exception)
            {
                return Redirect("~/theme/ErrorPage.html");
            }
        }

        public List<SelectListItem> GetDocumentType()
        {

            if (Service == null)
                Service = BL.OrganizationUtility.GetCRMService();

            QueryExpression query = new QueryExpression("new_documentmanagementsystem");
            query.ColumnSet = new ColumnSet(true);
            EntityCollection Col = Service.RetrieveMultiple(query);
            if (Col != null && Col.Entities.Count > 0)
                lstDocumentType = Col.Entities.Select(x => new SelectListItem { Text = Convert.ToString(x.Attributes["new_name"]), Value = x.Id.ToString() }).ToList();

            return lstDocumentType;
        }

        //public ActionResult PartialViewShared(Guid annotationid, List<SelectListItem> lstPortalUsr)
        //{
        //    try
        //    {
        //        BL.NotesData Data = new BL.NotesData();
        //        Data.annotationid = annotationid;
        //        Data.lstPortalUser = lstPortalUsr;
        //        Data.PortalUser = "---Select---";
        //        return PartialView(Data);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        public ActionResult PartialViewShared(BL.NotesData Mod)
        {
            try
            {
                BL.NotesData Data = new BL.NotesData();
                Data.annotationid = Mod.annotationid;
                Data.lstPortalUser = Mod.lstPortalUser;
                Data.PortalUser = Mod.PortalUser;
                return PartialView(Data);
            }
            catch (Exception)
            {
                return Redirect("~/theme/ErrorPage.html");
            }
        }

        public ActionResult DocumentShared(Guid Id)//, string SearchKeyword, string Documenttyp, string PortalUser, string ApprovalStatus, DateTime Start_Date, DateTime End_Date)
        {
            try
            {
                if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty)
                {
                    //TempData["SearchKeyword"] = SearchKeyword;
                    //TempData["Documenttyp"] = Documenttyp;
                    //TempData["PortalUser"] = PortalUser;
                    //TempData["ApprovalStatus"] = ApprovalStatus;
                    //TempData["Start_Date"] = Start_Date;
                    //TempData["End_Date"] = End_Date;

                    BL.NotesData Data = new BL.NotesData();
                    //Data.SearchKeyword = SearchKeyword;
                    //Data.Documenttyp = Documenttyp;
                    //Data.PortalUser = PortalUser;
                    //Data.ApprovalStatus = ApprovalStatus;
                    //Data.Start_Date = Start_Date;
                    //Data.End_Date=End_Date;
                    Data.lstPortalUser = GetPortalUser(Id);
                    Data.lstAssignedPortalUser = GetSharedPortalUser(Id);
                    Data.NotesSharedUser = GetSharedPortalNotesUser(Id);
                    Data.annotationid = Id;
                    lstTroubleTicketNotes = PortalRepository.GetAnnotationIdWiseNotesData("annotation", Service, Id);

                    if (lstTroubleTicketNotes.Count > 0 && lstTroubleTicketNotes != null)
                    {
                        Data.lstNotesData = lstTroubleTicketNotes;
                    }
                    ViewBag.GetTimeZone = new Func<DateTime, string>(LocalTime);
                    ViewBag.LiDashboard = "class=";
                    ViewBag.LiGetAllCasesForIncident = "class=active";
                    ViewBag.LiCreateCases = "class=";
                    ViewBag.LiPassword = "class=";
                    return View(Data);
                }
                else
                {
                    return RedirectToAction("../Activity/Login");
                }
            }
            catch (Exception)
            {
                return Redirect("~/theme/ErrorPage.html");
            }
        }

        [HttpPost]
        public ActionResult SaveShared(BL.NotesData ModelNotes)
        {
            try
            {
                if (Service == null)
                    Service = BL.OrganizationUtility.GetCRMService();

                if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty)
                {
                    EntityReference User = new EntityReference("new_portaluser", new Guid(ModelNotes.PortalUser));

                    Entity EntityShared = new Entity("mhl_portalnoteshared");
                    EntityShared["mhl_name"] = User.Name;
                    EntityShared["mhl_documentnotesid"] = ModelNotes.annotationid.ToString();
                    EntityShared["mhl_portaluser"] = User;
                    EntityShared["mhl_allowtoapprove"] = ModelNotes.AllowApprove;
                    Service.Create(EntityShared);
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
                //return Redirect("TicketsNotes?Id=" + ModelNotes.IncidentId);
                //return Redirect("DocumentNotes");// return Redirect("DocumentNotes");
                return Redirect("DocumentShared?Id=" + ModelNotes.annotationid.ToString());
            }
            catch (Exception)
            {
                return Redirect("~/theme/ErrorPage.html");
            }
        }

        public ActionResult RemovedShared(string annotationid, string PortalUser)
        {
            try
            {
                if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty)
                {
                    QueryExpression QueryShared = new QueryExpression("mhl_portalnoteshared");
                    QueryShared.ColumnSet = new ColumnSet(true);

                    QueryShared.Criteria.AddCondition(new ConditionExpression("mhl_documentnotesid", ConditionOperator.Equal, annotationid));
                    QueryShared.Criteria.AddCondition(new ConditionExpression("mhl_portaluser", ConditionOperator.Equal, PortalUser));
                    EntityCollection cols = Service.RetrieveMultiple(QueryShared);

                    if (cols.Entities.Count > 0)
                    {
                        Entity ConfigEntity = cols.Entities[0];
                        Service.Delete("mhl_portalnoteshared", new Guid(ConfigEntity.Attributes["mhl_portalnotesharedid"].ToString()));
                    }
                    return Redirect("DocumentShared?Id=" + annotationid);
                }
                else
                {
                    return RedirectToAction("../Activity/Login");
                }
            }
            catch (Exception)
            {
                return Redirect("~/theme/ErrorPage.html");
            }
        }
        //[HttpPost]
        //public ActionResult SaveSharedDetail()
        //{
        //    try
        //    {
        //        Guid annotationid = new Guid();
        //        return DocumentShared(annotationid);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
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

        public ActionResult DocumentApproved(Guid Id, string Flag, string SearchKeyword, string Documenttyp, string PortalUser, string ApprovalStatus, DateTime Start_Date, DateTime End_Date)
        {
            try
            {
                if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty)
                {

                    if (Service == null)
                        Service = BL.OrganizationUtility.GetCRMService();

                    TempData["SearchKeyword"] = SearchKeyword;
                    TempData["Documenttyp"] = Documenttyp;
                    TempData["PortalUser"] = PortalUser;
                    TempData["ApprovalStatus"] = ApprovalStatus;
                    TempData["Start_Date"] = Start_Date;
                    TempData["End_Date"] = End_Date;

                    QueryExpression QueryShared = new QueryExpression("mhl_portalnoteshared");
                    QueryShared.ColumnSet = new ColumnSet(true);
                    QueryShared.Criteria.AddCondition(new ConditionExpression("mhl_documentnotesid", ConditionOperator.Equal, Id.ToString().Replace("{", "").Replace("}", "")));
                    QueryShared.Criteria.AddCondition(new ConditionExpression("mhl_name", ConditionOperator.NotNull));
                    EntityCollection ColsShared = Service.RetrieveMultiple(QueryShared);


                    //var SharedUser = ColsShared.Entities.Select();
                    if (ColsShared.Entities.Count > 0)
                    {
                        Entity EntityShared = new Entity("mhl_portalnoteshared");

                        if (Flag == "1")
                        {
                            OptionSetValue Approved = new OptionSetValue(125970000);
                            EntityShared["mhl_approvalstatus"] = Approved;
                        }
                        else
                        {
                            OptionSetValue Approved = new OptionSetValue(125970001);
                            EntityShared["mhl_approvalstatus"] = Approved;
                        }

                        EntityShared["mhl_portalnotesharedid"] = new Guid(ColsShared.Entities[0].Attributes["mhl_portalnotesharedid"].ToString());
                        Guid contactid = new Guid(BL.CurrentUser.Instance.VerifiedUser.UserId.ToString());
                        EntityReference ContactId = new EntityReference("new_portaluser", contactid);
                        EntityShared["mhl_approvedby"] = ContactId;
                        Service.Update(EntityShared);
                    }
                    ViewBag.GetTimeZone = new Func<DateTime, string>(LocalTime);
                    ViewBag.LiDashboard = "class=";
                    ViewBag.LiGetAllCasesForIncident = "class=active";
                    ViewBag.LiCreateCases = "class=";
                    ViewBag.LiPassword = "class=";
                    return Redirect("../DocumentNotes");
                }
                else
                {
                    return RedirectToAction("../Activity/Login");
                }
            }
            catch (Exception)
            {

                return Redirect("~/theme/ErrorPage.html");
            }
        }

        public ActionResult AllowNotesToUser(Guid portalnotesharedid, Guid annotationid, bool Flag)
        {
            try
            {
                if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty)
                {
                    if (Service == null)
                        Service = BL.OrganizationUtility.GetCRMService();

                    if (portalnotesharedid != Guid.Empty && annotationid != Guid.Empty)
                    {
                        Entity EntityShared = new Entity("mhl_portalnoteshared");
                        EntityShared["mhl_portalnotesharedid"] = portalnotesharedid;
                        EntityShared["mhl_allowtoapprove"] = Flag;
                        Service.Update(EntityShared);
                    }
                    ViewBag.GetTimeZone = new Func<DateTime, string>(LocalTime);
                    ViewBag.LiDashboard = "class=";
                    ViewBag.LiGetAllCasesForIncident = "class=active";
                    ViewBag.LiCreateCases = "class=";
                    ViewBag.LiPassword = "class=";
                    return Redirect("DocumentShared?Id=" + annotationid);
                }
                else
                {
                    return RedirectToAction("../Activity/Login");
                }
            }
            catch (Exception)
            {
                return Redirect("~/theme/ErrorPage.html");
            }
        }
    }
}
