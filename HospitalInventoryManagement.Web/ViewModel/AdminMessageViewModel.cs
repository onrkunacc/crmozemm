using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalInventoryManagement.Web.ViewModel
{
    public class AdminMessageViewModel
    {
        public int MessageID { get; set; }
        public string UserName { get; set; }
        public string HospitalName { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public DateTime SentDate { get; set; }
        public bool IsRead { get; set; }
    }
}
