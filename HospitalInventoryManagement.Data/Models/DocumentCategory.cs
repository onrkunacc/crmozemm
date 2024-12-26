using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.Data.Models
{
    public class DocumentCategory
    {
        public int Id { get; set; } // Primary Key
        public string Code { get; set; } // Örneğin: YNTM, MHSB
        public string Description { get; set; } // Kategori açıklaması
    }
}
