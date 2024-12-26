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

        // Evrak Ekleme (Form Görünümü)
        public async Task<IActionResult> AddDocument()
        {
            ViewBag.Categories = await _context.DocumentCategories.ToListAsync();
            return View();
        }

        // Evrak Ekleme (Form Post İşlemi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDocument(FollowDocument document)
        {
            if (ModelState.IsValid)
            {
                document.Date = DateTime.Now; // Tarih otomatik olarak atanır
                document.UserID = User.Identity.Name; // Kullanıcı kimliği atanır
                _context.FollowDocuments.Add(document);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Categories = await _context.DocumentCategories.ToListAsync();
            return View(document);
        }
    }
}
