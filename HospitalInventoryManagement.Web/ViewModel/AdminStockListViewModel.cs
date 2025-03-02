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
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }


    }
}
