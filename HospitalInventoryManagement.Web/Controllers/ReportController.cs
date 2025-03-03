using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace HospitalInventoryManagement.Web.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ReportController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> ExportToPdf()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var hospitalName = await _context.Hospitals
                .Where(h => h.HospitalID == user.HospitalID)
                .Select(h => h.HospitalName)
                .FirstOrDefaultAsync();

            var stocks = await _context.Stocks
                .Where(s => s.HospitalID == user.HospitalID)
                .Include(s => s.Product)
                .ThenInclude(p => p.Category)
                .OrderBy(s => s.Product.ProductName)
                .ToListAsync();

            var pdfBytes = GenerateStockPdf(stocks, hospitalName);
            return File(pdfBytes, "application/pdf", $"{hospitalName}_StokListesi.pdf");
        }

        private byte[] GenerateStockPdf(List<Stock> stocks, string hospitalName)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 25, 25, 30, 30);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            // ✅ Font Yolu (Windows Arial / Linux DejaVuSans)
            string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
            if (!System.IO.File.Exists(fontPath))
            {
                fontPath = "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf";  // Linux ortamı için
            }

            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            var titleFont = new Font(baseFont, 16, Font.BOLD, new BaseColor(0, 0, 0));
            var headerFont = new Font(baseFont, 10, Font.BOLD, new BaseColor(255, 255, 255));
            var cellFont = new Font(baseFont, 9, Font.NORMAL, new BaseColor(0, 0, 0));
            var footerFont = new Font(baseFont, 9, Font.NORMAL, new BaseColor(0, 0, 0));

            // Başlık
            document.Add(new Paragraph($"{hospitalName.ToUpper()}’NİN STOK LİSTESİ", titleFont));
            document.Add(new Paragraph("\n"));

            // Tablo oluşturma
            var table = new PdfPTable(6)
            {
                WidthPercentage = 100
            };
            table.SetWidths(new float[] { 2, 2, 1, 1, 1, 1 });

            string[] headers = { "Ürün Adı", "Kategori", "Referans No", "Lot No", "Miktar", "Miadı" };

            // Header'lar
            foreach (var header in headers)
            {
                var headerCell = new PdfPCell(new Phrase(header, headerFont))
                {
                    BackgroundColor = new BaseColor(76, 175, 80),  // Yeşil
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5
                };
                table.AddCell(headerCell);
            }

            // Satırları ekle (zebra style)
            int rowCount = 0;
            foreach (var stock in stocks)
            {
                var bgColor = rowCount % 2 == 0
                    ? new BaseColor(245, 245, 245)  // Açık Gri
                    : new BaseColor(255, 255, 255); // Beyaz

                AddTableCell(table, stock.Product?.ProductName, cellFont, bgColor);
                AddTableCell(table, stock.Product?.Category?.CategoryName, cellFont, bgColor);
                AddTableCell(table, stock.Product?.ReferenceNumber, cellFont, bgColor);
                AddTableCell(table, stock.LotNumber, cellFont, bgColor);
                AddTableCell(table, stock.Quantity.ToString(), cellFont, bgColor);
                AddTableCell(table, stock.ExpiryDate.ToString("dd-MM-yyyy"), cellFont, bgColor);

                rowCount++;
            }

            document.Add(table);

            // Footer - Oluşturulma Tarihi
            document.Add(new Paragraph($"\nOluşturulma Tarihi: {DateTime.Now:dd-MM-yyyy HH:mm}", footerFont));

            document.Close();
            writer.Close();

            return memoryStream.ToArray();
        }

        private void AddTableCell(PdfPTable table, string text, Font font, BaseColor backgroundColor)
        {
            var cell = new PdfPCell(new Phrase(text ?? "", font))
            {
                BackgroundColor = backgroundColor,
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5
            };
            table.AddCell(cell);
        }
    }
}
