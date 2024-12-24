using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.Data.Models
{
    public class Message
    {
        public int MessageID { get; set; }
        public string UserID { get; set; }
        public int? HospitalID { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public DateTime SentDate { get; set; }
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
        public ApplicationUser User { get; set; }
        public Hospital Hospital { get; set; }
    }
}
