using System;
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
    [Route("[area]/[controller]/[action]")]
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoiceController(ApplicationDbContext context)
        {
            _context = context;
        }
        //Fatura Listesi
        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Invoices
                .Include(f => f.Cari)
                .ToListAsync();
            return View(invoices);
        }

        //Yeni Fatura Ekleme 
        public async Task<IActionResult> AddInvoice()
        {
            ViewBag.Cariler = await _context.Cariler
           .Select(c => new { c.Id, c.Unvan })
           .ToListAsync();

            return View(new Invoices());
        }

        [HttpPost]
        public async Task<IActionResult> AddInvoice(Invoices invoice)
        {
            try
            {
                // Tutarı tr-TR formatına göre parse et
                var cultureInfo = new System.Globalization.CultureInfo("tr-TR");
                invoice.Tutar = decimal.Parse(invoice.Tutar.ToString(cultureInfo), cultureInfo);
                ModelState.Remove(nameof(invoice.Cari));

                // Validasyon kontrolü
                if (ModelState.IsValid)
                {
                    invoice.Ekler = string.IsNullOrWhiteSpace(invoice.Ekler) ? null : invoice.Ekler;

                    _context.Invoices.Add(invoice);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Fatura başarıyla eklendi.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Bir hata oluştu: {ex.Message}";
            }

            // Hata durumunda cariler tekrar yüklenir
            ViewBag.Cariler = await _context.Cariler
                .Select(c => new { c.Id, c.Unvan })
                .ToListAsync();

            return View(invoice);
        }

        // Fatura Silme
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        [HttpGet]
        public async Task<IActionResult> EditInvoice(int cariId, int month, int year)
        {
            try
            {
                var invoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.CariId == cariId && i.Ay == month && i.Donemi == year);

                if (invoice == null)
                {
                    TempData["ErrorMessage"] = "Fatura bulunamadı.";
                    return RedirectToAction("Index");
                }

                var cari = await _context.Cariler
                    .FirstOrDefaultAsync(c => c.Id == cariId);

                if (cari == null)
                {
                    TempData["ErrorMessage"] = "Cari bulunamadı.";
                    return RedirectToAction("Index");
                }

                var viewModel = new EditInvoiceViewModel
                {
                    FaturaId = invoice.Id,
                    CariId = cari.Id,
                    CariAdi = cari.Unvan,
                    Ay = invoice.Ay,
                    Yil = invoice.Donemi,
                    Tutar = invoice.Tutar,
                    Ekler = invoice.Ekler
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Bir hata oluştu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public async Task<IActionResult> EditInvoice(EditInvoiceViewModel model)
        {
            try
            {
                // Cari Adı validasyondan kaldırılıyor
                ModelState.Remove("CariAdi");

                if (ModelState.IsValid)
                {
                    var invoice = await _context.Invoices.FindAsync(model.FaturaId);

                    if (invoice == null)
                    {
                        TempData["ErrorMessage"] = "Fatura bulunamadı.";
                        return RedirectToAction("Index");
                    }
                    Console.WriteLine($"Model Tutar: {model.Tutar}"); 
                    Console.WriteLine($"Invoice Tutar (before update): {invoice.Tutar}");

                    var cultureInfo = new System.Globalization.CultureInfo("tr-TR");
                    invoice.Tutar = decimal.Parse(model.Tutar.ToString(cultureInfo), cultureInfo);
                    Console.WriteLine($"Invoice Tutar (after update): {invoice.Tutar}");
                    // Ekler alanı için boşluk kontrolü
                    invoice.Ekler = string.IsNullOrWhiteSpace(model.Ekler) ? null : model.Ekler;

                    // Veriyi güncelle ve kaydet
                    _context.Invoices.Update(invoice);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Fatura başarıyla güncellendi.";
                    return RedirectToAction("Details", "Cari", new { id = model.CariId, year = model.Yil });
                }
                else
                {
                    TempData["ErrorMessage"] = "Geçersiz form girişleri.";
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Bir hata oluştu: {ex.Message}");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> FilterInvoice (int year)
        {
            var invoices = await _context.Invoices
                .Include(f => f.Cari)
                .Where(f => f.Donemi == year)
                .ToListAsync();

            ViewBag.SelectedYear = year;
            return View("Index", invoices);
        }

    }
}
