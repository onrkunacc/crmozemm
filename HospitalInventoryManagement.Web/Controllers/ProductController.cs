using HospitalInventoryManagement.BL.Interfaces;
using HospitalInventoryManagement.BL.Service;
using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;
using HospitalInventoryManagement.Web.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;

namespace HospitalInventoryManagement.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProductController(IProductService productService, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _productService = productService;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index(int page = 1, int pageSize = 20)
        {
            var products = _context.Products
                .Include(p => p.Category) // Kategori ilişkisini dahil et
                .Include(p => p.Platform) // Platform ilişkisini dahil et
                .Include(p => p.ProductType)
                .OrderBy(p => p.ProductName);

            var pagedList = products.ToPagedList(page, pageSize);

            return View(pagedList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryID", "CategoryName");
            ViewBag.ProductTypes = new SelectList(_context.ProductTypes, "TypeID", "TypeName");
            ViewBag.Platforms = new SelectList(_context.Platforms, "PlatformID", "PlatformName");
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductCreateViewModel model)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // ViewModel'den Product modeline dönüştürme
                    var product = new Product
                    {
                        ProductName = model.ProductName,
                        ReferenceNumber = model.ReferenceNumber,
                        FlaconCountPerBox = model.FlaconCountPerBox,
                        BoxTextCount = model.BoxTextCount,
                        CategoryID = model.CategoryID,
                        TypeID = model.TypeID,
                        PlatformID = model.PlatformID
                    };

                    // Ürünü kaydetme işlemi
                    _productService.AddProduct(product);
                    TempData["SuccessMessage"] = "Ürün başarıyla oluşturuldu.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Hata durumunda loglama yapılabilir
                    Console.WriteLine($"Ürün oluşturulurken hata oluştu: {ex.Message}");
                    TempData["ErrorMessage"] = "Ürün kaydedilirken bir hata oluştu. Lütfen tekrar deneyiniz.";
                }
            }

            // ModelState geçerli değilse veya hata oluşursa, tekrar ViewData öğelerini doldurun
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryID", "CategoryName");
            ViewBag.ProductTypes = new SelectList(_context.ProductTypes, "TypeID", "TypeName");
            ViewBag.Platforms = new SelectList(_context.Platforms, "PlatformID", "PlatformName");

            // Kullanıcıyı aynı sayfaya geri yönlendirin
            return View(model);
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            // Yalnızca Admin rolündeki kullanıcıların erişimini sağlıyoruz
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var product = _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }

            var model = new ProductEditViewModel
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                ReferenceNumber = product.ReferenceNumber,
                FlaconCountPerBox = product.FlaconCountPerBox,
                BoxTextCount = product.BoxTextCount,
                CategoryID = product.CategoryID,
                TypeID = product.TypeID,
                PlatformID = product.PlatformID
            };

            PopulateDropdowns(model);
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(ProductEditViewModel model)
        {
            // Yalnızca Admin rolündeki kullanıcıların erişimini sağlıyoruz
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model);
                return View(model);
            }

            try
            {
                var product = _productService.GetProductById(model.ProductID);
                if (product == null)
                {
                    ModelState.AddModelError("", "Ürün bulunamadı.");
                    PopulateDropdowns(model);
                    return View(model);
                }

                // Ürün güncelleme işlemleri
                product.ProductName = model.ProductName;
                product.ReferenceNumber = model.ReferenceNumber;
                product.FlaconCountPerBox = model.FlaconCountPerBox;
                product.BoxTextCount = model.BoxTextCount;
                product.CategoryID = model.CategoryID;
                product.TypeID = model.TypeID;
                product.PlatformID = model.PlatformID;

                _productService.UpdateProduct(product);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
                ModelState.AddModelError("", "Ürün güncellenirken bir hata oluştu. Lütfen tekrar deneyin.");
                PopulateDropdowns(model);
                return View(model);
            }
        }
        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            var product = _context.Products
                                .Include(s => s.Category)
                                .Include(s => s.ProductType)
                                .Include(s => s.Platform)
                                .FirstOrDefault(s => s.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product); // Silme onay sayfasını gösterir
        }

        [HttpPost]
        public IActionResult Delete(int id, string confirm = "yes")
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("AccessDenied", "Account");
            }
            Product product = null; // stock değişkenini try bloğu dışında tanımlıyoruz

            try
            {
                product = _context.Products
                                .Include(s => s.Category)
                                .Include(s => s.ProductType)
                                .Include(s => s.Platform)
                                .FirstOrDefault(s => s.ProductID == id);

                if (product == null)
                {
                    ModelState.AddModelError("", "Silinmek istenen ürün bulunamadı.");
                    return RedirectToAction("Index");
                }

                _productService.DeleteProduct(id);
                return RedirectToAction("Index"); // Başarıyla silindikten sonra listeye döner
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
                ModelState.AddModelError("", "Stok silinirken bir hata oluştu. Bu stok başka tablolarla ilişkili olabilir ve silinemiyor olabilir.");
                return View(product); // Hata olduğunda aynı silme sayfasına geri döner
            }
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        private void PopulateDropdowns(ProductEditViewModel model)
        {
            model.Categories = new SelectList(_context.Categories, "CategoryID", "CategoryName");
            model.ProductTypes = new SelectList(_context.ProductTypes, "TypeID", "TypeName");
            model.Platforms = new SelectList(_context.Platforms, "PlatformID", "PlatformName");
        }
    }
}
