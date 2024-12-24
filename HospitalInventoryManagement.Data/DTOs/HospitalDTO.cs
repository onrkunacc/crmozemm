namespace HospitalInventoryManagement.Data.DTOs
{
    public class HospitalDTO
    {
        public int HospitalID { get; set; }
        public string HospitalName { get; set; }
        public List<StockDTO> Stocks { get; set; }  // Döngüyü kırmak için Hospital nesnesi kullanmayın, DTO kullanın
    }
}
