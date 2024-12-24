using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.Data.Models
{
    public class TestTemplate
    {
        public int TestTemplateID { get; set; }
        public int HospitalID { get; set; }
        public string TestName { get; set; }
        public bool IsActive { get; set; }
        [ForeignKey("HospitalID")]
        public Hospital Hospital { get; set; }
    }
}
