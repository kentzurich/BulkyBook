using BulkyBook.DataAccess.Repository.UnitOfWork;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Options;
using Stripe.Checkout;
using System.Security.Claims;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace BulkyBook.Areas.Customer.Controllers
{
	[Area("Customer")]
    [Authorize]
	public class CartController : Controller
    {
		private readonly IUnitOfWork _unitOfWork;
		private IEmailSender _emailSender;
		[BindProperty]
        public ShoppingCartViewModel ShoppingCartVM { get; set; }
        public double OrderTotal { get; set; }

        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender)
		{
			_unitOfWork = unitOfWork;
			_emailSender = emailSender;
		}

		public IActionResult Index()
        {
			//getting the name identity
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM = new ShoppingCartViewModel()
			{
				ListCart = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value, 
				includeProperties: "Product"),
				OrderHeader = new()
			};

			IEnumerable<ProductImageModel> productImages = _unitOfWork.ProductImages.GetAll();
			
			foreach(var cart in ShoppingCartVM.ListCart)
			{
				cart.Product.ProductImages = productImages.Where(x => x.ProductId == cart.Product.Id).ToList();
				cart.Price = GetPriceBasedQty(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}	

			return View(ShoppingCartVM);
        }

		public IActionResult Summary()
		{
			//getting the name identity
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM = new ShoppingCartViewModel()
			{
				ListCart = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value,
				includeProperties: "Product"),
				OrderHeader = new()
			};

			ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(
				x => x.Id == claim.Value);

			ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
			ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
			ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

			foreach (var cart in ShoppingCartVM.ListCart)
			{
				cart.Price = GetPriceBasedQty(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			return View(ShoppingCartVM);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
        [ActionName(nameof(Summary))]
        public IActionResult SummaryPOST()
		{
			//getting the name identity
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == claim.Value,
				includeProperties: "Product");
			ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
			ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;

			foreach (var cart in ShoppingCartVM.ListCart)
			{
				cart.Price = GetPriceBasedQty(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			ApplicationUserModel applicationUser = _unitOfWork.ApplicationUser.GetFirstOrDefault(x => x.Id == claim.Value);
			
			ShoppingCartVM.OrderHeader.PaymentStatus = 
				applicationUser.CompanyId.GetValueOrDefault() == 0 ? 
					StaticDetails.PAYMENTSTATUS_PENDING : StaticDetails.PAYMENTSTATUS_DELAYED;
			ShoppingCartVM.OrderHeader.OrderStatus = 
				applicationUser.CompanyId.GetValueOrDefault() == 0 ? 
					StaticDetails.ORDERSTATUS_PENDING : StaticDetails.ORDERSTATUS_APPROVED;

            if (!applicationUser.CompanyId.GetValueOrDefault().Equals(0))
                ShoppingCartVM.OrderHeader.PaymentDueDate = DateTime.Now.AddDays(30);

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
			_unitOfWork.Save();

			foreach (var cart in ShoppingCartVM.ListCart)
			{
				OrderDetailsModel orderDetail = new()
				{
					ProductId = cart.ProductId,
					OrderId = ShoppingCartVM.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count
				};
				_unitOfWork.OrderDetails.Add(orderDetail);
				_unitOfWork.Save();
			}

			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
                // Stripe Settings
                //var domainUrl = "https://localhost:44374/";
                var domainUrl = $"{Request.Scheme}://{Request.Host.Value}/";
                var options = new SessionCreateOptions
				{
					//PaymentMethodTypes = new List<string>
					//{
					//	"card",
					//},
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
					SuccessUrl = $"{domainUrl}Customer/Cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
					CancelUrl = $"{domainUrl}Customer/Cart/Index",
				};

				foreach (var item in ShoppingCartVM.ListCart)
				{
					var sessionLineItem = new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							UnitAmount = (long)(item.Price * 100),
							Currency = "usd",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = item.Product.Title,
							},
						},
						Quantity = item.Count,
					};
					options.LineItems.Add(sessionLineItem);
				}

				var service = new SessionService();
				Session session = service.Create(options);
				_unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
				_unitOfWork.Save();
				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);//redirecting new url which is provided by stripe
			}
			else
				return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
		}

		public IActionResult OrderConfirmation(int id)
		{
			OrderHeaderModel orderHeader = _unitOfWork.OrderHeader.GetFirstOrDefault(x => x.Id == id, includeProperties:"ApplicationUser");
			
			if(!orderHeader.PaymentStatus.Equals(StaticDetails.PAYMENTSTATUS_DELAYED))
			{
				//this is an order of a customer
				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);
				//check stripe status
				if (session.PaymentStatus.ToLower().Equals("paid"))
				{
					_unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeader.Id, session.Id, session.PaymentIntentId);
					_unitOfWork.OrderHeader.UpdateStatus(id, StaticDetails.ORDERSTATUS_APPROVED, StaticDetails.PAYMENTSTATUS_APPROVED);
					_unitOfWork.Save();
				}
			}

            _emailSender.SendEmailAsync(orderHeader.ApplicationUser.Email, "New Order - Bulky Book", $"<p>New order created. Order Id No. {orderHeader.Id}</p>");
			List<ShoppingCartModel> shoppingCart = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
			_unitOfWork.ShoppingCart.RemoveRange(shoppingCart);
			_unitOfWork.Save();
            HttpContext.Session.Clear();
            return View(id);
		}

		public IActionResult Plus(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
			_unitOfWork.ShoppingCart.IncrementCount(cart, 1);
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Minus(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);

			if(cart.Count <= 1)
			{
                _unitOfWork.ShoppingCart.Remove(cart);
                _unitOfWork.Save();
                var countItems = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == cart.ApplicationUserId).Count();
                HttpContext.Session.SetInt32(StaticDetails.SessionCart, countItems);
            }
			else
			{
                _unitOfWork.ShoppingCart.DecrementCount(cart, 1);
                _unitOfWork.Save();
            }
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Remove(int cartId)
		{
			var cart = _unitOfWork.ShoppingCart.GetFirstOrDefault(x => x.Id == cartId);
			_unitOfWork.ShoppingCart.Remove(cart);
			_unitOfWork.Save();
			var countItems = _unitOfWork.ShoppingCart.GetAll(x => x.ApplicationUserId == cart.ApplicationUserId).Count();
            HttpContext.Session.SetInt32(StaticDetails.SessionCart, countItems);

            return RedirectToAction(nameof(Index));
		}

		private double GetPriceBasedQty(ShoppingCartModel shoppingCart)
		{
			if (shoppingCart.Count <= 50)
				return shoppingCart.Product.Price;
			else
			{
				if (shoppingCart.Count <= 100)
					return shoppingCart.Product.Price_50;
				else
					return shoppingCart.Product.Price_100;
			}
		}
    }
}
