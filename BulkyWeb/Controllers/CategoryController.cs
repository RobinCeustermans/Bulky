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
            if (obj.Name == obj.DisplayOrder.ToString()) //toLower won't work if any property is null
            {
                ModelState.AddModelError("name", "Name and DisplayOrder cannot be the same value");
                ModelState.AddModelError("displayorder", "DisplayOrder and Name cannot be the same value");
            }          
            if (ModelState.IsValid)
            {
                _db.Categories.Add(obj);
                _db.SaveChanges();
                return RedirectToAction("Index", "Category");
            }
            return View(obj);
        }
    }
}
