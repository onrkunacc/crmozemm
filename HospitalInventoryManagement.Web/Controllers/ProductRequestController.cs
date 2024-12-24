using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;
using HospitalInventoryManagement.Web.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class ProductRequestController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProductRequestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> RequestProduct()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("AccessDenied", "Account");

        var hospital = _context.Hospitals.FirstOrDefault(h => h.HospitalID == user.HospitalID);

        var viewModel = new ProductRequestViewModel
        {
            RequestedByUserID = user.Id,
            RequestedByUserName = user.UserName,
            RequestedByHospitalName = hospital?.HospitalName
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> RequestProduct(ProductRequestViewModel model)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User); // Giriş yapan kullanıcıyı alıyoruz
            if (user == null)
                return RedirectToAction("AccessDenied", "Account");

            // Hastane bilgilerini kullanıcı üzerinden alıyoruz
            var hospital = _context.Hospitals.FirstOrDefault(h => h.HospitalID == user.HospitalID);

            if (ModelState.IsValid)
            {
                try
                {
                    // Yeni ürün isteği oluşturuluyor
                    var productRequest = new ProductRequest
                    {
                        ProductName = model.ProductName,
                        ReferenceNumber = model.ReferenceNumber,
                        RequestedByUserID = user.Id, // Giriş yapan kullanıcının ID'si otomatik atanıyor
                        RequestDate = DateTime.Now,
                        Status = "Bekliyor" // Varsayılan durum
                    };

                    _context.ProductRequests.Add(productRequest);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Ürün ekleme isteğiniz gönderildi. Admin onayından sonra eklenecektir.";
                    return RedirectToAction("RequestProduct");
                }
                catch (Exception ex)
                {
                    // Hata durumunda loglama ve kullanıcı bilgilendirmesi
                    Console.WriteLine($"Ürün isteği oluşturulurken bir hata oluştu: {ex.Message}");
                    TempData["ErrorMessage"] = "Ürün isteği oluşturulurken bir hata oluştu. Lütfen tekrar deneyiniz.";
                }
            }

            // ModelState geçersizse kullanıcıya gerekli bilgileri tekrar dolduruyoruz
            model.RequestedByUserID = user.Id;
            model.RequestedByUserName = user.UserName;
            model.RequestedByHospitalName = hospital?.HospitalName;

            TempData["ErrorMessage"] = "Formda hata var. Lütfen tüm alanları doldurun.";
            return View(model);
        }
        catch (Exception ex)
        {
            // Ana try-catch bloğu: Kullanıcı ve hastane bilgileri alınırken oluşan hataları yakalamak için
            Console.WriteLine($"Ürün isteği işlemi sırasında bir hata oluştu: {ex.Message}");
            TempData["ErrorMessage"] = "Bir hata oluştu. Lütfen tekrar deneyiniz.";
            return RedirectToAction("Error", "Home"); // Hata sayfasına yönlendirme
        }
    }
}
