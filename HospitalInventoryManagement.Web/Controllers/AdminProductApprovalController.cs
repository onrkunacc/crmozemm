using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;
using HospitalInventoryManagement.Web.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalInventoryManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductApprovalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminProductApprovalController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ReviewRequests()
        {
            try
            {
                // İlk sorgu: Verileri alın ve hastane ID'sini ayrı bir sütun olarak çek
                var requestsWithHospitalIds = _context.ProductRequests
                    .Where(r => r.Status == "Bekliyor")
                    .Include(r => r.RequestedByUser) 
                    .Select(r => new
                    {
                        r.ProductRequestID,
                        r.ProductName,
                        r.ReferenceNumber,
                        r.RequestedByUserID,
                        RequestedByUserName = r.RequestedByUser.UserName, 
                        HospitalID = r.RequestedByUser.HospitalID, 
                        r.RequestDate,
                        r.Status,
                        r.AdminComments
                    })
                    .ToList(); 

                // İkinci adım: Hastane adlarını çekerek ViewModel'e dönüştür
                var requests = requestsWithHospitalIds.Select(r => new ProductRequestViewModel
                {
                    ProductRequestID = r.ProductRequestID,
                    ProductName = r.ProductName,
                    ReferenceNumber = r.ReferenceNumber,
                    RequestedByUserID = r.RequestedByUserID,
                    RequestedByUserName = r.RequestedByUserName,
                    RequestedByHospitalName = _context.Hospitals
                        .FirstOrDefault(h => h.HospitalID == r.HospitalID)?.HospitalName, 
                    RequestDate = r.RequestDate,
                    Status = r.Status,
                    AdminComments = r.AdminComments
                }).ToList();

                return View(requests);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün isteklerini alırken hata oluştu: {ex.Message}");
                TempData["ErrorMessage"] = "Ürün isteklerini alırken bir hata oluştu.";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            try
            {
                var request = _context.ProductRequests.FirstOrDefault(r => r.ProductRequestID == id);
                if (request == null)
                {
                    TempData["ErrorMessage"] = "Ürün isteği bulunamadı.";
                    return RedirectToAction("ReviewRequests");
                }

                // Ürün tablosuna ekle
                var newProduct = new Product
                {
                    ProductName = request.ProductName,
                    ReferenceNumber = request.ReferenceNumber
                };

                _context.Products.Add(newProduct);

                // Durumu güncelle
                request.Status = "Onaylandı";
                _context.ProductRequests.Update(request);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ürün isteği başarıyla onaylandı ve ürün eklendi.";
                return RedirectToAction("ReviewRequests");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün isteği onaylanırken hata oluştu: {ex.Message}");
                TempData["ErrorMessage"] = "Ürün isteği onaylanırken bir hata oluştu. Lütfen tekrar deneyiniz.";
                return RedirectToAction("ReviewRequests");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RejectRequest(int id)
        {
            try
            {
                var request = _context.ProductRequests.FirstOrDefault(r => r.ProductRequestID == id);
                if (request == null)
                {
                    TempData["ErrorMessage"] = "Ürün isteği bulunamadı.";
                    return RedirectToAction("ReviewRequests");
                }

                // Durumu "Reddedildi" olarak güncelle
                request.Status = "Reddedildi";
                _context.ProductRequests.Update(request);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ürün isteği reddedildi.";
                return RedirectToAction("ReviewRequests");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ürün isteği reddedilirken hata oluştu: {ex.Message}");
                TempData["ErrorMessage"] = "Ürün isteği reddedilirken bir hata oluştu. Lütfen tekrar deneyiniz.";
                return RedirectToAction("ReviewRequests");
            }
        }
    }
}
