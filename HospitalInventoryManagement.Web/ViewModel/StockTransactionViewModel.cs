using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalInventoryManagement.Web.ViewModel
{
    public class StockTransactionViewModel
    {
        public int StockID { get; set; }
        public int ChangeQuantity { get; set; }
        public string TransactionType { get; set; } // "Çıkış" veya "Giriş" olacak
        public string Description { get; set; }
        [BindNever]
        public SelectList Stocks { get; set; }
    }
}
