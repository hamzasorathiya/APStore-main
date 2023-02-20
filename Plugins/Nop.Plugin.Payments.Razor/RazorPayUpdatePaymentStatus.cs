using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Services.Tasks;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Configuration;
using Nop.Services.Payments;
using Nop.Services.Orders;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Services.Events;
using Nop.Core.Domain.Localization;
using Nop.Services.Localization;
using Nop.Plugin.Payments.Razor.Controllers;

namespace Nop.Plugin.Payments.Razor
{
    public partial class RazorPayUpdatePaymentStatus : ITask
    {
        private readonly PaymentController _payment;
        private readonly RazorPaymentSettings _RazorPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IEventPublisher _eventPublisher;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IProductService _productService;
        public RazorPayUpdatePaymentStatus(PaymentController payment, RazorPaymentSettings RazorPaymentSettings,
            ISettingService settingService,
            IPaymentService paymentService, IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IStoreContext storeContext,
            ICustomerService customerService,
            IWorkContext workContext,
            OrderSettings orderSettings,
            IShoppingCartService shoppingCartService,
            ILocalizationService localizationService,
             IEventPublisher eventPublisher,
             LocalizationSettings localizationSettings,
             IProductService productService)
        {
            _payment = payment;
            _RazorPaymentSettings = RazorPaymentSettings;
            _settingService = settingService;
            _paymentService = paymentService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _storeContext = storeContext;
            _customerService = customerService;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _shoppingCartService = shoppingCartService;
            _localizationService = localizationService;
            _eventPublisher = eventPublisher;
            _localizationSettings = localizationSettings;
            _productService = productService;
        }
        public virtual void Execute()
        {
            #region
            Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient(_RazorPaymentSettings.Key, _RazorPaymentSettings.Secret);
            //Razorpay.Api.RazorpayClient client = new Razorpay.Api.RazorpayClient("rzp_live_o0Tx53gdhqhBj9", "fTkOPktT4sOmmbtLiq9iVo09");
            Dictionary<string, object> options = new Dictionary<string, object>();
            var t = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            var curtime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            var time = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds - 300;
            options.Add("from", time.ToString());
            options.Add("to", curtime.ToString());
            var data = client.Payment.All(options);
            foreach(var item in data)
            {
                var created_at = item.Attributes["created_at"];
                var d = DateTime.Now.Date;
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds(created_at.Value).ToLocalTime();
                if (d == dateTime.Date)
                {
                    if (item.Attributes["status"] == "authorized")
                    {
                        Dictionary<string, object> options1 = new Dictionary<string, object>();
                        options1.Add("amount", item.Attributes["amount"]);
                        var paymentCaptured = item.Capture(options1);
                        string oId = item.Attributes["description"];

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

                            _payment.SendNotificationsAndSaveNotes(order);
                            _eventPublisher.Publish(new OrderPlacedEvent(order));
                        }
                        }
                }
            }
            #endregion
        }


    }
}
