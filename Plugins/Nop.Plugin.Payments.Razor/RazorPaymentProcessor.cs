using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Razor.Controllers;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Web.Framework;
using Nop.Plugin.Payments.Razor;
using System.Web;
using Nop.Admin.Models.Settings;
using Nop.Web.Factories;

namespace Nop.Plugin.Payments.Razor
{
    /// <summary>
    /// Razor payment processor
    /// </summary>
    public class RazorPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly RazorPaymentSettings _RazorPaymentSettings;
        private readonly ISettingService _settingService;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IWebHelper _webHelper;

        private readonly ILocalizationService _localizationService;
        

        #endregion

        #region Ctor

        public RazorPaymentProcessor(RazorPaymentSettings RazorPaymentSettings,
            ISettingService settingService, ICurrencyService currencyService,
              ILocalizationService localizationService,
            CurrencySettings currencySettings, IWebHelper webHelper)
        {
            this._localizationService = localizationService;
            this._RazorPaymentSettings = RazorPaymentSettings;
            this._settingService = settingService;
            this._currencyService = currencyService;
            this._currencySettings = currencySettings;
            this._webHelper = webHelper;
            

    }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var myUtility = new RazorHelper();
            var orderId = postProcessPaymentRequest.Order.Id;
            var remotePostHelper = new Models.PaymentInitiateModel();
            remotePostHelper.FormName = "RazorForm";
            //remotePostHelper.amount = postProcessPaymentRequest.Order.OrderTotal.ToString(new CultureInfo("en-US", false).NumberFormat);
            remotePostHelper.amount = postProcessPaymentRequest.Order.OrderTotal;
            //remotePostHelper.PreviewImageUrl = 
            remotePostHelper.Currency = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            remotePostHelper.Order_Id = orderId.ToString();
            remotePostHelper.txnid = orderId.ToString();
            remotePostHelper.surl = _webHelper.GetStoreLocation(false) + "Plugins/Payment/Return";
            remotePostHelper.furl = _webHelper.GetStoreLocation(false) + "Plugins/Payment/Return";
            remotePostHelper.hash = myUtility.getchecksum(
                postProcessPaymentRequest.Order.Id.ToString(), postProcessPaymentRequest.Order.OrderTotal.ToString(new CultureInfo("en-US", false).NumberFormat),
                "productinfo", postProcessPaymentRequest.Order.BillingAddress.FirstName.ToString(),
                postProcessPaymentRequest.Order.BillingAddress.Email.ToString(), _RazorPaymentSettings.Key);
            //Billing details
            remotePostHelper.firstname = postProcessPaymentRequest.Order.BillingAddress.FirstName.ToString();
            remotePostHelper.billing_cust_address = postProcessPaymentRequest.Order.BillingAddress.Address1;
            remotePostHelper.phone = postProcessPaymentRequest.Order.BillingAddress.PhoneNumber;
            remotePostHelper.email = postProcessPaymentRequest.Order.BillingAddress.Email.ToString();
            remotePostHelper.billing_cust_city = postProcessPaymentRequest.Order.BillingAddress.City;
            var billingStateProvince = postProcessPaymentRequest.Order.BillingAddress.StateProvince;
            if (billingStateProvince != null)
                remotePostHelper.billing_cust_state = billingStateProvince.Abbreviation;
            else
                remotePostHelper.billing_cust_state = "";
            remotePostHelper.billing_zip_code = postProcessPaymentRequest.Order.BillingAddress.ZipPostalCode;
            var billingCountry = postProcessPaymentRequest.Order.BillingAddress.Country;
            if (billingCountry != null)
                remotePostHelper.billing_cust_country = billingCountry.ThreeLetterIsoCode;
            else
                remotePostHelper.billing_cust_country = "";

            //Delivery details

            if (postProcessPaymentRequest.Order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                remotePostHelper.delivery_cust_name =  postProcessPaymentRequest.Order.ShippingAddress.FirstName;
                remotePostHelper.delivery_cust_address  = postProcessPaymentRequest.Order.ShippingAddress.Address1;
                remotePostHelper.delivery_cust_notes =  string.Empty;
                remotePostHelper.delivery_cust_tel =  postProcessPaymentRequest.Order.ShippingAddress.PhoneNumber;
                remotePostHelper.delivery_cust_city =  postProcessPaymentRequest.Order.ShippingAddress.City;
                var deliveryStateProvince = postProcessPaymentRequest.Order.ShippingAddress.StateProvince;
                if (deliveryStateProvince != null)
                    remotePostHelper.delivery_cust_state =  deliveryStateProvince.Abbreviation;
                else
                    remotePostHelper.delivery_cust_state = "";
                remotePostHelper.delivery_zip_code =  postProcessPaymentRequest.Order.ShippingAddress.ZipPostalCode ;
                var deliveryCountry = postProcessPaymentRequest.Order.ShippingAddress.Country;
                if (deliveryCountry != null)
                    remotePostHelper.delivery_cust_country =  deliveryCountry.ThreeLetterIsoCode;
                else
                    remotePostHelper.delivery_cust_country = "";
            }

            HttpContext.Current.Session["remotePostHelper"] = remotePostHelper;
            HttpContext.Current.Response.RedirectToRoute("Plugin.Payments.Razor.LoadRazorPay");
            //  remotePostHelper.Add("Merchant_Param", _RazorPaymentSettings.MerchantParam);
            //remotePostHelper.Post();
        }



        //Hide payment begins

        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        //hide payment ends

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _RazorPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //Razor is the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice

            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //let's ensure that at least 1 minute passed after order is placed
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1)
                return false;

            return true;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "Payment";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Razor.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "Payment";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Payments.Razor.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentController);
        }

        public override void Install()
        {
            var settings = new RazorPaymentSettings()
            {

                Key = "",
                Secret = "",

            };
            _settingService.SaveSetting(settings);

            //locales

            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Razor.RedirectionTip", "You will be redirected to Razor site to complete the order.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Razor.MerchantId", "Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Razor.MerchantId.Hint", "Enter Key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Razor.Key", "Key");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Razor.Key.Hint", "Enter Key.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Razor.Secret", "Secret");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Razor.Secret.Hint", "Enter Secret.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Razor.PayUri", "Pay URI");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Razor.PayUri.Hint", "Enter Pay URI.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Razor.AdditionalFee", "Additional fee");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Razor.AdditionalFee.Hint", "Enter additional fee to charge your customers.");

            base.Install();
        }

        public override void Uninstall()
        {
            //locales

            this.DeletePluginLocaleResource("Plugins.Payments.Razor.RedirectionTip");
            this.DeletePluginLocaleResource("Plugins.Payments.Razor.MerchantId");
            this.DeletePluginLocaleResource("Plugins.Payments.Razor.MerchantId.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Razor.Key");
            this.DeletePluginLocaleResource("Plugins.Payments.Razor.Key.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Razor.Secret");
            this.DeletePluginLocaleResource("Plugins.Payments.Razor.Secret.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Razor.PayUri");
            this.DeletePluginLocaleResource("Plugins.Payments.Razor.PayUri.Hint");
            this.DeletePluginLocaleResource("Plugins.Payments.Razor.AdditionalFee");
            this.DeletePluginLocaleResource("Plugins.Payments.Razor.AdditionalFee.Hint");

            base.Uninstall();
        }
        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }


        public bool SkipPaymentInfo
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to Razor site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payments.Razor.RedirectionTip"); }
        }


        #endregion
    }
}
