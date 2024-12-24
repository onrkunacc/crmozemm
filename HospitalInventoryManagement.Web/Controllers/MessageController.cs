using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;
using HospitalInventoryManagement.Web.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalInventoryManagement.Web.Controllers
{
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageController(ApplicationDbContext context,UserManager<ApplicationUser> userManager)
        {
             _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("AccessDenied", "Account");

            return View(new CreateMessageViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Create (CreateMessageViewModel viewModel)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                if (!ModelState.IsValid)
                {
                    foreach (var state in ModelState)
                    {
                        Console.WriteLine($"Key: {state.Key}");
                        foreach (var error in state.Value.Errors)
                        {
                            Console.WriteLine($"Error: {error.ErrorMessage}");
                        }
                    }

                    return View(viewModel);
                }

                // Mesaj oluştur
                var message = new Message
                {
                    UserID = user.Id,
                    HospitalID = user.HospitalID,
                    Subject = viewModel.Subject,
                    Content = viewModel.Content,
                    SentDate = DateTime.Now
                };
                Console.WriteLine($"Mesaj = {message}");
                // Mesajı ekle
                _context.Messages.Add(message);

                // Veritabanına kaydet
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Mesajınız başarıyla gönderildi.";
                return RedirectToAction("Create");
            }
            catch (DbUpdateException dbEx)
            {
                // Veritabanı güncelleme hatası yakalama
                Console.WriteLine($"Veritabanı Hatası: {dbEx.Message}");
                TempData["ErrorMessage"] = "Mesaj gönderilirken bir hata oluştu. Lütfen tekrar deneyiniz.";
            }
            catch (Exception ex)
            {
                // Genel hata yakalama
                Console.WriteLine($"Hata: {ex.Message}");
                TempData["ErrorMessage"] = "Beklenmeyen bir hata oluştu. Lütfen sistem yöneticisine başvurun.";
            }

            // Hata durumunda tekrar aynı view'i döndür       
            return View();

        }


        [Authorize(Roles ="Admin")]
        [HttpGet]
        public IActionResult AdminMessageIndex()
        {
            var messages = _context.Messages
                .Where(m => !m.IsDeleted)
                .Include(m => m.User) // Kullanıcı bilgilerini almak için Include
                .Include(m => m.Hospital) // Hastane bilgilerini almak için Include
                .OrderByDescending(m => m.SentDate)
                .Select(m => new AdminMessageViewModel
                {
                    MessageID = m.MessageID,
                    UserName = m.User.UserName, // Gönderenin kullanıcı adı
                    HospitalName = m.Hospital.HospitalName, // Gönderenin hastane adı
                    Subject = m.Subject,
                    Content = m.Content,
                    SentDate = m.SentDate,
                    IsRead = m.IsRead
                })
                .ToList();

            return View(messages);
        }
        // Mesajı sil
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null)
            {
                message.IsDeleted = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AdminMessageIndex");
        }

        // Mesajı okundu olarak işaretle
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AdminMessageIndex");
        }
    }
}
