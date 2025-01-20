using System.ComponentModel.DataAnnotations.Schema;
using HospitalInventoryManagement.Data.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalInventoryManagement.Web.Areas.Invoice.Models.ViewModels
{
    public class CariCreateViewModel
    {
        public Cariler Cari { get; set; } = new Cariler();
        [BindNever] 
        public IEnumerable<SelectListItem> CariGruplari { get; set; }
    }
}
 