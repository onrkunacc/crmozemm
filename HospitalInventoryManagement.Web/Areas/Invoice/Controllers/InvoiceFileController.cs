using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Packaging;
using HospitalInventoryManagement.Web.Areas.Invoice.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using HospitalInventoryManagement.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace YourProject.Areas.Invoice.Controllers
{
    [Area("Invoice")]
    [Authorize(Roles = "Admin,InvoiceRole")]
    public class InvoiceFileController : Controller
    {
        private readonly string _baseFilePath = @"C:\Temp\OdemeYazilari";
        private readonly ApplicationDbContext _context;

        public InvoiceFileController(ApplicationDbContext context)
        {
                _context = context;
        }

        // Dosya Listeleme
        public async Task<IActionResult> Index(int? cariId, int? year)
        {
            // Cariler ve yıllar için ViewBag doldur
            ViewBag.Cariler = await _context.Cariler
                .Select(c => new CariViewModel { Id = c.Id, Unvan = c.Unvan })
                .ToListAsync();

            ViewBag.Years = Enumerable.Range(DateTime.Now.Year - 10, 10).ToList();

            ViewBag.SelectedCariId = cariId;
            ViewBag.SelectedYear = year;

            // Varsayılan olarak cariId veya year belirtilmediyse dosyaları listeleme
            if (!cariId.HasValue || !year.HasValue)
            {
                return View(new List<FileViewModel>());
            }

            // Dosyaları getir
            var cariFolder = Path.Combine(_baseFilePath, $"Cari{cariId}", year.ToString());
            if (!Directory.Exists(cariFolder))
            {
                Directory.CreateDirectory(cariFolder);
            }

            var files = Directory.GetFiles(cariFolder);
            var fileModels = files.Select(file => new FileViewModel
            {
                FileName = Path.GetFileName(file),
                FilePath = file
            }).ToList();

            return View(fileModels);
        }

        [HttpGet]
        public async Task<IActionResult> UploadFile()
        {
            ViewBag.Cariler = await _context.Cariler
                     .Select(c => new CariViewModel { Id = c.Id, Unvan = c.Unvan })
                     .ToListAsync();

            ViewBag.Years = Enumerable.Range(DateTime.Now.Year - 5, 10).ToList();

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, int cariId, int year)
        {
            var cari = await _context.Cariler.FindAsync(cariId);
            if (cari == null)
            {
                TempData["ErrorMessage"] = "Geçersiz cari seçimi.";
                return RedirectToAction("UploadFile");
            }

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Geçersiz dosya.";
                return RedirectToAction("UploadFile");
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

        [HttpGet]
        public IActionResult UpdateFileView(int cariId, int year, string fileName)
        {
            var filePath = Path.Combine(_baseFilePath, $"Cari{cariId}", year.ToString(), fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Dosya bulunamadı.");
            }

            string fileContent;
            using (var wordDoc = WordprocessingDocument.Open(filePath, false))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;
                fileContent = body.InnerXml; // Word içeriği HTML/XML formatında alınır
            }

            var model = new UpdateFileViewModel
            {
                CariId = cariId,
                Year = year,
                FileName = fileName,
                FileContent = fileContent // HTML olarak frontend'e gönderilir
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult UpdateFile(UpdateFileViewModel model)
        {
            var filePath = Path.Combine(_baseFilePath, $"Cari{model.CariId}", model.Year.ToString(), model.FileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Dosya bulunamadı.");
            }

            using (var wordDoc = WordprocessingDocument.Open(filePath, true))
            {
                var body = wordDoc.MainDocumentPart.Document.Body;
                body.InnerXml = model.FileContent; // Kullanıcıdan gelen düzenlenmiş içerik dosyaya yazılır
                wordDoc.MainDocumentPart.Document.Save();
            }

            TempData["SuccessMessage"] = "Dosya başarıyla güncellendi.";
            return RedirectToAction("Index", new { cariId = model.CariId, year = model.Year });
        }

        public IActionResult AllFiles()
        {
            var allFilesModel = new List<DirectoryViewModel>();

            var cariDirectories = Directory.GetDirectories(_baseFilePath);
            foreach (var cariDir in cariDirectories)
            {
                var cariId = Path.GetFileName(cariDir).Replace("Cari", "");
                var cari = _context.Cariler.FirstOrDefault(c => c.Id.ToString() == cariId);

                var yearFolders = Directory.GetDirectories(cariDir);
                var yearModels = new List<YearFolderViewModel>();

                foreach (var yearFolder in yearFolders)
                {
                    var year = Path.GetFileName(yearFolder);
                    var files = Directory.GetFiles(yearFolder);

                    var fileModels = files.Select(file => new FileViewModel
                    {
                        FileName = Path.GetFileName(file),
                        FilePath = file
                    }).ToList();

                    yearModels.Add(new YearFolderViewModel
                    {
                        Year = int.Parse(year),
                        Files = fileModels
                    });
                }

                allFilesModel.Add(new DirectoryViewModel
                {
                    CariId = int.Parse(cariId),
                    CariName = cari?.Unvan ?? $"Cari {cariId}", // Cari adı varsa kullan, yoksa fallback
                    YearFolders = yearModels
                });
            }

            return View(allFilesModel);
        }

    }
}
