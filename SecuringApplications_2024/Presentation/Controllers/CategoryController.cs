using Common.Models;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    public class CategoryController : Controller
    {
        private CategoriesRepository _categoriesRepository;

        public CategoryController(CategoriesRepository categoriesRepository)
        {
            _categoriesRepository = categoriesRepository;
        }

        public IActionResult Index()
        {
            List<Category> categories = _categoriesRepository.GetCategories().ToList();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            ModelState.Remove("Id");

            if(ModelState.IsValid)
            {
                this._categoriesRepository.AddCategory(category);
                TempData["message"] = "Category uploaded successfully!";
                return RedirectToAction("Index", "Category");
            }

            TempData["error"] = "Failed to upload category due to missing data!";
            return View(category);
        }
    }
}
