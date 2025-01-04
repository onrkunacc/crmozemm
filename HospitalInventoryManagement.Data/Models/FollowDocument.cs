using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.Data.Models
{
    public class FollowDocument
    {
        public int Id { get; set; }
        public string Code { get; set; }
        [Required]
        public string Subject { get; set; }
        public DateTime Date { get; set; }

        [Column("CategoryId"),Required]
        public int CategoryId { get; set; }       
        public DocumentCategory Category { get; set; }
    }
}
