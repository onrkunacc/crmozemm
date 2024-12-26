using System.ComponentModel.DataAnnotations;

namespace HospitalInventoryManagement.Web.ViewModel
{
    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Referans numarası zorunludur.")]
        public string ReferenceNumber { get; set; }

        public int FlaconCountPerBox { get; set; }

        [Required(ErrorMessage = "Kutu test adedi zorunludur.")]
        public int BoxTextCount { get; set; }

        [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Ürün tipi seçimi zorunludur.")]
        public int TypeID { get; set; }

        [Required(ErrorMessage = "Platform seçimi zorunludur.")]
        public int PlatformID { get; set; }

    }
}
