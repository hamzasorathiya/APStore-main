using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Security.Captcha;
using Nop.Web.Framework.Security.Honeypot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Web.Models.Customer;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;

namespace Nop.Web.Controllers
{
    public partial class CustomerController
    {
        [StoreClosed(true)]
        [PublicStoreAllowNavigation(true)]
        [HttpPost]
        public virtual ActionResult LoginFromPopup(bool? checkoutAsGuest, string returnUrl = "")
        {
            if (_workContext.CurrentCustomer != null && _workContext.CurrentCustomer.IsRegistered())
                return RedirectToRoute("Homepage");

            var model = _customerModelFactory.PrepareLoginModel(checkoutAsGuest);
            ViewBag.ReturnUrl = returnUrl;
            if (checkoutAsGuest != null && Convert.ToBoolean(checkoutAsGuest))
                return PartialView("_checkoutasguest", model);
            else
                return PartialView("_login", model);
        }


        [HttpPost]
        [CaptchaValidator]
        //available even when a store is closed
        [StoreClosed(true)]
        //available even when navigation is not allowed
        [PublicStoreAllowNavigation(true)]
        [PublicAntiForgery]
        [ValidateAntiForgeryToken]
        public virtual ActionResult AjaxLoginCheck(LoginModel model, string returnUrl, bool captchaValid) //, IFormCollection collection
        {
            LoginModel model1 = new LoginModel();

            //Update model class from form collection
            //TryUpdateModelAsync(model);
            var loginid = 0;

            /*ISettingService settingService = EngineContext.Current.Resolve<ISettingService>();
            int captchaAfterNumberofFailedAttempts = settingService.GetSettingByKey<int>("captchasettings.showonloginpageafternumberoffailedattempts",
                storeId: _storeContext.CurrentStore.Id, loadSharedValueIfNotFound: true);*/

            //var failedAttempts = HttpContext.Session.GetInt32("FailedLoginAttempts") ?? 0;
            var isDisplayCaptcha = false;
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptchaMessage"));
                isDisplayCaptcha = true;
            }

            if (ModelState.IsValid)
            {
                if (_customerSettings.UsernamesEnabled && model.Username != null)
                {
                    model.Username = model.Username.Trim();
                }
                var loginResult = _customerRegistrationService.ValidateCustomer(_customerSettings.UsernamesEnabled ? model.Username : model.Email, model.Password);
                switch (loginResult)
                {
                    case CustomerLoginResults.Successful:
                        {
                            var customer = _customerSettings.UsernamesEnabled
                                ? _customerService.GetCustomerByUsername(model.Username)
                                : _customerService.GetCustomerByEmail(model.Email);
                            loginid = customer.Id;
                            //migrate shopping cart
                            _shoppingCartService.MigrateShoppingCart(_workContext.CurrentCustomer, customer, true);

                            //sign in new customer
                            _authenticationService.SignIn(customer, model.RememberMe);

                            //raise event       
                            _eventPublisher.Publish(new CustomerLoggedinEvent(customer));

                            //activity log
                            _customerActivityService.InsertActivity(customer, "PublicStore.Login",
                                _localizationService.GetResource("ActivityLog.PublicStore.Login"), customer);

                            break;
                        }
                    case CustomerLoginResults.CustomerNotExist:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.CustomerNotExist"));
                        break;
                    case CustomerLoginResults.Deleted:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.Deleted"));
                        break;
                    case CustomerLoginResults.NotActive:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotActive"));
                        break;
                    case CustomerLoginResults.NotRegistered:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.NotRegistered"));
                        break;
                    case CustomerLoginResults.LockedOut:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials.LockedOut"));
                        break;
                    case CustomerLoginResults.WrongPassword:
                    default:
                        ModelState.AddModelError("", _localizationService.GetResource("Account.Login.WrongCredentials"));
                        break;
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? e.Exception.Message : e.ErrorMessage)).ToArray();
            return Json(new
            {
                Errors = errors,
                RedirectURL = returnUrl,
                Id = loginid,
                Email = model.Email,
                IsDisplayCaptcha = isDisplayCaptcha,
            });

        }

    }
}