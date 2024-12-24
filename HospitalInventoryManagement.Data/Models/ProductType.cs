using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.Data.Models
{
    public class ProductType
    {
        [Key]
        public int TypeID { get; set; }
        public string TypeName { get; set; }
        public ICollection<Product> Products { get; set;}
    }
}
