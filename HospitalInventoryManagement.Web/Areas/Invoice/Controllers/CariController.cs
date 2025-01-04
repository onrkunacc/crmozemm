using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalInventoryManagement.Web.Areas.Invoice.Controllers
{
    [Area("Invoice")]
    [Authorize(Roles = "InvoiceRole, Admin")]
    public class CariController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CariController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Cari Listesi
        public async Task<IActionResult> Index()
        {
            var cariler = await _context.Cariler.Include(c => c.CariGrubu).ToListAsync();
            return View(cariler);
        }

        // Yeni Cari Ekleme (Form Görünümü)
        public IActionResult Create()
        {
            ViewBag.CariGruplari = new SelectList(_context.CariGruplari, "Id", "GrupAdi");
            return View(new Cariler());
        }

        // Yeni Cari Ekleme (Post İşlemi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cariler cari)
        {
            if (ModelState.IsValid)
            {
                _context.Cariler.Add(cari);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CariGruplari = _context.CariGruplari.ToList();
            return View(cari);
        }

        [HttpGet]
        public async Task<IActionResult> EditInvoice(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        [HttpPost]
        public async Task<IActionResult> EditInvoice(Invoices updatedInvoice)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var invoice = await _context.Invoices.FindAsync(updatedInvoice.Id);

                    if (invoice == null)
                    {
                        return NotFound();
                    }

                    // Verileri güncelle
                    invoice.Tutar = updatedInvoice.Tutar;
                    invoice.KapanisTarihi = updatedInvoice.KapanisTarihi;
                    invoice.Donemi = updatedInvoice.Donemi;
                    invoice.Ay = updatedInvoice.Ay;

                    _context.Invoices.Update(invoice);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Details", new { id = invoice.CariId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Fatura düzenlenirken bir hata oluştu: " + ex.Message);
                }
            }

            return View(updatedInvoice);
        }

        // Cari Detayları
        //Cari İşlemlerini 12 aylık fatura takibini devam ettiriyorum.Son işlemler .
        [Area("Invoice")]
        [Route("[area]/[controller]/[action]/{id?}")]
        public async Task<IActionResult> Details(int id)
        {
            Console.WriteLine($"Details action invoked with ID: {id}");
            var cari = await _context.Cariler
                .Include(c => c.CariGrubu)
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cari == null)
            {
                return NotFound();
            }

            // 12 aylık veri hazırlama
            var aylar = Enumerable.Range(1, 12).Select(ay => new
            {
                AyIndex = ay,
                AyAdi = new[]
                {
                    "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
                    "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"
                }[ay - 1],
                Fatura = cari.Invoices.FirstOrDefault(f => f.Ay == ay)
            }).ToList();

            ViewBag.Faturalar = aylar;

            return View(cari);
        }

        // Cari Silme
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var cari = await _context.Cariler.FindAsync(id);
            if (cari != null)
            {
                _context.Cariler.Remove(cari);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
