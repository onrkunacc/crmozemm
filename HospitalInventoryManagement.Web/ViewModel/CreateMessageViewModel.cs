using System.ComponentModel.DataAnnotations;

namespace HospitalInventoryManagement.Web.ViewModel
{
    public class CreateMessageViewModel
    {
        [Required(ErrorMessage = "Konu alanı zorunludur.")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Mesaj içeriği zorunludur.")]
        public string Content { get; set; }

    }
}
