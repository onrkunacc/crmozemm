namespace HospitalInventoryManagement.Web.ViewModel
{
    public class BarcodeStockTransactionViewModel
    {
        public string ReferenceNumber { get; set; } // Barkoddan gelen referans numarası
        public int Quantity { get; set; } // Giriş veya çıkış yapılacak miktar
        public string TransactionType { get; set; } 
        public string LotNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string ProductName { get; set; }
    }
}
