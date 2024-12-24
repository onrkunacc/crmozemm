using HospitalInventoryManagement.BL.Service;
using HospitalInventoryManagement.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalInventoryManagement.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly CategoryService _categoryService;

        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // Kategori listesi
        public IActionResult Index()
        {
            var categories = _categoryService.GetAllCategories();
            return View(categories);
        }

        // Yeni kategori ekleme formu
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Yeni kategori ekleme işlemi
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryService.AddCategory(category);
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // Kategori silme işlemi
        [HttpPost]
        public IActionResult Delete(int id)
        {
            _categoryService.DeleteCategory(id);
            return RedirectToAction("Index");
        }

        // Kategori düzenleme formu
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _categoryService.GetAllCategories().FirstOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // Kategori güncelleme işlemi
        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryService.UpdateCategory(category);
                return RedirectToAction("Index");
            }
            return View(category);
        }
    }
}
