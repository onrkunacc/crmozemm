using HospitalInventoryManagement.BL.Interfaces;
using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.DTOs;
using HospitalInventoryManagement.Data.Models;
using HospitalInventoryManagement.Web.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using X.PagedList; 

namespace HospitalInventoryManagement.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStockService _stockService;
        private readonly IStatisticsService _statisticsService;
        private readonly ApplicationDbContext _context;

        public HomeController(UserManager<ApplicationUser> userManager, IStockService stockService, IStatisticsService statisticsService, ApplicationDbContext context)
        {
            _userManager = userManager;
            _stockService = stockService;
            _statisticsService = statisticsService;
            _context = context;
        }

        // Ana Sayfa
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Admin kontrolü yaparak admin kullanýcýlarý AdminIndex'e yönlendir
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("AdminIndex");
            }

            // Kullanýcýnýn HospitalID'sine göre stoðu azalan ürünleri getir
            var lowStockItems = _stockService.GetStocksByHospital(user.HospitalID)
                                             .Where(s => s.Quantity < 10)
                                             .OrderBy(s => s.Quantity)
                                             .Take(10)
                                             .ToList();

            // Stok verileri
            var stockData = _stockService.GetStocksByHospital(user.HospitalID)
                .Select(s => new StockChartData
                {
                    ProductName = s.Product.ProductName,
                    Quantity = s.Quantity
                }).ToList();

            var viewModel = new HomeViewModel
            {
                LowStockItems = lowStockItems,
                StockData = stockData
            };

            return View(viewModel);
        }

        // Admin Ana Sayfa
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AdminIndex(int? hospitalId, string sortOrder, int pageNumber = 1, int pageSize = 50)
        {
            var viewModel = new AdminStockListViewModel
            {
                Hospitals = new SelectList(_context.Hospitals, "HospitalID", "HospitalName"),
                SelectedHospitalID = hospitalId,
                SortOrder = sortOrder,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var stocks = _stockService.GetAllStocks();

            if (hospitalId.HasValue)
            {
                stocks = stocks.Where(s => s.HospitalID == hospitalId.Value).ToList();
            }

            switch (sortOrder)
            {
                case "NameAsc":
                    stocks = stocks.OrderBy(s => s.Product.ProductName).ToList();
                    break;
                case "NameDesc":
                    stocks = stocks.OrderByDescending(s => s.Product.ProductName).ToList();
                    break;
                case "DateAsc":
                    stocks = stocks.OrderBy(s => s.LastUpdated).ToList();
                    break;
                case "DateDesc":
                    stocks = stocks.OrderByDescending(s => s.LastUpdated).ToList();
                    break;
            }

            viewModel.TotalCount = stocks.Count;

            stocks = stocks.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            viewModel.Stocks = stocks.Select(s => new StockDTO
            {
                StockID = s.StockID,
                ProductName = s.Product.ProductName,
                Quantity = s.Quantity,
                ExpiryDate = s.ExpiryDate,
                HospitalName = s.Hospital.HospitalName,
                LastUpdated = s.LastUpdated
            }).ToList();

            return View(viewModel);
        }

        public IActionResult Error(string message)
        {
            ViewBag.ErrorMessage = message; 
            return View();
        }
    }
}
