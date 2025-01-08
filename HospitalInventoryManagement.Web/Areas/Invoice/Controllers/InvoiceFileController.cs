using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Packaging;
using HospitalInventoryManagement.Web.Areas.Invoice.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace YourProject.Areas.Invoice.Controllers
{
    [Area("Invoice")]
    [Authorize(Roles = "Admin, InvoiceManager")]
    public class InvoiceFileController : Controller
    {
        private readonly string _baseFilePath = @"C:\Temp\OdemeYazilari";

        // Dosya Listeleme
        public IActionResult Index(int cariId, int year)
        {
            var cariFolder = Path.Combine(_baseFilePath, $"Cari{cariId}", year.ToString());
            if (!Directory.Exists(cariFolder))
            {
                Directory.CreateDirectory(cariFolder);
            }

            var files = Directory.GetFiles(cariFolder);
            var fileModels = new List<FileViewModel>();

            foreach (var file in files)
            {
                fileModels.Add(new FileViewModel
                {
                    FileName = Path.GetFileName(file),
                    FilePath = file
                });
            }

            return View(fileModels);
        }

        // Dosya Yükleme
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, int cariId, int year)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Geçersiz dosya.";
                return RedirectToAction("Index");
            }

            var folderPath = Path.Combine(_baseFilePath, $"Cari{cariId}", year.ToString());
            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            TempData["SuccessMessage"] = "Dosya başarıyla yüklendi.";
            return RedirectToAction("Index", new { cariId, year });
        }

        // Dosya İndirme
        public IActionResult DownloadFile(int cariId, int year, string fileName)
        {
            var filePath = Path.Combine(_baseFilePath, $"Cari{cariId}", year.ToString(), fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Dosya bulunamadı.");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        // Dosya Güncelleme (Parametre Değişikliği)
        [HttpPost]
        public IActionResult UpdateFile(int cariId, int year, string fileName, Dictionary<string, string> parameters)
        {
            var filePath = Path.Combine(_baseFilePath, $"Cari{cariId}", year.ToString(), fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Dosya bulunamadı.");
            }

            using (var wordDoc = WordprocessingDocument.Open(filePath, true))
            {

                var body = wordDoc.MainDocumentPart.Document.Body;
                foreach (var parameter in parameters)
                {
                    body.InnerXml = body.InnerXml.Replace($"{{{parameter.Key}}}", parameter.Value);
                }

                wordDoc.MainDocumentPart.Document.Save();
            }

            TempData["SuccessMessage"] = "Dosya başarıyla güncellendi.";
            return RedirectToAction("Index", new { cariId, year });
        }
    }
}
