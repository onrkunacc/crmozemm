// Controllers/StatisticsController.cs
using HospitalInventoryManagement.BL.Interfaces;
using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;
using HospitalInventoryManagement.Web.ViewModel;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using HospitalInventoryManagement.Data.DTOs;

[Authorize]
public class StatisticsController : Controller
{
    private readonly IStatisticsService _statisticsService;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StatisticsController(IStatisticsService statisticsService, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _statisticsService = statisticsService;
        _context = context;
        _userManager = userManager;
    }

    // İstatistik Ekleme (Giriş yapan kullanıcının hastanesine göre)
    [HttpGet]
    public async Task<IActionResult> EnterMonthlyStatistics()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var viewModel = new MonthlyStatisticsViewModel
        {
            HospitalID = user.HospitalID
        };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> EnterMonthlyStatistics(MonthlyStatisticsViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _statisticsService.AddMonthlyStatistics(user.HospitalID, model.Month, model.TestCount, model.TestName);
                TempData["SuccessMessage"] = "İstatistik başarıyla kaydedildi.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");

                // Hata oluşursa hata mesajını TempData'ya ekleyelim
                TempData["ErrorMessage"] = "İstatistik kaydedilirken bir hata oluştu. Lütfen tekrar deneyin.";

            }
            return RedirectToAction("EnterMonthlyStatistics");
        }

        model.HospitalID = user.HospitalID;
        return View(model);
    }

    // Giriş yapan hastaneye ait istatistikleri görüntüleme
    public async Task<IActionResult> ViewHospitalStatistics(DateTime? selectedMonth)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var statistics = _statisticsService.GetStatisticsByHospital(user.HospitalID);
        //Seçili ay var ise ona göre filtreleme yapmak için kullanıyoruz.
        if (selectedMonth.HasValue)
        {
            statistics = statistics
                .Where(s => s.Month.Year == selectedMonth.Value.Year && s.Month.Month == selectedMonth.Value.Month)
                .ToList();
        }

        statistics = statistics.OrderByDescending(s => s.Month).ToList();
        // Verileri StatisticsDTO tipine dönüştürüyoruz
        var statisticsDTO = statistics.Select(s => new StatisticsDTO
        {
            HospitalID = s.HospitalID,
            HospitalName = _context.Hospitals.FirstOrDefault(h => h.HospitalID == s.HospitalID)?.HospitalName, // HospitalName bilgisi
            Month = s.Month,
            TestCount = s.TestCount,
            TestName = s.TestName
        }).ToList();

        // Kullanıcının hastane ID'sini ViewBag ile View'e taşıyoruz
        ViewBag.SelectedMonth = selectedMonth;
        ViewBag.HospitalID = user.HospitalID;

        return View(statisticsDTO); // DTO modelini view'e gönderiyoruz
    }


    //todo: Burada dropdown menü düzenlenecek
    public IActionResult ViewAllStatistics(int? hospitalId, string sortOrder)
    {
        var allStatistics = _statisticsService.GetAllStatistics();

        // Hastane ID'ye göre filtreleme
        if (hospitalId.HasValue)
        {
            allStatistics = allStatistics.Where(s => s.HospitalID == hospitalId.Value).ToList();
        }

        // Verileri StatisticsDTO tipine dönüştürürken HospitalName alanını da dolduruyoruz
        var allStatisticsDTO = allStatistics.Select(s => new StatisticsDTO
        {
            HospitalID = s.HospitalID,
            HospitalName = _context.Hospitals.FirstOrDefault(h => h.HospitalID == s.HospitalID)?.HospitalName,
            Month = s.Month,
            TestCount = s.TestCount,
            TestName = s.TestName
        }).ToList();

        // En yeni aydan en eski aya doğru sıralama 
        allStatisticsDTO = allStatisticsDTO.OrderByDescending(s => s.Month).ToList();

        // Sıralama seçenekleri
        switch (sortOrder)
        {
            case "NameAsc":
                allStatisticsDTO = allStatisticsDTO.OrderBy(s => s.TestName).ToList();
                break;
            case "NameDesc":
                allStatisticsDTO = allStatisticsDTO.OrderByDescending(s => s.TestName).ToList();
                break;
            case "DateAsc":
                allStatisticsDTO = allStatisticsDTO.OrderBy(s => s.Month).ToList();
                break;
            case "DateDesc":
                allStatisticsDTO = allStatisticsDTO.OrderByDescending(s => s.Month).ToList();
                break;
            default:
                break;
        }

        // Hastane listesini dropdown için ViewBag'e ekliyoruz
        ViewBag.Hospitals = new SelectList(_context.Hospitals, "HospitalID", "HospitalName");
        ViewBag.SelectedHospitalID = hospitalId; // Seçili hastane ID'sini ViewBag’e ekliyoruz
        ViewBag.SortOrder = sortOrder;

        return View(allStatisticsDTO); // DTO modelini view'e gönderiyoruz
    }

    public IActionResult ExportToPdf(int hospitalId)
    {
       
        var hospital = _context.Hospitals.FirstOrDefault(h => h.HospitalID == hospitalId);
        if (hospital == null)
        {
            return NotFound("Hastane bulunamadı.");
        }

        var statistics = _statisticsService.GetStatisticsByHospital(hospitalId)
                                       .OrderByDescending(s => s.Month) // En yeni aydan en eskiye sıralıyoruz
                                       .ToList();

        // Bellek akışı
        var stream = new MemoryStream();

        // iTextSharp ile PDF belgesini oluştur
        var document = new Document();
        var writer = PdfWriter.GetInstance(document, stream);
        writer.CloseStream = false; // Bellek akışının otomatik kapanmasını engelliyoruz

        try
        {
            // Türkçe karakter desteği için TrueType font ekleyelim
            var fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
            var baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            var font = new Font(baseFont, 12, Font.NORMAL);

            document.Open();

            // Hastane başlığını ekliyoruz
            var hospitalTitle = new Paragraph($"{hospital.HospitalName}", font)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(hospitalTitle);

            // Aylık istatistik başlığı
            var title = new Paragraph("Aylık İstatistik", font)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            document.Add(title);


            // 3 sütunlu tablo oluşturuyoruz
            var table = new PdfPTable(3)
            {
                WidthPercentage = 100
            };
            table.SetWidths(new float[] { 1f, 2f, 1f });

            // Tablo başlıklarını ekle
            var headers = new string[] { "Ay", "Test Adı", "Test Sayısı" };
            foreach (var header in headers)
            {
                var cell = new PdfPCell(new Phrase(header, font))
                {
                    BackgroundColor = new BaseColor(200, 200, 200),
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5
                };
                table.AddCell(cell);
            }

            // İstatistik verilerini tabloya ekle
            foreach (var stat in statistics)
            {
                table.AddCell(new PdfPCell(new Phrase(stat.Month.ToString("MMMM yyyy", new System.Globalization.CultureInfo("tr-TR")), font)));
                table.AddCell(new PdfPCell(new Phrase(stat.TestName, font)));
                table.AddCell(new PdfPCell(new Phrase(stat.TestCount.ToString(), font))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
            }

            document.Add(table); // Tabloyu PDF'e ekle
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PDF oluşturulurken hata oluştu: {ex.Message}");
            return StatusCode(500, "PDF oluşturulurken bir hata oluştu.");
        }
        finally
        {
            document.Close(); // Belgeyi kapat
            writer.Close(); // PdfWriter'ı kapat
        }

        // Bellek akışının başını ayarla ve dosyayı döndür
        stream.Position = 0;
        return File(stream, "application/pdf", $"{hospital.HospitalName}.pdf");
    }
    public IActionResult ExportToExcel(int hospitalId)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Lisansı kabul et

        // Hastane bilgilerini alalım
        var hospital = _context.Hospitals.FirstOrDefault(h => h.HospitalID == hospitalId);
        if (hospital == null)
        {
            return NotFound("Hastane bulunamadı.");
        }

        var statistics = _statisticsService.GetStatisticsByHospital(hospitalId)
                                       .OrderByDescending(s => s.Month) // En yeni aydan en eskiye sıralıyoruz
                                       .ToList();
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Statistics");

            // Hastane başlığını ilk satıra ekle
            worksheet.Cells[1, 1].Value = $"Hastane: {hospital.HospitalName}";
            worksheet.Cells[1, 1, 1, 3].Merge = true; // İlk üç hücreyi birleştir
            worksheet.Cells[1, 1].Style.Font.Bold = true;

            // İkinci satıra başlıkları ekle
            worksheet.Cells[2, 1].Value = "Ay";
            worksheet.Cells[2, 2].Value = "Test Adı";
            worksheet.Cells[2, 3].Value = "Test Sayısı";

            int row = 3;
            foreach (var stat in statistics)
            {
                worksheet.Cells[row, 1].Value = stat.Month.ToString("MMMM yyyy", new System.Globalization.CultureInfo("tr-TR"));
                worksheet.Cells[row, 2].Value = stat.TestName;
                worksheet.Cells[row, 3].Value = stat.TestCount;
                row++;
            }

            // Bellek akışını dışarı çıkar ve dosya olarak döndür
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{hospital.HospitalName}.xlsx");
        }
    }

    [HttpGet]
    public async Task<IActionResult> FilterStatistics()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        ViewBag.Hospitals = User.IsInRole("Admin")
            ? new SelectList(_context.Hospitals, "HospitalID", "HospitalName")
            : new SelectList(_context.Hospitals.Where(h => h.HospitalID == user.HospitalID), "HospitalID", "HospitalName");

        return View(new MonthlyStatisticsFilterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> FilterStatistics(MonthlyStatisticsFilterViewModel filterModel)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var statistics = _statisticsService.GetStatisticsByDateRange(filterModel.HospitalID, filterModel.StartMonth, filterModel.EndMonth);
        var testSummaries = statistics
            .GroupBy(stat => stat.TestName)
            .Select(g => new MonthlyStatisticsSummaryViewModel
            {
                TestName = g.Key,
                MonthlyTestCounts = g.Select(stat => new MonthlyTestCount
                {
                    Month = stat.Month.ToString("MMMM"), // Örneğin "Ocak 2024" formatında ay adı
                    TestCount = stat.TestCount
                }).ToList()
            })
            .ToList();

        ViewBag.StartMonth = filterModel.StartMonth.ToString("MMMM yyyy");
        ViewBag.EndMonth = filterModel.EndMonth.ToString("MMMM yyyy");

        return View("FilteredStatistics", testSummaries);
    }

    [HttpGet]
    public async Task<IActionResult> ManageTestTemplates()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var templates = _context.TestTemplates
                .Where(t => t.HospitalID == user.HospitalID && t.IsActive)
                .ToList();

            return View(templates);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata oluştu: {ex.Message}");
            TempData["ErrorMessage"] = "Test şablonları yüklenirken bir hata oluştu.";
            return RedirectToAction("Error", "Home");
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddTestTemplate(string testName)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (string.IsNullOrWhiteSpace(testName))
            {
                TempData["ErrorMessage"] = "Test adı boş olamaz.";
                return RedirectToAction("ManageTestTemplates");
            }

            var newTemplate = new TestTemplate
            {
                HospitalID = user.HospitalID,
                TestName = testName,
                IsActive = true
            };

            _context.TestTemplates.Add(newTemplate);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yeni test şablonu başarıyla eklendi.";
            return RedirectToAction("ManageTestTemplates");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata oluştu: {ex.Message}");
            TempData["ErrorMessage"] = "Test şablonu eklenirken bir hata oluştu.";
            return RedirectToAction("ManageTestTemplates");
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTestTemplate(int id)
    {
        try
        {
            var template = await _context.TestTemplates.FindAsync(id);
            if (template == null)
            {
                TempData["ErrorMessage"] = "Test şablonu bulunamadı.";
                return RedirectToAction("ManageTestTemplates");
            }

            template.IsActive = false;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Test şablonu başarıyla kaldırıldı.";
            return RedirectToAction("ManageTestTemplates");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata oluştu: {ex.Message}");
            TempData["ErrorMessage"] = "Test şablonu kaldırılırken bir hata oluştu.";
            return RedirectToAction("ManageTestTemplates");
        }
    }

    [HttpGet]
    public async Task<IActionResult> MonthlyStatisticsEntry(DateTime? selectedMonth)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var templates = _context.TestTemplates
                .Where(t => t.HospitalID == user.HospitalID && t.IsActive)
                .ToList();

            var selectedDate = selectedMonth ?? DateTime.Now;

            var viewModel = new MonthlyStatisticsEntryViewModel
            {
                SelectedMonth = selectedDate,
                Tests = templates.Select(t => new TestEntry
                {
                    TestName = t.TestName,
                    TestCount = 0
                }).ToList()
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata oluştu: {ex.Message}");
            TempData["ErrorMessage"] = "Test giriş ekranı yüklenirken bir hata oluştu. Lütfen tekrar deneyin.";
            return RedirectToAction("Error", "Home");
        }
    }


    [HttpPost]
    public async Task<IActionResult> SaveMonthlyStatistics(MonthlyStatisticsEntryViewModel model)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (model.Tests == null || !model.Tests.Any())
            {
                TempData["ErrorMessage"] = "Test istatistikleri boş olamaz.";
                return RedirectToAction("MonthlyStatisticsEntry");
            }

            foreach (var test in model.Tests)
            {
                if (test.TestCount < 0)
                {
                    TempData["ErrorMessage"] = "Test sayısı negatif olamaz.";
                    return RedirectToAction("MonthlyStatisticsEntry");
                }

                var newStatistic = new MonthlyStatistics
                {
                    HospitalID = user.HospitalID,
                    TestName = test.TestName,
                    TestCount = test.TestCount,
                    Month = model.SelectedMonth ?? DateTime.Now // Seçilen tarihi kullan veya şu anki tarihi kullan
                };

                _context.MonthlyStatistics.Add(newStatistic);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "İstatistikler başarıyla kaydedildi.";
            return RedirectToAction("MonthlyStatisticsEntry");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hata oluştu: {ex.Message}");
            TempData["ErrorMessage"] = "İstatistikler kaydedilirken bir hata oluştu. Lütfen tekrar deneyin.";
            return RedirectToAction("MonthlyStatisticsEntry");
        }
    }
}
