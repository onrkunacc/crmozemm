using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.Data.Models
{
    public class Hospital
    {
        [Key]
        public int HospitalID { get; set; }
        public string HospitalName { get; set; }

        public ICollection<Stock> Stocks { get; set; }

    }
}
