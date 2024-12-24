using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HospitalInventoryManagement.Web.ViewModel
{
    public class StockViewModel
    {
        public int StockID { get; set; }

        [Required(ErrorMessage = "Ürün seçilmesi zorunludur.")]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Hastane seçilmesi zorunludur.")]
        public int HospitalID { get; set; }

        [Required(ErrorMessage = "Miadı seçilmesi zorunludur.")]
        [DataType(DataType.Date)]
        public DateTime ExpirationDate { get; set; }

        [Required(ErrorMessage = "Adet girilmesi zorunludur.")]
        public int Quantity { get; set; }
        public string LotNumber { get; set; } 

        public string ReferenceNumber { get; set; } 

        [BindNever]
        public SelectList Products { get; set; }

        [BindNever]
        public SelectList Hospitals { get; set; }


    }
}
