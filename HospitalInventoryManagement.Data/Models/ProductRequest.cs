using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.Data.Models
{
    public class ProductRequest
    {
        public int ProductRequestID { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [Display(Name = "Ürün Adı")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Referans numarası zorunludur.")]
        [Display(Name = "Referans Numarası")]
        public string ReferenceNumber { get; set; }

        [Required]
        [ForeignKey("RequestedByUser")]
        public string RequestedByUserID { get; set; } // Giriş yapan kullanıcının ID'si

        public ApplicationUser RequestedByUser { get; set; } // ApplicationUser ile ilişki

        public DateTime RequestDate { get; set; } = DateTime.Now; // Varsayılan olarak şu anki tarih

        [Display(Name = "Durum")]
        public string Status { get; set; } = "Bekliyor"; // Varsayılan durum

        [Display(Name = "Admin Yorumları")]
        public string? AdminComments { get; set; } // Zorunlu değil
    }
}
