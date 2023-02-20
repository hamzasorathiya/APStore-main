using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Nop.Services.Payments;
using System.Net;
using Nop.Plugin.Payments.Razor.Models;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Localization;
using Nop.Core;
using Nop.Web.Models.Common;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Framework.Themes;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Services.Media;
using Nop.Services.Customers;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Messages;
using Nop.Core.Domain.Vendors;
using Nop.Services.Vendors;
using Nop.Services.Events;
using Nop.Services.Common;
using Nop.Core.Domain.Localization;

namespace Nop.Plugin.Payments.Razor.Controllers
{
    public class PaymentController : BasePaymentController
    {
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;

        private readonly RazorPaymentSettings _RazorPaymentSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IThemeContext _themeContext;
        private readonly ICacheManager _cacheManager;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly IPictureService _pictureService;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IVendorService _vendorService;
        private readonly OrderSettings _orderSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPdfService _pdfService;
        private readonly LocalizationSettings _localizationSettings;
        public PaymentController(ISettingService settingService,
            IPaymentService paymentService, IOrderService orderService,
            IOrderProcessingService orderProcessingService,
             ILocalizationService localizationService,
            RazorPaymentSettings RazorPaymentSettings,
            PaymentSettings paymentSettings,
            IStoreContext storeContext,
            IThemeContext themeContext,
            ICacheManager cacheManager,
            StoreInformationSettings storeInformationSettings,
            IPictureService pictureService,
            IWebHelper webHelper,
            ICustomerService customerService,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            IVendorService vendorService,
            OrderSettings orderSettings,
            IEventPublisher eventPublisher,
            IPdfService pdfService,
            LocalizationSettings localizationSettings)
        {
            this._settingService = settingService;
            this._paymentService = paymentService;
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._RazorPaymentSettings = RazorPaymentSettings;
            this._localizationService = localizationService;
            this._paymentSettings = paymentSettings;
            this._storeContext = storeContext;
            this._themeContext = themeContext;
            this._cacheManager = cacheManager;
            this._storeInformationSettings = storeInformationSettings;
            this._pictureService = pictureService;
            this._webHelper = webHelper;
            this._customerService = customerService;
            this._productService = productService;
            this._shoppingCartService = shoppingCartService;
            this._workContext = workContext;
            this._workflowMessageService = workflowMessageService;
            this._vendorService = vendorService;
            this._orderSettings = orderSettings;
            this._eventPublisher = eventPublisher;
            this._pdfService = pdfService;
            this._localizationSettings = localizationSettings;
        }

        public ActionResult Index()
        {
            return View();
        }


        //[HttpPost]
        public ActionResult CreateOrder(Models.PaymentInitiateModel _requestData)
        {
            // Generate random receipt number for order
            Random randomObj = new Random();
            string transactionId = randomObj.Next(10000000, 100000000).ToString();

            Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient("rzp_test_xeaKLKmEK2ONVq", "iXOxfIenZM0VNlOm36Z9mD74");
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", Convert.ToInt32(_requestData.amount) * 100);  // Amount will in paise
            options.Add("receipt", transactionId);
            options.Add("currency", "INR");
            options.Add("payment_capture", "0"); // 1 - automatic  , 0 - manual
                                                 //options.Add("notes", "-- You can put any notes here --");
            Razorpay.Api.Order orderResponse = client.Order.Create(options);
            string orderId = orderResponse["id"].ToString();

            // Create order model for return on view
            OrderModel orderModel = new OrderModel
            {
                orderId = orderResponse.Attributes["id"],
                razorpayKey = "rzp_test_xeaKLKmEK2ONVq",
                amount = Convert.ToInt32(_requestData.amount) * 100,
                currency = "INR",
                name = _requestData.name,
                email = _requestData.email,
                contactNumber = _requestData.contactNumber,
                address = _requestData.address,
                //PreviewImageUrl = _requestData.PreviewImageUrl,
                description = "Testing description"
            };

            // Return on PaymentPage with Order data
            return View("PaymentPage", orderModel);
        }

        public class OrderModel
        {
            public string orderId { get; set; }
            public string razorpayKey { get; set; }
            public decimal amount { get; set; }
            public string currency { get; set; }
            public string name { get; set; }
            public string email { get; set; }
            public string contactNumber { get; set; }
            public string address { get; set; }
            public string description { get; set; }
            public string PreviewImageUrl { get; set; }
        }


        //[HttpPost]
        public ActionResult Return()
        {
            // Payment data comes in url so we have to get it from url

            // This id is razorpay unique payment id which can be use to get the payment details from razorpay server
            string paymentId = Request.Params["rzp_paymentid"];

            // This is orderId
            string orderId = Request.Params["rzp_orderid"];

            Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient("rzp_test_xeaKLKmEK2ONVq", "iXOxfIenZM0VNlOm36Z9mD74");

            Razorpay.Api.Payment payment = client.Payment.Fetch(paymentId);

            // This code is for capture the payment 
            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", payment.Attributes["amount"]);
            Razorpay.Api.Payment paymentCaptured = payment.Capture(options);
            string amt = paymentCaptured.Attributes["amount"];

            //// Check payment made successfully

            if (paymentCaptured.Attributes["status"] == "captured")
            {
                // Create these action method
                return RedirectToAction("Success");
            }
            else
            {
                return RedirectToAction("Failed");
            }
        }

