using HospitalInventoryManagement.BL.Interfaces;
using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;
using HospitalInventoryManagement.Web.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalInventoryManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;

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

        [HttpGet]
        public async Task<JsonResult> GetHospitalStocks(int? hospitalId)
        {
            // Eğer hospitalId null ise tüm hastaneleri al
            var query = _context.Stocks.AsQueryable();

            if (hospitalId.HasValue)
            {
                query = query.Where(s => s.HospitalID == hospitalId.Value);
            }

            // Gruplandırılmış veriyi çek
            var stockData = await query
                .GroupBy(s => s.Hospital.HospitalName)
                .Select(g => new
                {
                    HospitalName = g.Key,
                    TotalStock = g.Sum(s => s.Quantity) // Toplam stok miktarı
                })
                .ToListAsync();

            return Json(stockData);
        }
        public async Task<ActionResult> Index()
        {
            // Hastane listesini ViewBag'e aktar
            ViewBag.Hospitals = _context.Hospitals
                .Select(h => new { h.HospitalID, h.HospitalName })
                .ToList();

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
