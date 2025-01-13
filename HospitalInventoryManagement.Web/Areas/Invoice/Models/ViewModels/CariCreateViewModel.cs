using HospitalInventoryManagement.Data.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalInventoryManagement.Web.Areas.Invoice.Models.ViewModels
{
    public class CariCreateViewModel
    {
        public Cariler Cari { get; set; }
        public IEnumerable<SelectListItem> CariGruplari { get; set; }
    }
}
