using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.Data.Models
{
    public class StockTransaction
    {
        [Key]
        public int TransactionID { get; set; }
        public int StockID { get; set;}
        public int ChangeQuantity { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public string Description { get; set; }
        public string LotNumber { get; set; }
        public Stock Stock {  get; set; }
    }
}
