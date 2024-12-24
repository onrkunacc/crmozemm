using System.ComponentModel.DataAnnotations;

namespace HospitalInventoryManagement.Web.ViewModel
{
    public class FlaconManagementViewModel
    {
        public int StockID { get; set; }
        public string ProductName { get; set; }
        public string HospitalName { get; set; }
        public int CurrentFlaconCount { get; set; }

        [Required(ErrorMessage = "Flakon değişim miktarı gereklidir.")]
        [Range(1, int.MaxValue, ErrorMessage = "Değişim miktarı sıfırdan büyük olmalıdır.")]
        public int ChangeAmount { get; set; }
    }
}
