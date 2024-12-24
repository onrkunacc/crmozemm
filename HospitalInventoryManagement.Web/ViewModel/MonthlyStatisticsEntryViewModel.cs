namespace HospitalInventoryManagement.Web.ViewModel
{
    public class MonthlyStatisticsEntryViewModel
    {
        public DateTime? SelectedMonth { get; set; }
        public List<TestEntry> Tests { get; set; } = new List<TestEntry>();
    }

    public class TestEntry
    {
        public string TestName { get; set; }
        public int TestCount { get; set; }
    }
}
