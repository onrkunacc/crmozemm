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
            // Parametrelerin loglanması
            Console.WriteLine($"Gelen Parametreler - CariId: {cariId}, Year: {year}, FileName: {fileName}");

            if (cariId == 0 || year == 0 || string.IsNullOrEmpty(fileName))
            {
                Console.WriteLine("Parametrelerden biri eksik!");
                return NotFound(new { Message = "Geçersiz parametreler." });
            }

            var filePath = Path.Combine(_baseFilePath, $"Cari{cariId}", year.ToString(), fileName);

            if (!System.IO.File.Exists(filePath))
            {
                Console.WriteLine($"Dosya bulunamadı: {filePath}");
                return NotFound(new { Message = "Dosya bulunamadı." });
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
                Console.WriteLine($"Dosya bulunamadı: {filePath}");
                return NotFound(new { Message = "Dosya bulunamadı." });
            }

            string htmlContent;

            try
            {
                using (var wordDoc = WordprocessingDocument.Open(filePath, false))
                {
                    var body = wordDoc.MainDocumentPart.Document.Body;

                    // Daha iyi HTML formatı oluştur
                    htmlContent = ConvertWordToFormattedHtml(body);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return StatusCode(500, "Dosya yükleme sırasında hata oluştu.");
            }

            var model = new UpdateFileViewModel
            {
                CariId = cariId,
                Year = year,
                FileName = fileName,
                FileContent = htmlContent
            };

            return View(model);
        }

        private string ConvertWordToFormattedHtml(Body body)
        {
            var htmlBuilder = new StringBuilder();

            foreach (var paragraph in body.Elements<Paragraph>())
            {
                // Varsayılan olarak sola hizala
                string paragraphStyle = "text-align: left;";

                var alignmentValue = paragraph.ParagraphProperties?.Justification?.Val;

                if (alignmentValue != null)
                {
                    if (alignmentValue.Value == JustificationValues.Center)
                    {
                        paragraphStyle = "text-align: center;";
                    }
                    else if (alignmentValue.Value == JustificationValues.Right)
                    {
                        paragraphStyle = "text-align: right;";
                    }
                }

                htmlBuilder.Append($"<p style='{paragraphStyle}'>");

                foreach (var run in paragraph.Elements<Run>())
                {
                    var bold = run.RunProperties?.Bold != null ? "font-weight: bold;" : "";
                    var italic = run.RunProperties?.Italic != null ? "font-style: italic;" : "";
                    var fontSize = run.RunProperties?.FontSize?.Val != null
                        ? $"font-size: {run.RunProperties.FontSize.Val}px;"
                        : "";

                    var textStyle = $"{bold} {italic} {fontSize}";

                    foreach (var text in run.Elements<Text>())
                    {
                        htmlBuilder.Append($"<span style='{textStyle}'>{text.Text}</span>");
                    }
                }

                htmlBuilder.Append("</p>");
            }

            return htmlBuilder.ToString();
        }



        [HttpPost]
        public IActionResult UpdateFile(UpdateFileViewModel model)
        {
            var filePath = Path.Combine(_baseFilePath, $"Cari{model.CariId}", model.Year.ToString(), model.FileName);

            if (!System.IO.File.Exists(filePath))
            {
                Console.WriteLine($"Dosya bulunamadı: {filePath}");
                return NotFound(new { Message = "Dosya bulunamadı." });
            }

            try
            {
                using (var doc = DocX.Load(filePath))
                {
                    // Mevcut paragrafları temizle
                    var paragraphs = doc.Paragraphs.ToList();
                    foreach (var paragraph in paragraphs)
                    {
                        doc.RemoveParagraph(paragraph);
                    }

                    // Summernote HTML içeriğini yeni paragraflar olarak ekle
                    doc.InsertParagraph(model.FileContent);

                    // Dosyayı kaydet
                    doc.Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return StatusCode(500, "Dosya güncelleme sırasında hata oluştu.");
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


    }
}
