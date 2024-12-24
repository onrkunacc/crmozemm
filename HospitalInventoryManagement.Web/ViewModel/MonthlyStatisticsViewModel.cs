namespace HospitalInventoryManagement.Web.ViewModel
{
    public class MonthlyStatisticsViewModel
    {
        public int HospitalID { get; set; } // Kullanıcının hastanesinin ID'si
        public DateTime Month { get; set; } // Testlerin yapıldığı ay
        public int TestCount { get; set; } // Test sayısı
        public string TestName { get; set; } // Testin adı
    }
}
