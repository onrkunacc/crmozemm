using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;
using HospitalInventoryManagement.Web.Areas.Invoice.Models.ViewModels;
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
        public IActionResult Create()
        {
            var cariGruplari = _context.CariGruplari
                .Select(cg => new SelectListItem
                {
                    Value = cg.Id.ToString(),
                    Text = cg.GrupAdi
                }).ToList();

            var viewModel = new CariCreateViewModel
            {
                Cari = new Cariler(),
                CariGruplari = cariGruplari
            };

            return View(viewModel);
        }


        // Yeni Cari Ekleme (Post İşlemi)
        [HttpPost]
        public async Task<IActionResult> Create(CariCreateViewModel viewModel)
        {
            Console.WriteLine($"Gelen CariGrubuId: {viewModel.Cari.CariGrubuId}");

            // ModelState'ten bu alanları kaldır
            ModelState.Remove("CariGruplari");
            ModelState.Remove("Cari.Invoices");
            ModelState.Remove("Cari.CariGrubu");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState Hataları:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }

                // View'e yeniden doldurulabilir alanları ekle
                viewModel.CariGruplari = _context.CariGruplari
                    .Select(cg => new SelectListItem
                    {
                        Value = cg.Id.ToString(),
                        Text = cg.GrupAdi
                    }).ToList();

                return View(viewModel);
            }

            // Cari ekleme işlemi
            _context.Cariler.Add(viewModel.Cari);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cari başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }


        [Area("Invoice")]
        [HttpGet]
        public async Task<IActionResult> Details(int id, int? year)
        {
            int currentYear = year ?? DateTime.Now.Year;

            // Cari ve faturaları getir
            var cari = await _context.Cariler
                .Include(c => c.CariGrubu)
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cari == null)
            {
                return NotFound("Cari bulunamadı.");
            }

            // Faturaları filtrele
            var invoicesForSelectedYear = cari.Invoices
                .Where(f => f.Donemi == currentYear)
                .ToList();

            // 12 aylık listeyi oluştur ve faturalarla eşleştir
            var allMonths = Enumerable.Range(1, 12)
                .Select(month => new FaturaViewModel
                {
                    AyIndex = month,
                    AyAdi = new[]
                    {
                        "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran",
                        "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"
                    }[month - 1],
                    Tutar = invoicesForSelectedYear.FirstOrDefault(f => f.Ay == month)?.Tutar,
                    FaturaVarMi = invoicesForSelectedYear.Any(f => f.Ay == month),
                    FaturaId = invoicesForSelectedYear.FirstOrDefault(f => f.Ay == month)?.Id,
                    CariId = id // CariId'yi her nesneye ekliyoruz
                }).ToList();

            // ViewModel'i View'e aktar
            var viewModel = new
            {
                Cari = cari,
                Faturalar = allMonths,
                CurrentYear = currentYear,
                AvailableYears = _context.Invoices
                    .Where(i => i.CariId == id)
                    .Select(i => i.Donemi)
                    .Distinct()
                    .ToList()
            };

            return View(viewModel);
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
