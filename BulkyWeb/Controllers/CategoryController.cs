using Bulky.Models;
using BulkyWeb.DataAcces.Data;
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

        [HttpPost] //annotation has to be present
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
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index", "Category");
            }
            return View(obj);
        }

        public IActionResult Edit(int? id)
        {
            if (id==null || id.Value == 0)
            {
                return NotFound();
            }
            Category? category = _db.Categories.Find(id); //only works on pk
            //Category category1 = _db.Categories.FirstOrDefault(x => x.Id == id); //could search on name
            //Category category2 = _db.Categories.Where(x => x.Id == id).FirstOrDefault();
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Update(obj);
                _db.SaveChanges();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index", "Category");
            }
            return View(obj);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id.Value == 0)
            {
                return NotFound();
            }
            Category? category = _db.Categories.Find(id); 

            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Category? category = _db.Categories.Find(id);
            if (category == null) 
            {
                return NotFound();
            }

            _db.Categories.Remove(category);
            _db.SaveChanges();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index", "Category");
        }
    }
}
