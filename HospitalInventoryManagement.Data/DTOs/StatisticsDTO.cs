using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.Data.DTOs
{
    public class StatisticsDTO
    {
        //İstatistik Verileri DTO
        public int HospitalID { get; set; }
        public string HospitalName { get; set; }
        public DateTime Month { get; set; }
        public string TestName { get; set; }
        public int TestCount { get; set; }  
    }
}
