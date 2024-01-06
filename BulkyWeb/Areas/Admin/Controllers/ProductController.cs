using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
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
                productVM.Product = _unitOfWork.ProductRepository.Get(x => x.Id == id, includeProperties: "ProductImages");
                return View(productVM);
            }
        }

        #region ProductSingleImage
        //[HttpPost]
        //public IActionResult Upsert(ProductVM obj, IFormFile? file)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        string wwwrootPath = _webHostEnvironment.WebRootPath;
        //        if (file != null)
        //        {
        //            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        //            string productPath = Path.Combine(wwwrootPath, @"images\product");

        //            //if (!string.IsNullOrEmpty( obj.Product.ImageUrl))
        //            //{
        //            //    //delete old image
        //            //    var oldPath = Path.Combine(wwwrootPath, obj.Product.ImageUrl.TrimStart('\\'));

        //            //    if (System.IO.File.Exists(oldPath))
        //            //    {
        //            //        System.IO.File.Delete(oldPath);
        //            //    }
        //            //}

        //            //using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
        //            //{
        //            //    file.CopyTo(fileStream);
        //            //}

        //            //obj.Product.ImageUrl = @"\images\product\" + fileName;
        //        }

        //        if (obj.Product.Id == 0)
        //        {
        //            _unitOfWork.ProductRepository.Add(obj.Product);
        //        }
        //        else
        //        {
        //            _unitOfWork.ProductRepository.Update(obj.Product);
        //        }

        //        _unitOfWork.Save();
        //        TempData["success"] = "Product created successfully";
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        IEnumerable<SelectListItem> CategoryList = _unitOfWork.CategoryRepository.GetAll().ToList().Select(x =>
        //            new SelectListItem { Value = x.Id.ToString(), Text = x.Name });

        //        obj.CategoryList = CategoryList;

        //        return View(obj);
        //    }
        //}
        #endregion

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM,List< IFormFile> files)
        { 
            if (ModelState.IsValid)
            {
                //first create a product, with its id, a folder will be created
                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.ProductRepository.Add(productVM.Product);
                }
                else
                {
                    _unitOfWork.ProductRepository.Update(productVM.Product);
                }

                _unitOfWork.Save();

                string wwwrootPath = _webHostEnvironment.WebRootPath;
                if (files != null)
                {
                    foreach (IFormFile file in files) 
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productParh = @"images\products/product-" + productVM.Product.Id;
                        string finalPath = Path.Combine(wwwrootPath, productParh);

                        if (!Directory.Exists(finalPath))
                        {
                            Directory.CreateDirectory(finalPath);
                        }

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        ProductImage image = new ProductImage()
                        {
                            ImageUrl = @"\" + productParh + @"\" + fileName,
                            ProductId = productVM.Product.Id
                        };

                        if (productVM.Product.ProductImages == null)
                        {
                            productVM.Product.ProductImages = new List<ProductImage>();
                        }

                        productVM.Product.ProductImages.Add(image);

                        //_unitOfWork.ProductImageRepository.Add(image); //this can be done outside the loop by modifing product repository ->  objDb.ProductImages = product.ProductImages;
                    }

                    _unitOfWork.ProductRepository.Update(productVM.Product);
                    _unitOfWork.Save();
                }

                TempData["success"] = "Product upserted successfully";
                return RedirectToAction("Index");
            }
            else
            {
                IEnumerable<SelectListItem> CategoryList = _unitOfWork.CategoryRepository.GetAll().ToList().Select(x =>
                    new SelectListItem { Value = x.Id.ToString(), Text = x.Name });

               productVM.CategoryList = CategoryList;

                return View(productVM);
            }    
        }

        #region Deleting without API

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
        #endregion

        public IActionResult DeleteImage(int imageId)
        {
            var imageToBeDeleted = _unitOfWork.ProductImageRepository.Get(x => x.Id == imageId);
            int productId = imageToBeDeleted.ProductId;
            if (imageToBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, imageToBeDeleted.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                _unitOfWork.ProductImageRepository.Remove(imageToBeDeleted);
                _unitOfWork.Save();

                TempData["success"] = "Deleted successfully";
            }

            return RedirectToAction(nameof(Upsert), new { id = productId });
        }

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

            string productParh = @"images\products/product-" + id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productParh);

            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath); 
                }

                Directory.Delete(finalPath);
            }

            ////for single file
            //var oldPath  = Path.Combine(_webHostEnvironment.WebRootPath, obj.ImageUrl.TrimStart('\\'));

            //if (System.IO.File.Exists(oldPath))
            //{
            //    System.IO.File.Delete(oldPath);
            //}

            _unitOfWork.ProductRepository.Remove(obj);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successfull" });
        }
        #endregion
    }
}
