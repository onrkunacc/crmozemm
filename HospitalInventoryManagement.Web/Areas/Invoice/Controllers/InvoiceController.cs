using System;
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

        //Fatura Detayları
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.Invoices
                .Include(f => f.Cari)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }
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

    }
}
