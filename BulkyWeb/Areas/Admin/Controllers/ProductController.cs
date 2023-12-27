using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.ProductRepository.GetAll().ToList();
            return View(objProductList);
        }

        public IActionResult Create()
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.CategoryRepository.GetAll().ToList().Select(x =>
                new SelectListItem { Value = x.Id.ToString(), Text = x.Name });

            //ViewBag.CategoryList = CategoryList;
            //ViewData["CategoryList"] = CategoryList;

            ProductVM productVM = new ProductVM() 
            {
                CategoryList = CategoryList,
                Product = new Product() { }
            };
            return View(productVM);
        }

        [HttpPost]
        public IActionResult Create(ProductVM obj)
        { 
            if (ModelState.IsValid)
            {
                _unitOfWork.ProductRepository.Add(obj.Product);
                _unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                IEnumerable<SelectListItem> CategoryList = _unitOfWork.CategoryRepository.GetAll().ToList().Select(x =>
                    new SelectListItem { Value = x.Id.ToString(), Text = x.Name });

               obj.CategoryList = CategoryList;

                return View(obj);
            }    
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id.Value == 0)
            {
                return NotFound();
            }

            Product? product = _unitOfWork.ProductRepository.Get(x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.ProductRepository.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Product updated successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id.Value == 0)
            {
                return NotFound();
            }

            Product? product = _unitOfWork.ProductRepository.Get(x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
             Product? product = _unitOfWork.ProductRepository.Get(x => x.Id==id);

            _unitOfWork.ProductRepository.Remove(product);
            _unitOfWork.Save();
            TempData["success"] = "Product deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
