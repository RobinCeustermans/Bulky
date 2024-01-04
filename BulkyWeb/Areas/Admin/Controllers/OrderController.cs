using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
			_unitOfWork = unitOfWork;
        }

        public IActionResult Index()
		{
			return View();
		}

        public IActionResult Details(int orderId)
        {
            OrderVM = new OrderVM
            {
                OrderHeader = _unitOfWork.OrderHeaderRepository.Get(x => x.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetailRepository.GetAll(x => x.OrderHeaderId == orderId, includeProperties:"Product")
            };

            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles =SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeaderRepository.Get(x => x.Id == OrderVM.OrderHeader.Id); //the id comes from hidden field in view

            orderHeaderFromDb.Name = OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;

            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeaderRepository.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Order details updated successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProccessing()
        {
            _unitOfWork.OrderHeaderRepository.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["Success"] = "Order details updated successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }     
        
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unitOfWork.OrderHeaderRepository.Get(x => x.Id == OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _unitOfWork.OrderHeaderRepository.Update(orderHeader);
            _unitOfWork.Save();
            TempData["Success"] = "Order shipped successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.OrderHeaderRepository.Get(x => x.Id == OrderVM.OrderHeader.Id);

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var option = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service =  new RefundService();
                Refund refund = service.Create(option);

                _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }

            _unitOfWork.Save();
            TempData["Success"] = "Order cancelled successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            OrderVM.OrderHeader = _unitOfWork.OrderHeaderRepository.Get(x => x.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetail = _unitOfWork.OrderDetailRepository.GetAll(x => x.OrderHeaderId == OrderVM.OrderHeader.Id, includeProperties: "Product");

            //stripe logic
            var domain = "http://localhost:5187/";
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId{OrderVM.OrderHeader.Id}",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVM.OrderDetail)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), //€20.99 -> 2099
                        Currency = "EUR",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new Stripe.Checkout.SessionService();
            Session session = service.Create(options);

            _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId); //for now paymentintentid remains null
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaderRepository.Get(x => x.Id == orderHeaderId, includeProperties: "ApplicationUser");

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //this then is a order by company
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid") //check documentation for naming
                {
                    _unitOfWork.OrderHeaderRepository.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId); //now session has PaymentIntentId
                    _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            return View(orderHeaderId);
        }


        #region Api Calls
        [HttpGet]
		public IActionResult GetAll(string status)
		{
            IEnumerable<OrderHeader> objOrderHeaderList;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderHeaderList = _unitOfWork.OrderHeaderRepository.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objOrderHeaderList = _unitOfWork.OrderHeaderRepository.GetAll(x => x.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    objOrderHeaderList = objOrderHeaderList.Where(x => x.OrderStatus == SD.StatusPending);
                    break;

                case "inprocess":
                    objOrderHeaderList = objOrderHeaderList.Where(x => x.OrderStatus == SD.StatusInProcess);
                    break;

                case "completed":
                    objOrderHeaderList = objOrderHeaderList.Where(x => x.OrderStatus == SD.StatusShipped);
                    break;     
                
                case "paymentPending":
                    objOrderHeaderList = objOrderHeaderList.Where(x => x.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;

                case "approved":
                    objOrderHeaderList = objOrderHeaderList.Where(x => x.OrderStatus == SD.StatusApproved);
                    break;

                default:
                    break;
            }

            return Json(new { data = objOrderHeaderList });
		}

		#endregion
	}
}
