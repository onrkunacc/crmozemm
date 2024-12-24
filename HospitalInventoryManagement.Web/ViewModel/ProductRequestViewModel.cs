using System.ComponentModel.DataAnnotations;

namespace HospitalInventoryManagement.Web.ViewModel
{
    public class ProductRequestViewModel
    {
        public int ProductRequestID { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [Display(Name = "Ürün Adı")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Referans numarası zorunludur.")]
        [Display(Name = "Referans Numarası")]
        public string ReferenceNumber { get; set; }

        public string RequestedByUserID { get; set; } // Giriş yapan kullanıcının ID'si
        public string RequestedByUserName { get; set; } // Gönderen kullanıcı adı
        public string RequestedByHospitalName { get; set; } // Gönderen kullanıcının hastane adı

        public DateTime RequestDate { get; set; } = DateTime.Now;

        [Display(Name = "Durum")]
        public string Status { get; set; } = "Bekliyor"; // Varsayılan durum

        [Display(Name = "Admin Yorumları")]
        public string? AdminComments { get; set; } // Admin yorumları
    }
}
