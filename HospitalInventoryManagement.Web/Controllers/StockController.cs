using HospitalInventoryManagement.BL.Extensions;
using HospitalInventoryManagement.BL.Interfaces;
using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;
using HospitalInventoryManagement.Web.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalInventoryManagement.Web.Controllers
{
    public class StockController : Controller
    {
        private readonly IStockService _stockService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StockController(IStockService stockService, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _stockService = stockService;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string searchQuery)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Tüm stokları veya hastane bazlı stokları getir
                var stocksQuery = _context.Stocks.AsQueryable();

                if (User.IsInRole("Admin"))
                {
                    stocksQuery = stocksQuery
                        .Include(s => s.Product)
                        .ThenInclude(p => p.Category)
                        .Include(s => s.Product.ProductType)
                        .Include(s => s.Product.Platform)
                        .Include(s => s.Hospital);
                }
                else
                {
                    stocksQuery = stocksQuery
                        .Where(s => s.HospitalID == user.HospitalID)
                        .Include(s => s.Product)
                        .ThenInclude(p => p.Category)
                        .Include(s => s.Product.ProductType)
                        .Include(s => s.Product.Platform)
                        .Include(s => s.Hospital);
                }

                if (!string.IsNullOrEmpty(searchQuery))
                {
                    stocksQuery = stocksQuery.Where(s => s.Product.ProductName.Contains(searchQuery));
                }

                var stocks = stocksQuery.ToList();

                // ViewBag ile arama terimini gönder
                ViewBag.SearchQuery = searchQuery;

                return View(stocks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bir hata oluştu: {ex.Message}");
                return RedirectToAction("Error", "Home", new { message = "Stokları yüklerken bir hata oluştu." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // Kullanıcının hastane ID'sine göre ürün listesini dolduruyoruz
            var viewModel = new StockViewModel
            {
                Products = new SelectList(_context.Products, "ProductID", "ProductName")
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(StockViewModel viewModel)
        {

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // ModelState'ten Products, Hospitals, LotNumber ve ReferenceNumber alanlarını kaldırıyoruz
            ModelState.Remove("Products");
            ModelState.Remove("Hospitals");
            ModelState.Remove("LotNumber");
            ModelState.Remove("ReferenceNumber");

            // ModelState hatalarını loglama
            if (!ModelState.IsValid)
            {
                viewModel.Products = new SelectList(_context.Products, "ProductID", "ProductName");
                return View(viewModel);
            }

            try
            {
                var existingStock = _context.Stocks.FirstOrDefault(s =>
                        s.ProductID == viewModel.ProductID &&
                        s.LotNumber == viewModel.LotNumber &&
                        s.HospitalID == user.HospitalID);

                if (existingStock != null)
                {
                    // Mevcut stok güncellemesi
                    existingStock.Quantity += viewModel.Quantity;
                    existingStock.ExpiryDate = viewModel.ExpirationDate;
                    existingStock.LastUpdated = DateTime.Now;

                    _context.Stocks.Update(existingStock);
                }
                else
                {
                    // Yeni stok oluşturma
                    var stock = new Stock
                    {
                        ProductID = viewModel.ProductID,
                        HospitalID = user.HospitalID,
                        Quantity = viewModel.Quantity,
                        LotNumber = viewModel.LotNumber,
                        ExpiryDate = viewModel.ExpirationDate,
                        LastUpdated = DateTime.Now
                    };

                    _context.Stocks.Add(stock);
                }

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Stok girişi başarılı bir şekilde yapıldı.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
                ModelState.AddModelError("", "Stok oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.");
                viewModel.Products = new SelectList(_context.Products, "ProductID", "ProductName");
                return View(viewModel);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var stock = _stockService.GetStockById(id);
            if (stock == null || (!User.IsInRole("Admin") && stock.HospitalID != user.HospitalID))
            {
                return NotFound();
            }

            var model = new StockViewModel
            {
                StockID = stock.StockID,
                ProductID = stock.ProductID,
                HospitalID = stock.HospitalID,
                Quantity = stock.Quantity,
                ExpirationDate = stock.ExpiryDate
            };

            PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(StockViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null || (!User.IsInRole("Admin") && model.HospitalID != user.HospitalID))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            // ModelState'ten Products ve Hospitals alanlarını kaldırdım
            ModelState.Remove("Products");
            ModelState.Remove("Hospitals");
            ModelState.Remove("LotNumber");
            ModelState.Remove("ReferenceNumber");

            // ModelState hata loglama
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"ModelState Hatası: {error.ErrorMessage}");
                }

                PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                var stock = _stockService.GetStockById(model.StockID);
                if (stock == null)
                {
                    ModelState.AddModelError("", "Stok bulunamadı.");
                    PopulateDropdowns(model);
                    return View(model);
                }

                Console.WriteLine($"Eski Quantity: {stock.Quantity}, Yeni Quantity: {model.Quantity}");

                // Güncelleme işlemi
                stock.StockID = model.StockID;
                stock.ProductID = model.ProductID;
                stock.HospitalID = model.HospitalID;
                stock.Quantity = model.Quantity;
                stock.ExpiryDate = model.ExpirationDate;
                stock.LastUpdated = DateTime.Now;

                _stockService.UpdateStock(stock);

                var updatedStock = _stockService.GetStockById(model.StockID);
                Console.WriteLine($"Güncellenmiş Quantity: {updatedStock.Quantity}");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
                ModelState.AddModelError("", "Stok güncellenirken bir hata oluştu. Lütfen tekrar deneyin.");
                PopulateDropdowns(model);
                return View(model);
            }
        }
        [HttpGet]
        public JsonResult GetProductDetails(int productId)
        {
            var product = _context.Products
                            .Where(p => p.ProductID == productId)
                            .Select(p => new {
                                referenceNumber = p.ReferenceNumber
                            })
                            .FirstOrDefault();

            return Json(product);
        }

        private void PopulateDropdowns(StockViewModel model)
        {
            model.Products = new SelectList(_context.Products, "ProductID", "ProductName", model.ProductID);
            model.Hospitals = new SelectList(_context.Hospitals, "HospitalID", "HospitalName", model.HospitalID);
        }

        [HttpGet]
        public async Task<IActionResult> SelectStock()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var stocks = User.IsInRole("Admin")
                ? _stockService.GetAllStocks()
                : _stockService.GetStocksByHospital(user.HospitalID);

            var stockSelectList = stocks.Select(s => new
            {
                StockID = s.StockID,
                DisplayText = $"{s.Product.ProductName} - Lot: {s.LotNumber}"
            }).ToList();

            ViewBag.Stocks = new SelectList(stockSelectList, "StockID", "DisplayText");

            return View();
        }

        // Seçilen stoğa göre düzenleme sayfasına yönlendirme
        [HttpPost]
        public async Task<IActionResult> SelectStock(int stockId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var stock = _stockService.GetStockById(stockId);
            if (stock == null || (!User.IsInRole("Admin") && stock.HospitalID != user.HospitalID))
            {
                ModelState.AddModelError("", "Lütfen geçerli bir stok ürünü seçin.");
                ViewBag.Stocks = new SelectList(_stockService.GetStocksByHospital(user.HospitalID), "StockID", "Product.ProductName");
                return View();
            }

            return RedirectToAction("Edit", new { id = stockId });
        }
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var stock = _context.Stocks
                                .Include(s => s.Product)
                                .Include(s => s.Hospital)
                                .FirstOrDefault(s => s.StockID == id);

            if (stock == null)
            {
                return NotFound();
            }

            return View(stock); // Silme onay sayfasını gösterir
        }

        [HttpPost]
        public IActionResult Delete(int id, string confirm = "yes")
        {
            Stock stock = null; // stock değişkenini try bloğu dışında tanımlıyoruz

            try
            {
                stock = _context.Stocks
                                .Include(s => s.Product)
                                .Include(s => s.Hospital)
                                .FirstOrDefault(s => s.StockID == id);

                if (stock == null)
                {
                    ModelState.AddModelError("", "Silinmek istenen stok bulunamadı.");
                    return RedirectToAction("Index");
                }

                _stockService.DeleteStock(id);
                return RedirectToAction("Index"); // Başarıyla silindikten sonra listeye döner
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
                ModelState.AddModelError("", "Stok silinirken bir hata oluştu. Bu stok başka tablolarla ilişkili olabilir ve silinemiyor olabilir.");
                return View(stock); // Hata olduğunda aynı silme sayfasına geri döner
            }
        }

        [HttpGet]
        public async Task<IActionResult> BarcodeStockEntry()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var viewModel = new BarcodeStockTransactionViewModel
            {
                TransactionType = "Giriş"
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> BarcodeStockEntry(BarcodeStockTransactionViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                var product = _context.Products.FirstOrDefault(p => p.ReferenceNumber == viewModel.ReferenceNumber);
                if (product == null)
                {
                    ModelState.AddModelError("", "Ürün bulunamadı. Lütfen geçerli bir referans numarası girin.");
                    return View(viewModel);
                }

                
                var existingStock = _context.Stocks.FirstOrDefault(s =>
                    s.ProductID == product.ProductID &&
                    s.LotNumber == viewModel.LotNumber &&
                    s.HospitalID == user.HospitalID);

                if (existingStock != null)
                {
                    existingStock.Quantity += viewModel.Quantity;
                    existingStock.ExpiryDate = viewModel.ExpiryDate;
                    existingStock.LastUpdated = DateTime.Now;

                    _context.Stocks.Update(existingStock);
                }
                else
                {
                    // Yeni stok oluştur
                    var newStock = new Stock
                    {
                        ProductID = product.ProductID,
                        HospitalID = user.HospitalID,
                        Quantity = viewModel.Quantity,
                        LotNumber = viewModel.LotNumber,
                        ExpiryDate = viewModel.ExpiryDate,
                        LastUpdated = DateTime.Now,
                        FlaconCount = product.FlaconCountPerBox
                    };

                    _context.Stocks.Add(newStock);
                }

                // Değişiklikleri kaydet
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Stok girişi başarılı bir şekilde yapıldı.";
                return RedirectToAction("BarcodeStockEntry");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
                ModelState.AddModelError("", "Stok işlemi yapılırken bir hata oluştu. Lütfen tekrar deneyin.");
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> BarcodeStockExit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var viewModel = new BarcodeStockTransactionViewModel
            {
                TransactionType = "Çıkış"
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> BarcodeStockExit(BarcodeStockTransactionViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            try
            {
                // Hastane ID'sine göre stok bulmak için `hospitalId` parametresini ekleyin
                var stock = _stockService.GetStockByReferenceNumber(viewModel.ReferenceNumber, user.HospitalID);

                if (stock == null || stock.HospitalID != user.HospitalID)
                {
                    ModelState.AddModelError("", "Stok bulunamadı veya bu stok üzerinde işlem yapma yetkiniz yok.");
                    return View(viewModel);
                }

                if (stock.Quantity < viewModel.Quantity)
                {
                    ModelState.AddModelError("", "Yetersiz stok miktarı. Çıkış yaparken stok miktarı 0'ın altına düşemez.");
                    return View(viewModel);
                }

                // Stok çıkış işlemi (expiryDate ve lotNumber dahil edildi)
                _stockService.ProcessStockTransaction(
                    viewModel.ReferenceNumber,
                    -viewModel.Quantity, // Çıkış işlemi için negatif miktar
                    user.HospitalID,
                    viewModel.LotNumber,
                    viewModel.ExpiryDate
                );

                TempData["SuccessMessage"] = "Stok çıkışı başarılı bir şekilde yapıldı.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
                ModelState.AddModelError("", "Stok işlemi yapılırken bir hata oluştu. Lütfen tekrar deneyin.");
                return View(viewModel);
            }
        }

        [HttpGet]
        public JsonResult GetProductDetailsByBarcode(string barcode)
        {
            var barcodeData = BarcodeParser.Parse(barcode);

            if (barcodeData.ReferenceNumber == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı." });
            }

            var product = _context.Products
                .Where(p => p.ReferenceNumber == barcodeData.ReferenceNumber)
                .Select(p => new
                {
                    ProductName = p.ProductName
                })
                .FirstOrDefault();

            if (product == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı." });
            }

            // Ürün bilgisi başarıyla bulundu; JSON ile döndür.
            return Json(new
            {
                success = true,
                product = new
                {
                    referenceNumber = barcodeData.ReferenceNumber,
                    productName = product.ProductName,
                    lotNumber = barcodeData.LotNumber, // Barkoddan gelen Lot Number
                    expiryDate = barcodeData.ExpiryDate?.ToString("dd.MM.yyyy") ?? "Bilinmiyor" // Barkoddan gelen son kullanma tarihi
                }
            });
        }

        [HttpGet]
        public JsonResult GetProductByReferenceNumber(string referenceNumber)
        {
            var product = _context.Products
                            .Where(p => p.ReferenceNumber == referenceNumber)
                            .Select(p => new
                            {
                                ProductID = p.ProductID,
                                ReferenceNumber = p.ReferenceNumber,
                                ProductName = p.ProductName
                            })
                            .FirstOrDefault();

            if (product == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı." });
            }

            return Json(new
            {
                success = true,
                product = new
                {
                    productId = product.ProductID,
                    referenceNumber = product.ReferenceNumber,
                    productName = product.ProductName
                }
            });
        }

        [HttpGet]
        public IActionResult ManageFlacon(int id)
        {
            var stock = _stockService.GetStockById(id);

            if (stock == null)
            {
                TempData["ErrorMessage"] = "Seçilen stok bulunamadı.";
                return RedirectToAction("Index");
            }

            if (stock.Product == null)
            {
                TempData["ErrorMessage"] = "Stok ilişkili ürün bulunamadı.";
                return RedirectToAction("Index");
            }

            if (stock.Hospital == null)
            {
                TempData["ErrorMessage"] = "Stok ilişkili hastane bulunamadı.";
                return RedirectToAction("Index");
            }

            // Product tablosundan flakon sayısını alıp Stock tablosundaki değeri eşitle
            if (stock.FlaconCount == 0)
            {
                stock.FlaconCount = stock.Product.FlaconCountPerBox;
                _stockService.UpdateStock(stock); // Güncelleme işlemi
            }

            var viewModel = new FlaconManagementViewModel
            {
                StockID = stock.StockID,
                ProductName = stock.Product.ProductName,
                CurrentFlaconCount = stock.FlaconCount, // Stok tablosundaki flakon sayısı
                HospitalName = stock.Hospital.HospitalName
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult ManageFlacon(FlaconManagementViewModel model)
        {
            try
            {
                var stock = _stockService.GetStockById(model.StockID);
                if (stock == null)
                {
                    TempData["ErrorMessage"] = "Stok bulunamadı.";
                    return RedirectToAction("Index");
                }

                if (model.ChangeAmount <= 0)
                {
                    TempData["ErrorMessage"] = "Geçerli bir flakon miktarı giriniz.";
                    return RedirectToAction("ManageFlacon", new { id = model.StockID });
                }

                if (stock.FlaconCount < model.ChangeAmount)
                {
                    TempData["ErrorMessage"] = "Yeterli flakon yok.";
                    return RedirectToAction("ManageFlacon", new { id = model.StockID });
                }

                // Flakon çıkışı
                stock.FlaconCount -= model.ChangeAmount;

                // Eğer flakonlar tamamen bittiyse, kutu (quantity) azalt
                while (stock.FlaconCount <= 0 && stock.Quantity > 0)
                {
                    stock.Quantity -= 1;

                    // Yeni kutuya geçiş: flakon sayısını ürünün kutu başına flakon sayısına eşitle
                    stock.FlaconCount = stock.Product.FlaconCountPerBox;

                    if (stock.Quantity == 0)
                    {
                        // Eğer quantity 0 olduysa, stoğu kaldırabilirsin (opsiyonel)
                        _stockService.DeleteStock(stock.StockID);
                        TempData["SuccessMessage"] = "Stok tamamen tükendi ve kaldırıldı.";
                        return RedirectToAction("Index");
                    }
                }

                // Normal güncelleme
                _stockService.UpdateStock(stock);

                TempData["SuccessMessage"] = $"Flakon çıkışı yapıldı. Kalan flakon: {stock.FlaconCount}, Kalan Kutu: {stock.Quantity}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
                TempData["ErrorMessage"] = "Flakon değişim işlemi sırasında bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

    }
}
