namespace HospitalInventoryManagement.Web.Areas.Invoice.Models.ViewModels
{
    public class DirectoryViewModel
    {
        public int CariId { get; set; }
        public string CariName { get; set; }
        public List<YearFolderViewModel> YearFolders { get; set; }
    }
}
