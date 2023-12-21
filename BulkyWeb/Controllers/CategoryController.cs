using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = _db.Categories.ToList();
            return View(objCategoryList);
        }

        public IActionResult Create()
        { 
            return View();
            //you could pas a new class, but not needed as long as the class is defined in the view, could be useful to define a few properties beforehand
            //return View(new Category());
        }

        [HttpPost] //aanotation has to be present
        public IActionResult Create(Category obj)
        { 
            _db.Categories.Add(obj);
            _db.SaveChanges();
            return RedirectToAction("Index", "Category");
        }
    }
}
