namespace HospitalInventoryManagement.Data.DTOs
{
    public class StockDTO
    {
        public int StockID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string ProductName { get; set; }  // Sadece ProductName'i serileştirin
        public string HospitalName { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
