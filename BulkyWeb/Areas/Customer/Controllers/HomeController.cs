using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                //user is logged in, then get session and show number after cat icon on top
                HttpContext.Session.SetInt32(SD.SessionCart, 
                    _unitOfWork.ShoppingCartRepository.GetAll(x => x.ApplicationUserId == claim.Value).Count());
            }

            IEnumerable<Product> products = _unitOfWork.ProductRepository.GetAll(includeProperties: "Category");
            return View(products);
        }

        public IActionResult Details(int id)
        {
            ShoppingCart shoppingCart = new ShoppingCart()
            {
                Product = _unitOfWork.ProductRepository.Get(x => x.Id == id, includeProperties: "Category"),
                Count = 1,
                ProductId = id
            };

            return View(shoppingCart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCartRepository.Get(x => x.ApplicationUserId == userId && x.ProductId == shoppingCart.ProductId);

            if (cartFromDb != null) 
            {
                cartFromDb.Count += shoppingCart.Count; //update the count
                _unitOfWork.ShoppingCartRepository.Update(cartFromDb); //EF will track this thus still an update, so you have turn it off
                TempData["success"] = "Cart updated successfully";
                _unitOfWork.Save();
            }
            else
            {
                _unitOfWork.ShoppingCartRepository.Add(shoppingCart);
                TempData["success"] = "Cart updated successfully";
                _unitOfWork.Save();
                //if you would want to set an object, extra configuration is required
                //this way you can display the amount of items in a cart per user by using session, configuration in progra.cs is required, make sure to save before using
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCartRepository.GetAll(x => x.ApplicationUserId == userId).Count());
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
