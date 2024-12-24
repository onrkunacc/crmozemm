using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.Data.Models
{
    public class Platform
    {
        [Key]
        public int PlatformID { get; set; }
        public string PlatformName { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
