using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.Data.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ReferenceNumber { get; set; }
        public string? LotNumber { get; set; }
        public int BoxTextCount { get; set; }
        public int FlaconCountPerBox { get; set; }
        public int CategoryID { get; set; }
        public Category Category { get; set; }
        public int TypeID { get; set; }
        public ProductType ProductType { get; set; }
        public int PlatformID { get; set; }
        public Platform Platform { get; set; }  
        public ICollection<Stock> Stocks { get; set; }
    }
}
