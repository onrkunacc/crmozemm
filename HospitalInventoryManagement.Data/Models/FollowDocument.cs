using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.Data.Models
{
    public class FollowDocument
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Subject { get; set; }
        public DateTime Date { get; set; }
        public string UserID { get; set; }
        public int CategoryID { get; set; }
        public DocumentCategory Category { get; set; }
    }
}
