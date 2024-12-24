using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.Data.Models
{
    public class MonthlyStatistics
    {
       
        public int MonthlyStatisticsID { get; set; }
        public int HospitalID { get; set; }
        public DateTime Month {  get; set; }
        public int TestCount { get; set; }
        public string TestName { get; set; }
        public Hospital Hospital { get; set; }
    }
}
