using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalInventoryManagement.Web.Areas.Document.Controllers
{
    [Area("Document")]
    [Authorize(Roles = "DocumentRole, Admin")]

    public class DocumentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocumentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Evrak Listesi
        public async Task<IActionResult> Index()
        {
            var documents = await _context.FollowDocuments
                .Include(d => d.Category) // Kategoriyi de dahil et
                .ToListAsync();
            return View(documents);
        }
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.DocumentCategories
                    .Select(c => new { c.Id, c.Description })
                    .ToListAsync();

                return Json(categories); // JSON formatında kategorileri döndür
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Hata oluştu", Details = ex.Message });
            }
        }

        // Evrak Ekleme (Form Görünümü)
        public IActionResult AddDocument()
        {
            // Sadece boş bir FollowDocument modelini View'e gönderiyoruz
            return View(new FollowDocument());
        }

        // Evrak Ekleme (Form Post İşlemi)
        [HttpPost]
        public async Task<IActionResult> AddDocument(FollowDocument document)
        {
            // Validasyon Kontrolleri
            if (document.CategoryId == 0)
            {
                ModelState.AddModelError("CategoryId", "Lütfen bir kategori seçin.");
            }
            if (string.IsNullOrWhiteSpace(document.Subject))
            {
                ModelState.AddModelError("Subject", "Lütfen bir konu girin.");
            }
            ModelState.Remove("Code");
            ModelState.Remove("Category");
            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _context.DocumentCategories.FirstOrDefaultAsync(c => c.Id == document.CategoryId);
                    if (category == null)
                    {
                        ModelState.AddModelError("", "Seçilen kategori geçersiz.");
                        return View(document);
                    }

                    // Evrak Kodu Oluşturma
                    var currentYear = DateTime.Now.Year;
                    var latestDocument = await _context.FollowDocuments
                        .Where(d => d.CategoryId == document.CategoryId && d.Date.Year == currentYear)
                        .OrderByDescending(d => d.Id)
                        .FirstOrDefaultAsync();

                    int nextId = latestDocument != null ? latestDocument.Id + 1 : 1;

                    document.Code = $"{category.Code}.{currentYear}.{nextId}";
                    document.Date = DateTime.Now;

                    // Veritabanına Kaydetme
                    _context.FollowDocuments.Add(document);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Hata: {ex.Message}");
                    ModelState.AddModelError("", "Beklenmeyen bir hata oluştu.");
                }
            }

            // ModelState hatası varsa kategorileri tekrar yükle
            ViewBag.Categories = await _context.DocumentCategories.ToListAsync();
            return View(document);
        }

       
    }
}
