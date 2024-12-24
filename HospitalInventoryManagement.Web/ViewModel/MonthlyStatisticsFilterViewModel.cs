using HospitalInventoryManagement.Data.Models;

namespace HospitalInventoryManagement.Web.ViewModel
{
    public class MonthlyStatisticsFilterViewModel
    {
        public int HospitalID { get; set; } // Formdaki Hastane seçimi için
        public DateTime StartMonth { get; set; } // Formdaki Başlangıç ayı
        public DateTime EndMonth { get; set; } // Formdaki Bitiş ayı
        public IEnumerable<MonthlyStatistics> Statistics { get; set; } // Filtrelenmiş istatistikler
        public double AverageTestCount { get; set; } // Ortalama test sayısı
    }
}
