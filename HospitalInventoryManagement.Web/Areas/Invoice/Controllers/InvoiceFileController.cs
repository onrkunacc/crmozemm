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

                    // Word dosyasını düzenlenebilir HTML'ye çevir
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
                using (var wordDoc = WordprocessingDocument.Open(filePath, true))
                {
                    var body = wordDoc.MainDocumentPart.Document.Body;

                    // Mevcut içeriği temizle
                    body.RemoveAllChildren();

                    // CKEditor’den gelen HTML’yi ekle
                    var paragraphs = model.FileContent.Split(new[] { "<p>", "</p>" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var paragraph in paragraphs)
                    {
                        var p = new Paragraph(new Run(new Text(paragraph.Trim())));
                        body.AppendChild(p);
                    }

                    wordDoc.MainDocumentPart.Document.Save();
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

        private string ConvertWordToFormattedHtml(Body body)
        {
            var htmlBuilder = new StringBuilder();

            foreach (var element in body.ChildElements)
            {
                if (element is Paragraph paragraphElement)
                {
                    string paragraphStyle = "text-align: left;";

                    // Paragraf hizalama
                    var alignmentValue = paragraphElement.ParagraphProperties?.Justification?.Val;
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

                    foreach (var run in paragraphElement.Elements<Run>())
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
                else if (element is Table tableElement)
                {
                    htmlBuilder.Append("<table style='border-collapse: collapse; width: 50%; margin: 0 auto; border: 1px solid black;'>");

                    foreach (var row in tableElement.Elements<TableRow>())
                    {
                        htmlBuilder.Append("<tr>");

                        foreach (var cell in row.Elements<TableCell>())
                        {
                            string cellStyle = "border: 1px solid black; padding: 5px; text-align: left;";
                            int colSpan = 1;
                            int rowSpan = 1;

                            // Sütun birleşimlerini kontrol et
                            var gridSpan = cell.TableCellProperties?.GridSpan?.Val;
                            if (gridSpan != null)
                            {
                                colSpan = int.Parse(gridSpan.InnerText);
                            }

                            // Satır birleşimlerini kontrol et
                            var vMerge = cell.TableCellProperties?.VerticalMerge;
                            if (vMerge != null && vMerge.Val != null)
                            {
                                if (vMerge.Val.Value == MergedCellValues.Restart)
                                {
                                    rowSpan = GetRowSpan(cell);
                                }
                                else if (vMerge.Val.Value == MergedCellValues.Continue)
                                {
                                    continue;
                                }
                            }

                            // Hücreyi oluştur
                            htmlBuilder.Append($"<td style='{cellStyle}' colspan='{colSpan}' rowspan='{rowSpan}'>");

                            var combinedText = new StringBuilder();

                            // Hücre içeriğini birleştir
                            foreach (var cellParagraph in cell.Elements<Paragraph>())
                            {
                                foreach (var run in cellParagraph.Elements<Run>())
                                {
                                    foreach (var text in run.Elements<Text>())
                                    {
                                        combinedText.Append(text.Text.Trim() + " ");
                                    }
                                }
                            }

                            // Fatura Tarihi ve Numarası kontrolü
                            var cellContent = combinedText.ToString().Trim();
                            if (cellContent.Contains("Fatura Tarihi") || cellContent.Contains("Fatura Numarası"))
                            {
                                // Özel işlem gerekirse burada yapabilirsiniz
                                cellContent = cellContent.Replace("Fatura Tarihi", "").Replace("Fatura Numarası", "").Trim();
                            }

                            htmlBuilder.Append(cellContent);
                            htmlBuilder.Append("</td>");
                        }

                        htmlBuilder.Append("</tr>");
                    }

                    htmlBuilder.Append("</table>");
                }
            }

            return htmlBuilder.ToString();
        }

        // Satır birleşimlerini çözümlemek için yardımcı metot
        private int GetRowSpan(TableCell cell)
        {
            var rowSpan = 1;
            var nextCell = cell;

            while (nextCell != null)
            {
                var vMerge = nextCell.TableCellProperties?.VerticalMerge;
                if (vMerge == null || vMerge.Val == null || vMerge.Val.Value != MergedCellValues.Continue)
                {
                    break;
                }

                rowSpan++;
                nextCell = GetNextCellInRow(nextCell);
            }

            return rowSpan;
        }

        // Satırdaki bir sonraki hücreyi bulmak için bir yardımcı metot
        private TableCell GetNextCellInRow(TableCell currentCell)
        {
            var row = currentCell.Ancestors<TableRow>().FirstOrDefault();
            if (row == null) return null;

            var cells = row.Elements<TableCell>().ToList();
            var index = cells.IndexOf(currentCell);

            if (index >= 0 && index < cells.Count - 1)
            {
                return cells[index + 1];
            }

            return null;
        }
    }
}
