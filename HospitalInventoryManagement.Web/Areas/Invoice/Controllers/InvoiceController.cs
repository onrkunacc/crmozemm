using System;
using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddInvoice(Invoices invoice)
        {
            if (invoice.CariId == 0)
            {
                ModelState.AddModelError("CariId", "Lüften bir cari seçiniz.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Eğer KapanisTarihi girilmemişse null bırak
                    invoice.KapanisTarihi = invoice.KapanisTarihi == DateTime.MinValue ? null : invoice.KapanisTarihi;
                    //< div class="mb-3">
                    //    < label asp -for= "KapanisTarihi" class="form-label">Kapanış Tarihi:</label>
                    //    <input asp-for="KapanisTarihi" type="date" class="form-control" />
                    //    <span asp-validation-for="KapanisTarihi" class="text-danger"></span>
                    //</div>
                    _context.Invoices.Add(invoice);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Hata:{ex.Message}");

                }

            }

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
    }
}
