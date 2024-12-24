using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalInventoryManagement.Web.ViewModel
{
    public class ProductEditViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ReferenceNumber { get; set; }
        public string LotNumber { get; set; }
        public int BoxTextCount { get; set; }

        public int CategoryID { get; set; }
        public int TypeID { get; set; }
        public int PlatformID { get; set; }

        // SelectList değerleri için
        [BindNever]
        public SelectList Categories { get; set; }
        [BindNever]
        public SelectList ProductTypes { get; set; }
        [BindNever]
        public SelectList Platforms { get; set; }
    }
}
