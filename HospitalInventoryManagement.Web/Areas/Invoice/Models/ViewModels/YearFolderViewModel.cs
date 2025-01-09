namespace HospitalInventoryManagement.Web.Areas.Invoice.Models.ViewModels
{
    public class YearFolderViewModel
    {
        public int Year { get; set; }
        public List<FileViewModel> Files { get; set; }
    }
}
