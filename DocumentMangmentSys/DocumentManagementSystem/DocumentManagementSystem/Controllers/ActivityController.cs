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
using Microsoft.Xrm.Client;
using Tesseract;
//using AL = OCRApplication;


namespace ActivityTest.Controllers
{
    public class ActivityController : Controller
    {
        List<SelectListItem> lstDocumentType = null;

        static IOrganizationService Service = null;

        public ActionResult Index()
        {
            return View();
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

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            #region Image To Text Test
            //var testImagePath = "~/App_Data/IMG-20171220-WA0005.jpg";
            //var dataPath = "\\App_Data\\test.txt";

            //try
            //{
            //    using (var tEngine = new TesseractEngine(dataPath, "eng", EngineMode.Default)) //creating the tesseract OCR engine with English as the language
            //    {
            //        using (var img = Pix.LoadFromFile(testImagePath)) // Load of the image file from the Pix object which is a wrapper for Leptonica PIX structure
            //        {
            //            using (var page = tEngine.Process(img)) //process the specified image
            //            {
            //                var text = page.GetText(); //Gets the image's content as plain text.
            //                Console.WriteLine(text); //display the text
            //                Console.WriteLine(page.GetMeanConfidence()); //Get's the mean confidence that as a percentage of the recognized text.
            //                Console.ReadKey();
            //            }
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("Unexpected Error: " + e.Message);
            //} 
            #endregion

            //return Redirect("~/theme/ErrorPage.html");


            BL.User Data = new BL.User();
            Data.lstDocumentType = null;
            Data.Documenttyp = null;
            //string Filapath = @"C:\Users\Sai\Desktop\nuance\1.png";
            //AL.Class1 obj = new AL.Class1();

            //var result = obj.GetResult(Filapath); 


            ViewBag.ReturnUrl = returnUrl;
            if (!HttpContext.User.Identity.IsAuthenticated || BL.CurrentUser.Instance == null)
                return View(Data);

            return View(Data);
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(BL.User _User, string returnUrl)
        {


            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
            1, // Ticket Version
            _User.UserName, // username associated with ticket
            DateTime.Now, // Date/Time issued
            DateTime.Now.AddMinutes(30), // Date/Time to expire 
            false,// Cookie is persistant or not 
            _User.Documenttyp,// we can also store user role
            FormsAuthentication.FormsCookiePath);// Path cookie valid for


            if (_User.UserName != null)
            {
                if (_User.Password != null)
                {
                    Service = BL.OrganizationUtility.GetCRMService();
                    BL.User _verifiedUser = PortalRepository.AuthenticateUser(_User.UserName, _User.Password, _User.Documenttyp, Service);
                    if (_verifiedUser != null)
                    {
                        if (_verifiedUser.UserId != Guid.Empty)
                        {

                            string hashTicket = FormsAuthentication.Encrypt(ticket);
                            HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, hashTicket);
                            Response.Cookies.Add(authCookie);

                            BL.CurrentUser _cuser = new BL.CurrentUser(_verifiedUser);
                            BL.CurrentUser.Instance.VerifiedUser = _verifiedUser;
                            string name = BL.CurrentUser.Instance.VerifiedUser.UserName;
                            if (Url.IsLocalUrl(returnUrl) && !string.IsNullOrEmpty(returnUrl))
                                return Redirect(returnUrl);

                            return RedirectToAction("DocumentNotes", "Document");

                        }
                    }

                    else
                        _User.lstDocumentType = GetDocumentType();
                    ModelState.AddModelError("", "Invalid Username or Password");

                }
                else
                {
                    _User.lstDocumentType = GetDocumentType();
                    ModelState.AddModelError("", "Please Enter Password");
                }


            }
            else
            {
                _User.lstDocumentType = GetDocumentType();
                ModelState.AddModelError("", "Please Enter Username");
            }

            return View(_User);
        }

