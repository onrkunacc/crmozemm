using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Packaging;
using HospitalInventoryManagement.Web.Areas.Invoice.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using HospitalInventoryManagement.Data.Context;
using Microsoft.EntityFrameworkCore;
using Xceed.Words.NET;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;
using HospitalInventoryManagement.BL.Interfaces;
using HospitalInventoryManagement.BL.Service;

namespace YourProject.Areas.Invoice.Controllers
{
    [Area("Invoice")]
    [Authorize(Roles = "Admin,InvoiceRole")]
    public class InvoiceFileController : Controller
    {
        private readonly string _baseFilePath = @"C:\Temp\OdemeYazilari";
        private readonly ApplicationDbContext _context;
        private readonly IFileProccesingService _fileProccesingService;

        public InvoiceFileController(ApplicationDbContext context,IFileProccesingService fileProccesingService)
        {
             _context = context;
            _fileProccesingService = fileProccesingService;
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
            var files = GetFilesFromDirectory(cariId.Value, year.Value);
            return View(files);
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
            if (cari == null || file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Geçersiz cari veya dosya.";
                return RedirectToAction("UploadFile");
            }

            var filePath = Path.Combine(_baseFilePath, $"Cari{cariId}", year.ToString(), file.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

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
           
            if (cariId == 0 || year == 0 || string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine("Parametrelerden biri eksik!");
                return NotFound(new { Message = "Geçersiz parametreler." });
            }

            var filePath = Path.Combine(_baseFilePath, $"Cari{cariId}", year.ToString(), fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new { Message = "Dosya bulunamadı." });
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/octet-stream", fileName);
        }

        [HttpGet]
        public IActionResult UpdateFileView(int cariId, int year, string fileName)
        {
            var filePath = GetFilePath(cariId, year, fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { Message = "Dosya bulunamadı." });

            var htmlContent = _fileProccesingService.ConvertWordToFormattedHtml(filePath);
            var model = new UpdateFileViewModel
            {
                CariId = cariId,
                Year = year,
                FileName = fileName,
                FileContent = htmlContent
            };

            return View(model);
        }

        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateFileView(UpdateFileViewModel model)
        {
            var filePath = GetFilePath(model.CariId, model.Year, model.FileName);
            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = "Dosya bulunamadı.";
                return RedirectToAction("UpdateFileView", new { cariId = model.CariId, year = model.Year, fileName = model.FileName });
            }

            try
            {
                _fileProccesingService.UpdateWordDocumentFromHtml(filePath, model.FileContent);
                TempData["SuccessMessage"] = "Dosya başarıyla kaydedildi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Dosya kaydedilirken hata oluştu: {ex.Message}";
                return RedirectToAction("UpdateFileView", new { cariId = model.CariId, year = model.Year, fileName = model.FileName });
            }

            return RedirectToAction("Index", new { cariId = model.CariId, year = model.Year });
        }

        private List<FileViewModel> GetFilesFromDirectory(int cariId, int year)
        {
            var cariFolder = Path.Combine(_baseFilePath, $"Cari{cariId}", year.ToString());
            if (!Directory.Exists(cariFolder))
                return new List<FileViewModel>();

            return Directory.GetFiles(cariFolder)
                .Select(file => new FileViewModel
                {
                    FileName = Path.GetFileName(file),
                    FilePath = file
                })
                .ToList();
        }

        private string GetFilePath(int cariId, int year, string fileName)
        {
            return Path.Combine(_baseFilePath, $"Cari{cariId}", year.ToString(), fileName);
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
                    var folderName = Path.GetFileName(yearFolder);

                    // Hatalı klasörleri logla
                    Console.WriteLine($"Klasör adı kontrol ediliyor: {folderName}");

                    // Yalnızca yıl formatındaki klasörleri dahil et
                    if (int.TryParse(folderName, out var year) && year >= 1900 && year <= 2100)
                    {
                        Console.WriteLine($"Geçerli yıl klasörü bulundu: {folderName}");

                        var files = Directory.GetFiles(yearFolder);
                        var fileModels = files.Select(file => new FileViewModel
                        {
                            FileName = Path.GetFileName(file),
                            FilePath = file
                        }).ToList();

                        yearModels.Add(new YearFolderViewModel
                        {
                            Year = year,
                            Files = fileModels
                        });
                    }
                    else
                    {
                        // Geçersiz klasörlerin loglanması
                        Console.WriteLine($"Geçersiz klasör bulundu: {folderName}");
                    }
                }

                if (int.TryParse(cariId, out var parsedCariId))
                {
                    allFilesModel.Add(new DirectoryViewModel
                    {
                        CariId = parsedCariId,
                        CariName = cari?.Unvan ?? $"Cari {cariId}",
                        YearFolders = yearModels
                    });
                }
                else
                {
                    Console.WriteLine($"Geçersiz Cari ID: {cariId}");
                }
            }

            return View(allFilesModel);
        }

        [HttpGet]
        public IActionResult PrintFile(int cariId, int year, string fileName)
        {
            var filePath = Path.Combine(_baseFilePath, $"Cari{cariId}", year.ToString(), fileName);

            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = "Dosya bulunamadı.";
                return RedirectToAction("Index", new { cariId, year });
            }

            // Word dosyasını HTML olarak okuyalım (örnek: FileContent'e yükleyelim)
            var htmlContent = _fileProccesingService.ConvertWordToFormattedHtml(filePath);

            var model = new UpdateFileViewModel
            {
                CariId = cariId,
                Year = year,
                FileName = fileName,
                FileContent = htmlContent // Burada FileContent'i set ediyoruz.
            };

            return View("PrintFile", model);
        }
       
    }
}