        public ActionResult Complete(FormCollection form)
        {
            var processor = _paymentService.LoadPaymentMethodBySystemName("Payments.Razor") as RazorPaymentProcessor;
            if (processor == null ||
                !processor.IsPaymentMethodActive(_paymentSettings) || !processor.PluginDescriptor.Installed)
                throw new NopException("Razor module cannot be loaded");

            var myUtility = new RazorHelper();
            string orderId;
            //Assign following values to send it to verifychecksum function.
            if (String.IsNullOrWhiteSpace(_RazorPaymentSettings.Key))
                throw new NopException("Razor key is not set");

            // Payment data comes in url so we have to get it from url

            // This id is razorpay unique payment id which can be use to get the payment details from razorpay server
            string paymentId = Request.Params["rzp_paymentid"];

            // This is orderId
            orderId = Request.Params["rzp_orderid"];
            
            Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient(_RazorPaymentSettings.Key, _RazorPaymentSettings.Secret); //"rzp_test_xeaKLKmEK2ONVq", "iXOxfIenZM0VNlOm36Z9mD74"
            Razorpay.Api.Payment payment = client.Payment.Fetch(paymentId);

          
            // This code is for capture the payment 
            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", payment.Attributes["amount"]);
            Razorpay.Api.Payment paymentCaptured = payment.Fetch(paymentId);
            paymentCaptured = payment.Capture(options);

            string amt = paymentCaptured.Attributes["amount"];
            string oId = payment.Attributes["description"];
            if (paymentCaptured.Attributes["status"] == "captured" || paymentCaptured.Attributes["status"] == "authorized")
            {
                var order = _orderService.GetOrderById(Convert.ToInt32(oId));
                if (_orderProcessingService.CanMarkOrderAsPaid(order))
                {
                    _orderProcessingService.MarkOrderAsPaid(order);
                }
                Customer customer = _customerService.GetCustomerById(order.CustomerId);
                var cartItem = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
                foreach (var sc in cartItem)
                {
                    var productFromOrder = order.OrderItems.Where(x => x.ProductId == sc.ProductId).FirstOrDefault();
                    if (productFromOrder != null && productFromOrder.ProductId > 0)
                    {
                        _productService.AdjustInventory(sc.Product, -sc.Quantity, sc.AttributesXml,
                                    string.Format(_localizationService.GetResource("Admin.StockQuantityHistory.Messages.PlaceOrder"), order.Id));
                    }
                }
                cartItem.ToList().ForEach(sci => _shoppingCartService.DeleteShoppingCartItem(sci, false));
                SendNotificationsAndSaveNotes(order);
                _eventPublisher.Publish(new OrderPlacedEvent(order));
                if (paymentCaptured.Attributes["status"] == "authorized")
                {
                    payment.Capture(options);
                }
                //Thank you for shopping with us. Your credit card has been charged and your transaction is successful
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
            }
            else
            {
                /*
                   Here you need to put in the routines for a failed
                   transaction such as sending an email to customer
                   setting database status etc etc
               */
                // Need to do something here
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }


    

        public ActionResult Success()
        {
            return View();
        }

        public ActionResult Failed()
        {
            return View();
        }

        public ActionResult PaymentInfo()
        {
            var paymentInfoModel = new PaymentInfoModel();
            return PartialView("~/Plugins/Payments.Razor/Views/Payment/PaymentPage.cshtml", paymentInfoModel);
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.Key = _RazorPaymentSettings.Key;
            model.Secret = _RazorPaymentSettings.Secret;
            model.AdditionalFee = _RazorPaymentSettings.AdditionalFee;
            return View("~/Plugins/Payments.Razor/Views/Payment/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();
            //save settings
            _RazorPaymentSettings.Key = model.Key;
            _RazorPaymentSettings.Secret = model.Secret;
            _RazorPaymentSettings.AdditionalFee = model.AdditionalFee;
            _settingService.SaveSetting(_RazorPaymentSettings);
            SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            return Configure();
        }

        //[HttpPost]
        public ActionResult LoadRazorPay()
        {
            Models.PaymentInitiateModel remotePostHelper = new PaymentInitiateModel();
            remotePostHelper = (Models.PaymentInitiateModel)Session["remotePostHelper"];
            Session["remotePostHelper"] = null;
            // Generate random receipt number for order
            Random randomObj = new Random();
            string transactionId = randomObj.Next(10000000, 100000000).ToString();

            Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient(_RazorPaymentSettings.Key, _RazorPaymentSettings.Secret);
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Dictionary<string, object> options = new Dictionary<string, object>();
            options.Add("amount", remotePostHelper.amount * 100);  // Amount will in paise
            options.Add("receipt", transactionId);
            options.Add("currency", "INR");
            options.Add("payment_capture", "0"); // 1 - automatic  , 0 - manual
                                                 //options.Add("notes", "-- You can put any notes here --");
            Razorpay.Api.Order orderResponse = client.Order.Create(options);
            string orderId = orderResponse["id"].ToString();


            var model = new LogoModel
            {
                StoreName = _storeContext.CurrentStore.GetLocalized(x => x.Name)
            };

            var cacheKey = string.Format(ModelCacheEventConsumer.STORE_LOGO_PATH, _storeContext.CurrentStore.Id, _themeContext.WorkingThemeName, _webHelper.IsCurrentConnectionSecured());
            model.LogoPath = _cacheManager.Get(cacheKey, () =>
            {
                var logo = "";
                var logoPictureId = _storeInformationSettings.LogoPictureId;
                if (logoPictureId > 0)
                {
                    logo = _pictureService.GetPictureUrl(logoPictureId, showDefaultPicture: false);
                }
                if (String.IsNullOrEmpty(logo))
                {
                    //use default logo
                    logo = string.Format("{0}Themes/{1}/Content/images/logo.png", _webHelper.GetStoreLocation(), _themeContext.WorkingThemeName);
                }
                return logo;
            });

            string LogoPath = model.LogoPath;

            // Create order model for return on view
            OrderModel orderModel = new OrderModel
            {
                orderId = orderResponse.Attributes["id"],
                razorpayKey = _RazorPaymentSettings.Key,
                amount = remotePostHelper.amount * 100,
                currency = "INR",
                name = remotePostHelper.name,
                email = remotePostHelper.email,
                contactNumber = remotePostHelper.contactNumber,
                address = remotePostHelper.address,
                description = remotePostHelper.Order_Id,
                PreviewImageUrl = LogoPath
            };
            return View("~/Plugins/Payments.Razor/Views/Payment/LoadRazorPayment.cshtml", orderModel);
        }

        public virtual void SendNotificationsAndSaveNotes(Order order)
        {
            //notes, messages
            if (_workContext.OriginalCustomerIfImpersonated != null)
                //this order is placed by a store administrator impersonating a customer
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("Order placed by a store owner ('{0}'. ID = {1}) impersonating the customer.",
                        _workContext.OriginalCustomerIfImpersonated.Email, _workContext.OriginalCustomerIfImpersonated.Id),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
            else
                order.OrderNotes.Add(new OrderNote
                {
                    Note = "Order placed",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
            _orderService.UpdateOrder(order);

            //send email notifications
            var orderPlacedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderPlacedStoreOwnerNotification(order, _localizationSettings.DefaultAdminLanguageId);
            if (orderPlacedStoreOwnerNotificationQueuedEmailId > 0)
            {
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("\"Order placed\" email (to store owner) has been queued. Queued email identifier: {0}.", orderPlacedStoreOwnerNotificationQueuedEmailId),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
            }

            var orderPlacedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
                _pdfService.PrintOrderToPdf(order) : null;
            var orderPlacedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
                "order.pdf" : null;
            var orderPlacedCustomerNotificationQueuedEmailId = _workflowMessageService
                .SendOrderPlacedCustomerNotification(order, order.CustomerLanguageId, orderPlacedAttachmentFilePath, orderPlacedAttachmentFileName);
            if (orderPlacedCustomerNotificationQueuedEmailId > 0)
            {
                order.OrderNotes.Add(new OrderNote
                {
                    Note = string.Format("\"Order placed\" email (to customer) has been queued. Queued email identifier: {0}.", orderPlacedCustomerNotificationQueuedEmailId),
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
            }

            var vendors = GetVendorsInOrder(order);
            foreach (var vendor in vendors)
            {
                var orderPlacedVendorNotificationQueuedEmailId = _workflowMessageService.SendOrderPlacedVendorNotification(order, vendor, _localizationSettings.DefaultAdminLanguageId);
                if (orderPlacedVendorNotificationQueuedEmailId > 0)
                {
                    order.OrderNotes.Add(new OrderNote
                    {
                        Note = string.Format("\"Order placed\" email (to vendor) has been queued. Queued email identifier: {0}.", orderPlacedVendorNotificationQueuedEmailId),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    _orderService.UpdateOrder(order);
                }
            }
        }
        public virtual IList<Vendor> GetVendorsInOrder(Order order)
        {
            var vendors = new List<Vendor>();
            foreach (var orderItem in order.OrderItems)
            {
                var vendorId = orderItem.Product.VendorId;
                //find existing
                var vendor = vendors.FirstOrDefault(v => v.Id == vendorId);
                if (vendor == null)
                {
                    //not found. load by Id
                    vendor = _vendorService.GetVendorById(vendorId);
                    if (vendor != null && !vendor.Deleted && vendor.Active)
                    {
                        vendors.Add(vendor);
                    }
                }
            }

            return vendors;
        }
    }
}