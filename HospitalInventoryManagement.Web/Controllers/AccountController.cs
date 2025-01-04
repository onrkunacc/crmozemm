using System.Security.Claims;
using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;
using HospitalInventoryManagement.Web.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalInventoryManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.Hospitals = new SelectList(_context.Hospitals, "HospitalID", "HospitalName");
            // Hastaneleri doğrudan ApplicationDbContext üzerinden alıyoruz
            return View();
        }

        [Authorize(Roles = "Admin")]
        // Register POST
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Email'den UserName türetme
                var userName = model.Email.Split('@')[0];

                var user = new ApplicationUser
                {
                    UserName = userName, // Kullanıcı adını mail adresinden türettik
                    Email = model.Email,
                    HospitalID = model.HospitalID
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Varsayılan olarak "User" rolü atanıyor
                    if (!await _roleManager.RoleExistsAsync("User"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("User"));
                    }
                    await _userManager.AddToRoleAsync(user, "User");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            ViewBag.Hospitals = new SelectList(_context.Hospitals, "HospitalID", "HospitalName");
            return View(model);
        }
        // Login GET
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Login POST
        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Email)
                         ?? await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(
                        user.UserName,
                        model.Password,
                        model.RememberMe,
                        lockoutOnFailure: false
                    );

                    if (result.Succeeded)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        if (roles.Contains("DocumentRole"))
                        {
                            return RedirectToAction("Index", "Document", new { area = "Document" });
                        }
                        else if (roles.Contains("InvoiceRole"))
                        {
                            return RedirectToAction("Index", "Invoice", new { area = "Invoice" });
                        }
                        else if(roles.Contains("Admin"))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            // Kullanıcı başarılı şekilde giriş yaptı
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }

                ModelState.AddModelError(string.Empty, "Geçersiz giriş denemesi.");
            }

            return View(model);
        }

        // Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