        public ActionResult Dashboard()
        {
            if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty)
            {
                if (Service == null)
                    Service = BL.OrganizationUtility.GetCRMService();

                Session["Uname"] = BL.CurrentUser.Instance.VerifiedUser.FirstName + " " + BL.CurrentUser.Instance.VerifiedUser.LastName;
                ViewBag.Uname = BL.CurrentUser.Instance.VerifiedUser.UserName;

                BL.NotesData Model = new BL.NotesData() { lstPortalUser = null, lstAssignedPortalUser = null, lstDataRow = null, lstHeaderColumn = null };
                Guid DocumentId = new Guid();
                EntityCollection Col = PortalRepository.GetNameWiseDocumentId("new_documentmanagementsystem", Service, BL.CurrentUser.Instance.VerifiedUser.UserId, BL.CurrentUser.Instance.VerifiedUser.Documenttyp);
                if (Col != null && Col.Entities.Count > 0)
                {
                    foreach (Entity enty in Col.Entities)
                    {
                        DocumentId = new Guid(enty.Attributes["new_documentmanagementsystemid"].ToString());
                    }
                }

                Model.IncidentId = DocumentId;


                //BL.DashboardData CounterData = PortalRepository.GetCounterDocumentNotesData("annotation", Service, Model.IncidentId, BL.CurrentUser.Instance.VerifiedUser.LastName);

                BL.DashboardData DashData = new BL.DashboardData();

                //DashData.Approved = CounterData.Approved;
                // DashData.Reject = CounterData.Reject;
                // DashData.Pending = CounterData.Pending;

                ViewBag.LiDashboard = "class=active";
                ViewBag.LiGetAllCasesForIncident = "class=";
                ViewBag.LiCreateCases = "class=";
                ViewBag.LiPassword = "class=";
                return View(DashData);
                //    Service = BL.CRMUtility.GetCRMService();

                //List<BL.Activity> lstActivity = PortalUserRepository.GetAllActivity(Service, BL.CurrentUser.Instance.VerifiedUser.UserId, Guid.Empty);

            }
            else
                return RedirectToAction("Login");
        }

        public ActionResult ChangePassword()
        {
            try
            {
                if (Service == null)
                    Service = BL.OrganizationUtility.GetCRMService();

                if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty)
                {
                    ViewBag.LiDashboard = "class=";
                    ViewBag.LiGetAllCasesForIncident = "class=";
                    ViewBag.LiCreateCases = "class=";
                    ViewBag.LiPassword = "class=active";
                    return View();
                }
                else
                    return RedirectToAction("Login");


            }
            catch (Exception)
            {
                return Redirect("~/theme/ErrorPage.html");
            }
        }

        public ActionResult Logout()
        {
            try
            {
                if (Service == null)
                    Service = BL.OrganizationUtility.GetCRMService();

                if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty)
                {
                    Session["Uname"] = null;
                    BL.CurrentUser.Instance.VerifiedUser.UserId = Guid.Empty;
                    ViewBag.LiDashboard = "class=";
                    ViewBag.LiGetAllCasesForIncident = "class=";
                    ViewBag.LiCreateCases = "class=";
                    ViewBag.LiPassword = "class=active";
                    return RedirectToAction("Login");
                }
                else
                    return RedirectToAction("Login");


            }
            catch (Exception)
            {
                return Redirect("~/theme/ErrorPage.html");
            }
        }

        [HttpPost]
        public ActionResult SaveChangePassword(BL.ChangePassword CP)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (Service == null)
                        Service = BL.OrganizationUtility.GetCRMService();

                    if (BL.CurrentUser.Instance != null && BL.CurrentUser.Instance.VerifiedUser.UserId != Guid.Empty)
                    {
                        ModelState.Clear();
                        string Result = PortalRepository.ChangePWDAuthenticateUser(BL.CurrentUser.Instance.VerifiedUser.UserId.ToString(), CP.OldPassword, Service);
                        if (Result != null)
                        {
                            Entity entity = new Entity("new_portaluser");
                            //entity.Id = ""; 
                            entity.LogicalName = "new_portaluser";
                            Guid contactid = new Guid(BL.CurrentUser.Instance.VerifiedUser.UserId.ToString());
                            EntityReference ContactId = new EntityReference("new_portaluser", contactid);
                            entity.Attributes["new_portaluserid"] = ContactId.Id;
                            entity.Attributes["new_password"] = CP.NewPassword;
                            Service.Update(entity);

                            ModelState.AddModelError("", "Successfully Updated");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Incorrect Old Password");
                        }
                        ViewBag.LiDashboard = "class=";
                        ViewBag.LiGetAllCasesForIncident = "class=";
                        ViewBag.LiCreateCases = "class=";
                        ViewBag.LiPassword = "class=active";
                        return View("ChangePassword");
                    }
                    else
                        return RedirectToAction("Login");
                }
                else
                {
                    ViewBag.LiDashboard = "class=";
                    ViewBag.LiGetAllCasesForIncident = "class=";
                    ViewBag.LiCreateCases = "class=";
                    ViewBag.LiPassword = "class=active";
                    return View("ChangePassword");
                }
            }
            catch (Exception)
            {
                return Redirect("~/theme/ErrorPage.html");
            }
        }
    }
}
