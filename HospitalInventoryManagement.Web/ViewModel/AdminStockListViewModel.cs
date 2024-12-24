using HospitalInventoryManagement.Data.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalInventoryManagement.Web.ViewModel
{
    public class AdminStockListViewModel
    {
        public IEnumerable<StockDTO> Stocks { get; set; } // Stokların DTO versiyonu
        public SelectList Hospitals { get; set; } // Hastane seçim dropdown
        public int? SelectedHospitalID { get; set; } // Seçilen hastane ID'si
        public string SortOrder { get; set; } // Sıralama türü

    }
}
