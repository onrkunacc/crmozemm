using HospitalInventoryManagement.BL.Interfaces;
using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;
using HospitalInventoryManagement.Web.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HospitalInventoryManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IApiService _apiService;

        public AdminController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IApiService apiService, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _apiService = apiService;
            _context = context;
        }

        // Kullanıcı listesi
        public async Task<IActionResult> UserManagement()
        {
            var users = _userManager.Users.ToList();
            var userWithRoles = new List<UserWithRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userWithRoles.Add(new UserWithRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Roles = roles.ToList()
                });
            }

            return View(userWithRoles);
        }

        // Rol yönetimi için görünüm
        public IActionResult RoleManagement()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        // Rol oluşturma işlemi
        [HttpPost]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
            return RedirectToAction("RoleManagement");
        }

        // Kullanıcıya rol atama işlemi
        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _roleManager.RoleExistsAsync(roleName))
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }
            return RedirectToAction("UserManagement");
        }

        public async Task<ActionResult> Index(int page = 1)
        {
            await _apiService.LoginAsync("admin", "123ozem");

            // Tablo ve sayfa numaralarını çek
            var (tableHtml, pageNumbers) = await _apiService.FetchHtmlTableWithPagesAsync(page);

            // View'e verileri gönder
            ViewBag.HtmlTable = tableHtml;
            ViewBag.PageNumbers = pageNumbers;

            return View();
        }

        public IActionResult UserList()
        {
            var users = _userManager.Users
                .Select(u => new AdminUserViewModel
                {
                    UserName = u.UserName,
                    Email = u.Email,
                    HospitalName = u.HospitalID != null && u.HospitalID > 0
                        ? _context.Hospitals.Where(h => h.HospitalID == u.HospitalID).Select(h => h.HospitalName).FirstOrDefault()
                        : "Belirtilmemiş"
                })
                .ToList();

            return View(users);
        }
    }
}
