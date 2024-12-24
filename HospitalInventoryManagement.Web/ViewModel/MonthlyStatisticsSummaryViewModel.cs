namespace HospitalInventoryManagement.Web.ViewModel
{
    public class MonthlyStatisticsSummaryViewModel
    {
        public string TestName { get; set; }
        public List<MonthlyTestCount> MonthlyTestCounts { get; set; } = new List<MonthlyTestCount>(); // Aylara göre test sayıları

        public double AverageTestCount => MonthlyTestCounts.Any()
            ? MonthlyTestCounts.Average(x => x.TestCount)
            : 0; // Test sayılarının ortalaması
    }
    public class MonthlyTestCount
    {
        public string Month { get; set; } // Ay bilgisi (örneğin, "Ocak 2024")
        public int TestCount { get; set; } // O ay yapılan test sayısı
    }
}
