using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ClassLibrary1
{
    public class NotesData
    {
        public Guid annotationid { get; set; }
        public string subject { get; set; }
        public string notetext { get; set; }
        public Guid IncidentId { get; set; }

        //[RegularExpression(@"([a-zA-Z0-9\s_\\.\-:])+(.pdf|.doc|.docx|.txt)$", ErrorMessage = ("Only Pdf, Word & Text files allowed."))]
        public HttpPostedFileBase Attachment { get; set; }

        public string createdby { get; set; }
        public DateTime createdon { get; set; }
        public List<NotesData> lstNotesData { get; set; }
        public string filename { get; set; }
        public string filecontent { get; set; }
        public string mimetype { get; set; }
        public List<EntityAttributes> lstHeaderColumn { get; set; }
        public List<DataRow> lstDataRow { get; set; }
        public string SearchKeyword { get; set; }
        public string MessageOnView { get; set; }
        public List<SelectListItem> lstPortalUser { get; set; }
        public List<SelectListItem> lstAssignedPortalUser { get; set; }
        public string PortalUser { get; set; }
        public string SharedOwnerUser { get; set; }
        public string ApprovalStatus { get; set; }
        public string ApprovedBy { get; set; }
        public string ApprovedByName { get; set; }
        public Guid LoginUser { get; set; }
        public bool AllowApprove { get; set; }
        public List<PortalNotesUser> NotesSharedUser { get; set; }
        public List<DashboardData> DashData { get; set; }
        public List<SelectListItem> lstDocumentType { get; set; }
        public string Documenttyp { get; set; }
        public string AppendNotesText { get; set; }
        public DateTime Start_Date { get; set; }
        public DateTime End_Date { get; set; }
        public string PortalUsercreatedby { get; set; }
        public string DocumenttypSave { get; set; }
    }
}
