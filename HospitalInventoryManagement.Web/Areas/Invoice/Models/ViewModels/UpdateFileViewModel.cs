namespace HospitalInventoryManagement.Web.Areas.Invoice.Models.ViewModels
{
    public class UpdateFileViewModel
    {
        public int CariId { get; set; }
        public int Year { get; set; }
        public string FileName { get; set; }
        public string FileContent { get; set; }
    }
}
