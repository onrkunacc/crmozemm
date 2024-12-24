using HospitalInventoryManagement.Data.Models;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;

namespace HospitalInventoryManagement.Web.ViewModel
{
    public class HomeViewModel
    {
        public List<Stock> LowStockItems { get; set; }
        public List<MonthlyStatisticsViewModel> StatisticsData { get; set; }
        public List<StockChartData> StockData { get; set; }
    }

    public class StockChartData
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
}
