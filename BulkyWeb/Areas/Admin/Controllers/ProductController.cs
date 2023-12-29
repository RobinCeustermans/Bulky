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
        private readonly IWebHostEnvironment _webHostEnvironment; //to access wwwroot folder
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
                                                                                                   //must be correct casing
            List<Product> objProductList = _unitOfWork.ProductRepository.GetAll(includeProperties: "Category").ToList();
            return View(objProductList);
        }

        public IActionResult Upsert(int? id)
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.CategoryRepository.GetAll().ToList().Select(x =>
                new SelectListItem { Value = x.Id.ToString(), Text = x.Name });

            //ViewBag.CategoryList = CategoryList;
            //ViewData["CategoryList"] = CategoryList;

            ProductVM productVM = new ProductVM() 
            {
                CategoryList = CategoryList,
                Product = new Product()
               //Product = (id == null || id == 0) ? new Product() : _unitOfWork.ProductRepository.Get(x => x.Id == id)
            };
            if (id == null || id == 0)
            {
                //create
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product = _unitOfWork.ProductRepository.Get(x => x.Id == id);
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM obj, IFormFile? file)
        { 
            if (ModelState.IsValid)
            {
                string wwwrootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwrootPath, @"images\product");

                    if (!string.IsNullOrEmpty( obj.Product.ImageUrl))
                    {
                        //delete old image
                        var oldPath = Path.Combine(wwwrootPath, obj.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    obj.Product.ImageUrl = @"\images\product\" + fileName;
                }

                if (obj.Product.Id == 0)
                {
                    _unitOfWork.ProductRepository.Add(obj.Product);
                }
                else
                {
                    _unitOfWork.ProductRepository.Update(obj.Product);
                }

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

        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id.Value == 0)
        //    {
        //        return NotFound();
        //    }

        //    Product? product = _unitOfWork.ProductRepository.Get(x => x.Id == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(product);
        //}

        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePOST(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //     Product? product = _unitOfWork.ProductRepository.Get(x => x.Id==id);

        //    _unitOfWork.ProductRepository.Remove(product);
        //    _unitOfWork.Save();
        //    TempData["success"] = "Product deleted successfully";
        //    return RedirectToAction("Index");
        //}


        #region Api Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.ProductRepository.GetAll(includeProperties: "Category").ToList();
            return Json(new {data = objProductList});
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var obj = _unitOfWork.ProductRepository.Get(x => x.Id == id);
            if (obj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            var oldPath  = Path.Combine(_webHostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }

            _unitOfWork.ProductRepository.Remove(obj);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successfull" });
        }
        #endregion
    }
}
