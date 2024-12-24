using System.ComponentModel.DataAnnotations;

namespace HospitalInventoryManagement.Web.ViewModel
{
    public class LoginViewModel
    {

        [Required(ErrorMessage = "Kullanıcı adı veya email zorunludur.")]
        [Display(Name = "Kullanıcı Adı veya Email")]
        public string Email { get; set; } // Hem email hem de username olarak kullanılır

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [Display(Name = "Şifre")]
        public string Password { get; set; }

        [Display(Name = "Beni Hatırla")]
        public bool RememberMe { get; set; }
    }
}
