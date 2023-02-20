using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Plugin.Misc.Picker.Models;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.Picker.Controllers
{
    [AdminAuthorize]
    public class MiscPickerController : BasePluginController
    {
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly PickerSettings _pickerSettings;

        public MiscPickerController(ISettingService settingService,
            IStoreService storeService,
            IWorkContext workContext,
            PickerSettings pickerSettings)
        {
            this._pickerSettings = pickerSettings;
            this._settingService = settingService;
            this._storeService = storeService;
            this._workContext = workContext;
        }        
        static async Task MainAsync()
        {
            using (var client = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.BaseAddress = new Uri("https://pickrr.com/api/place-order/");                
                var a = new
                {
                    auth_token = "daad14a589de956daae463911334a726186862",
                    item_name = "",
                    item_list = @"[ {""price"": ""1200.0"", ""item_name"": ""Shirts"", ""quantity"": ""1"", ""sku"": ""gold"", ""item_tax_percentage"":""12""},
                     {""price"": ""1000.0"", ""item_name"": ""t-Shirts"", ""quantity"": ""1"", ""sku"": ""pink"", ""item_tax_percentage"":""10""},
                     {""price"": ""800.0"", ""item_name"": ""Wallets"", ""quantity"": ""2"", ""sku"": ""black"", ""item_tax_percentage"":""20""}]",
                    from_name = "Moin jujeja",
                    to_email = "clark@gmail.com",
                    from_phone_number = "7351857301",
                    from_address = "Basement, 1007 Mountain Drive",
                    from_pincode = "110022",
                    pickup_gstin = "XXXXXXXXXX",
                    to_name = "Hamza sorathiya",
                    to_phone_number = "7738828473",
                    to_pincode = "120002",
                    to_address = "344 Clinton Street",
                    quantity = 4,
                    invoice_value = 3550,
                    cod_amount = 3800,
                    client_order_id = "3407",
                    item_breadth = 10,
                    item_length = 10,
                    item_height = 5,
                    item_weight = 0.5,
                    is_reverse = false,
                    invoice_number = "657",
                    total_discount = 100,
                    shipping_charge = 50,
                    transaction_charge = 50,
                    giftwrap_charge = 50
                };
                var json = JsonConvert.SerializeObject(a);
                var Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");    
                var result = await client.PostAsync("https://pickrr.com/api/place-order/", Content);
                string resultContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine(resultContent);
            }
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var pickerSettings = _settingService.LoadSetting<PickerSettings>(storeScope);

            var model = new PickerMiscModel();
            model.auth_Token = pickerSettings.auth_token;
            model.Url = pickerSettings.Url;

            return View("~/Plugins/Misc.Picker/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [ChildActionOnly]
        public ActionResult Configure(PickerMiscModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }

            //save settings
            _pickerSettings.auth_token = model.auth_Token;
            _pickerSettings.Url = model.Url;

            _settingService.SaveSetting(_pickerSettings);

            return Configure();
        }
        //public void HandleEvent(OrderPlacedEvent eventMessage)
        //{
        //    var order = eventMessage.Order;
        //    var products = order.OrderItems;
        //}
    }
}
