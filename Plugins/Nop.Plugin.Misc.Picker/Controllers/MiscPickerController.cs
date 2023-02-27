using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Nop.Admin.Models.Orders;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Picker.Models;
using Nop.Plugin.Misc.Picker.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Vendors;
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
        private readonly IPickerService _pickerService;
        private readonly IAddressService _addressService;
        private readonly IVendorService _vendorService;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly IRepository<OrderTrack> _ordertrackRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IPickerService _pickerservice;

        public MiscPickerController(ISettingService settingService,
            IStoreService storeService,
            IWorkContext workContext,
            IPickerService pickerService,
            IOrderService orderService,
            IAddressService addressService,
            PickerSettings pickerSettings,
            IVendorService vendorService,
            ILogger logger,
            IRepository<OrderTrack> ordertrackRepository,
            IRepository<Order> orderRepository,
            IPickerService pickerservice)
        {
            this._pickerSettings = pickerSettings;
            this._settingService = settingService;
            this._storeService = storeService;
            this._workContext = workContext;
            this._pickerService = pickerService;
            this._addressService = addressService;
            this._orderService = orderService;
            this._vendorService = vendorService;
            this._logger = logger;
            this._ordertrackRepository = ordertrackRepository;
            this._orderRepository = orderRepository;
            this._pickerservice = pickerservice;
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
            model.PlaceOrderUrl = pickerSettings.PlaceOrderUrl;
            model.CancelOrderUrl = pickerSettings.CancelOrderUrl;
            model.TrackOrderUrl = pickerSettings.TrackOrderUrl;

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
            _pickerSettings.PlaceOrderUrl = model.PlaceOrderUrl;
            _pickerSettings.CancelOrderUrl = model.CancelOrderUrl;
            _pickerSettings.TrackOrderUrl = model.TrackOrderUrl;

            _settingService.SaveSetting(_pickerSettings);

            return Configure();
        }


        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone, int additionalData)
        {
            List<string> TrackingIds = _pickerService.GetOrderTrackInformationByOrderId(additionalData).Select(k => k.tracking_id).ToList();
            if (TrackingIds.Count == 0)
            {
                TrackingIds.Add("6234987569");
                TrackingInformation trackingInformation = new TrackingInformation()
                {
                    TrackingNumber = TrackingIds
                };

                return View("~/Plugins/Misc.Picker/Views/PublicInfo.cshtml", trackingInformation);
            }
            else
            {
                TrackingInformation trackingInformation = new TrackingInformation()
                {
                    TrackingNumber = TrackingIds
                };

                return View("~/Plugins/Misc.Picker/Views/PublicInfo.cshtml", trackingInformation);
            }
        }


        [ChildActionOnly]
        public ActionResult Orderplaceonpicker(string widgetZone)
        {
            var model = new OrderModel();
            bool url = Request.Url.AbsoluteUri.ToLower().IndexOf("admin/order/edit") != -1;
            if (url == true)
            {
                int URL = Convert.ToInt32(Url.RequestContext.RouteData.Values["id"]);
                var order = _orderService.GetOrderById(URL);
                model.Id = order.Id;
            }

            return View("~/Plugins/Misc.Picker/Views/Orderplaceonpicker.cshtml", model);
        }
        

        [HttpPost]
        public ActionResult Placeorderonpicker(int orderId)
        {
            List<object> Placeapiparameters = new List<object>();
            object PlaceOrderApiParameters = new { };
            try
            {
                var order = _orderService.GetOrderById(orderId);
                if (order != null)
                {
                    _logger.Information(string.Format("Order found"));
                }
                string Token = _pickerSettings.auth_token;
                if (!String.IsNullOrEmpty(Token))
                {
                    _logger.Information(string.Format("Token found"));
                }
                string PlaceOrderUrl = _pickerSettings.PlaceOrderUrl;
                if (!String.IsNullOrEmpty(PlaceOrderUrl))
                {
                    _logger.Information(string.Format("Place order url found"));
                }
                List<int> VendorIds = order.OrderItems.Select(b => b.Product.VendorId).Distinct().ToList();
                if (VendorIds != null && VendorIds.Count > 0)
                {
                    _logger.Information(string.Format("Vendorid found"));
                }

                if (!String.IsNullOrEmpty(Token))
                {
                    int i = 0;
                    foreach (var id in VendorIds)
                    {
                        List<string> item_name = order.OrderItems.Where(h => h.Product.VendorId == id).Select(b => b.Product.Name).ToList();

                        if (item_name.Count > 1)
                        {
                            i++;
                            var VendorDetail = _vendorService.GetVendorById(id);
                            if (VendorDetail != null)
                            {
                                _logger.Information(string.Format("VendorDetail found"));
                            }
                            var price = order.OrderItems.Where(h => h.Product.VendorId == id).Select(x => x.PriceExclTax).ToList();
                            var actualprice = price.Sum();
                            decimal totalPrice;
                            if (i == 1)
                            {
                                totalPrice = actualprice + order.PaymentMethodAdditionalFeeExclTax;
                            }
                            else
                            {
                                totalPrice = actualprice;
                            }
                            int VendorAddresId = VendorDetail.AddressId;
                            if (VendorAddresId != 0)
                            {
                                _logger.Information(string.Format("VendorAddresId found"));
                            }
                            var VendorAddress = _addressService.GetAddressById(VendorAddresId);
                            if (VendorAddress != null)
                            {
                                _logger.Information(string.Format("VendorAddress found"));
                            }
                            if (VendorDetail != null)
                            {
                                if (order.ShippingMethod == "Cash On Delivery")
                                {
                                    PlaceOrderApiParameters = new
                                    {
                                        auth_token = Token,
                                        item_name = JsonConvert.SerializeObject(item_name),
                                        from_name = VendorDetail.Name,
                                        from_phone_number = VendorAddress.PhoneNumber,
                                        from_address = VendorAddress.Address1,
                                        from_pincode = VendorAddress.ZipPostalCode,
                                        to_name = order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName,
                                        to_phone_number = order.ShippingAddress.PhoneNumber,
                                        to_address = order.ShippingAddress.Address1 + ", " + order.ShippingAddress.City + ",  " + order.ShippingAddress.StateProvince.Name + ",  " + order.ShippingAddress.Country.Name,
                                        to_pincode = order.ShippingAddress.ZipPostalCode,
                                        invoice_value = totalPrice,
                                        cod_amount = totalPrice,
                                    };
                                    Placeapiparameters.Add(PlaceOrderApiParameters);
                                }
                                else
                                {
                                    PlaceOrderApiParameters = new
                                    {
                                        auth_token = Token,
                                        item_name = JsonConvert.SerializeObject(item_name),
                                        from_name = VendorDetail.Name,
                                        from_phone_number = VendorAddress.PhoneNumber,
                                        from_address = VendorAddress.Address1,
                                        from_pincode = VendorAddress.ZipPostalCode,
                                        to_name = order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName,
                                        to_phone_number = order.ShippingAddress.PhoneNumber,
                                        to_address = order.ShippingAddress.Address1 + ", " + order.ShippingAddress.City + ",  " + order.ShippingAddress.StateProvince.Name + ",  " + order.ShippingAddress.Country.Name,
                                        to_pincode = order.ShippingAddress.ZipPostalCode,
                                        invoice_value = totalPrice,
                                    };
                                    Placeapiparameters.Add(PlaceOrderApiParameters);
                                }
                            }

                        }
                        else
                        {
                            i++;
                            var VendorDetail = _vendorService.GetVendorById(id);
                            if (VendorDetail != null)
                            {
                                _logger.Information(string.Format("VendorDetail found"));
                            }
                            var price = order.OrderItems.Where(h => h.Product.VendorId == id).Select(x => x.PriceExclTax).FirstOrDefault();
                            decimal totalPrice;
                            if (i == 1)
                            {
                                totalPrice = price + order.PaymentMethodAdditionalFeeExclTax;
                            }
                            else
                            {
                                totalPrice = price;
                            }
                            int VendorAddresId = VendorDetail.AddressId;
                            if (VendorAddresId != 0)
                            {
                                _logger.Information(string.Format("VendorAddresId found"));
                            }
                            var VendorAddress = _addressService.GetAddressById(VendorAddresId);
                            if (VendorAddress != null)
                            {
                                _logger.Information(string.Format("VendorAddress found"));
                            }
                            if (VendorDetail != null)
                            {
                                if (order.ShippingMethod == "Cash On Delivery")
                                {
                                    PlaceOrderApiParameters = new
                                    {
                                        auth_token = Token,
                                        item_name = JsonConvert.SerializeObject(item_name),
                                        from_name = VendorDetail.Name,
                                        from_phone_number = VendorAddress.PhoneNumber,
                                        from_address = VendorAddress.Address1,
                                        from_pincode = VendorAddress.ZipPostalCode,
                                        to_name = order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName,
                                        to_phone_number = order.ShippingAddress.PhoneNumber,
                                        to_address = order.ShippingAddress.Address1 + ", " + order.ShippingAddress.City + ",  " + order.ShippingAddress.Country.Name,
                                        to_pincode = order.ShippingAddress.ZipPostalCode,
                                        invoice_value = totalPrice,
                                        cod_amount = totalPrice,
                                    };
                                    Placeapiparameters.Add(PlaceOrderApiParameters);
                                }
                                else
                                {
                                    PlaceOrderApiParameters = new
                                    {
                                        auth_token = Token,
                                        item_name = JsonConvert.SerializeObject(item_name),
                                        from_name = VendorDetail.Name,
                                        from_phone_number = VendorAddress.PhoneNumber,
                                        from_address = VendorAddress.Address1,
                                        from_pincode = VendorAddress.ZipPostalCode,
                                        to_name = order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName,
                                        to_phone_number = order.ShippingAddress.PhoneNumber,
                                        to_address = order.ShippingAddress.Address1 + ", " + order.ShippingAddress.City + ",  " + order.ShippingAddress.Country.Name,
                                        to_pincode = order.ShippingAddress.ZipPostalCode,
                                        invoice_value = totalPrice,
                                    };
                                    Placeapiparameters.Add(PlaceOrderApiParameters);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                _logger.Error(exc.Message, exc);
                throw;
            }

            return Json(Placeapiparameters);
        }


        [HttpPost]
        public void Getplaceorderapiresponse(string Apiresponse = "", int orderId = 0)
        {
            Rootobject rootobject = new Rootobject();
            rootobject = JsonConvert.DeserializeObject<Rootobject>(Apiresponse);
            var ordertrack = new OrderTrack
            {
                err = (string)rootobject.err,
                success = rootobject.success,
                tracking_id = rootobject.tracking_id,
                order_id = rootobject.order_id,
                order_pk = rootobject.order_pk,
                manifest_link = rootobject.manifest_link,
                routing_code = rootobject.routing_code,
                courier = rootobject.courier,
                dispatch_mode = rootobject.dispatch_mode,
                courier_id = rootobject.courier_id,
                manifest_img_link_v2 = rootobject.manifest_img_link_v2,
                manifest_link_pdf = rootobject.manifest_link_pdf,
                edd_stamp = rootobject.edd_stamp,
                manifest_img_link = rootobject.manifest_img_link,
                track_url = rootobject.track_url,
                OriginalOrderId = orderId,
            };
            _pickerService.InsertOrderDetails(ordertrack);
        }


        [HttpPost]
        public ActionResult Cancelorderonpicker(int ordernumber)
        {
            List<object> CancelApiParameters = new List<object>();
            object CancelOrderApiParameters = new { };
            var raw = _ordertrackRepository.Table.Where(x => x.OriginalOrderId == ordernumber).ToList();
            string Token = _pickerSettings.auth_token;
            foreach (var item in raw)
            {
                CancelOrderApiParameters = new
                {
                    auth_token = Token,
                    tracking_id = item.tracking_id,
                };
                CancelApiParameters.Add(CancelOrderApiParameters);
            }

            return Json(CancelApiParameters);
        }


        [HttpPost]
        public ActionResult Trackorderonpicker(string tracking_id)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ShippingInformation shippingInformation = new ShippingInformation();
            var Token = _pickerSettings.auth_token;
            var trackingId = tracking_id;
            string TrackOrderUrl = _pickerSettings.TrackOrderUrl;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(TrackOrderUrl);
            var result = Task.Run(async () => await client.GetAsync("https://async.pickrr.com/track/tracking/?tracking_id=" + trackingId + "&auth_token=" + Token + ""));
            string resultContent = Task.Run(async () => await result.Result.Content.ReadAsStringAsync()).Result;
            Shippingdata shippingdata = new Shippingdata();
            shippingdata = JsonConvert.DeserializeObject<Shippingdata>(resultContent);

            List<PickerShippingInformation> listOfShippingInfo = _pickerservice.GetPickerShippingInformationByTrackingId(shippingdata.tracking_id);
            if (shippingdata?.track_arr?.Length > 0)
            {
                foreach (Track_Arr item in shippingdata?.track_arr?.ToList())
                {
                    var pickershippinginformation = new PickerShippingInformation();
                    if (item != null)
                    {
                        if (listOfShippingInfo != null && listOfShippingInfo.Count > 0)
                        {
                            var shippingInfo = listOfShippingInfo.Where(x => x.status_name == item.status_name).FirstOrDefault();
                            if (shippingInfo != null)
                            {
                                //Update
                                pickershippinginformation.tracking_id = shippingdata.tracking_id;
                                pickershippinginformation.status_name = Convert.ToString(item.status_name);
                                pickershippinginformation.status_body = Convert.ToString(item.status_array.Select(l => l.status_body).FirstOrDefault());
                                pickershippinginformation.status_time = Convert.ToString(item.status_array.Select(l => l.status_time).FirstOrDefault());
                                pickershippinginformation.status_location = Convert.ToString(item.status_array.Select(l => l.status_location).FirstOrDefault());
                                pickershippinginformation.current_status_body = shippingdata.status.current_status_body;
                                pickershippinginformation.current_status_type = shippingdata.status.current_status_type;
                                pickershippinginformation.current_status_location = shippingdata.status.current_status_location;
                                pickershippinginformation.to_city = shippingdata.info.to_city;
                                pickershippinginformation.to_pincode = shippingdata.info.to_pincode;
                                pickershippinginformation.from_pincode = shippingdata.info.from_pincode;
                                pickershippinginformation.from_city = shippingdata.info.from_city;
                                _pickerservice.UpdateShippingDetails(pickershippinginformation);
                            }
                            else
                            {
                                //Insert
                                pickershippinginformation.tracking_id = shippingdata.tracking_id;
                                pickershippinginformation.status_name = Convert.ToString(item.status_name);
                                pickershippinginformation.status_body = Convert.ToString(item.status_array.Select(l => l.status_body).FirstOrDefault());
                                pickershippinginformation.status_time = Convert.ToString(item.status_array.Select(l => l.status_time).FirstOrDefault());
                                pickershippinginformation.status_location = Convert.ToString(item.status_array.Select(l => l.status_location).FirstOrDefault());
                                pickershippinginformation.current_status_body = shippingdata.status.current_status_body;
                                pickershippinginformation.current_status_type = shippingdata.status.current_status_type;
                                pickershippinginformation.current_status_location = shippingdata.status.current_status_location;
                                pickershippinginformation.to_city = shippingdata.info.to_city;
                                pickershippinginformation.to_pincode = shippingdata.info.to_pincode;
                                pickershippinginformation.from_pincode = shippingdata.info.from_pincode;
                                pickershippinginformation.from_city = shippingdata.info.from_city;
                                _pickerservice.InsertShippingDetails(pickershippinginformation);
                            }
                        }
                        else
                        {
                            pickershippinginformation.tracking_id = shippingdata.tracking_id;
                            pickershippinginformation.status_name = Convert.ToString(item.status_name);
                            pickershippinginformation.status_body = Convert.ToString(item.status_array.Select(l => l.status_body).FirstOrDefault());
                            pickershippinginformation.status_time = Convert.ToString(item.status_array.Select(l => l.status_time).FirstOrDefault());
                            pickershippinginformation.status_location = Convert.ToString(item.status_array.Select(l => l.status_location).FirstOrDefault());
                            pickershippinginformation.current_status_body = shippingdata.status.current_status_body;
                            pickershippinginformation.current_status_type = shippingdata.status.current_status_type;
                            pickershippinginformation.current_status_location = shippingdata.status.current_status_location;
                            pickershippinginformation.to_city = shippingdata.info.to_city;
                            pickershippinginformation.to_pincode = shippingdata.info.to_pincode;
                            pickershippinginformation.from_pincode = shippingdata.info.from_pincode;
                            pickershippinginformation.from_city = shippingdata.info.from_city;
                            _pickerservice.InsertShippingDetails(pickershippinginformation);
                        }
                    }
                }                
            }
            var ShippingInfo = _pickerService.GetPickerShippingInformationByTrackingId(tracking_id).ToList();
            List<statusinfo> statusinfo = new List<statusinfo>();
            foreach (var item in ShippingInfo)
            {
                statusinfo.Add(new statusinfo
                {
                    status_name = item.status_name,
                    status_body = item.status_body,
                    status_time = item.status_time,
                    status_location = item.status_location,
                });
            }
            shippingInformation.statusinfo = statusinfo;

            return PartialView("~/Plugins/Misc.Picker/Views/_ShippingPopUp.cshtml", shippingInformation);
        }


        [HttpPost]
        public ActionResult Placeorderonpickerselected(ICollection<int> selectedIds)
        {
            List<object> Placebulkapiparameters = new List<object>();
            object PlacebulkOrderApiParameters = new { };
            if (selectedIds != null)
            {
                foreach (var OrderId in selectedIds)
                {
                    try
                    {
                        var order = _orderService.GetOrderById(OrderId);
                        if (order != null)
                        {
                            _logger.Information(string.Format("Order found"));
                        }
                        string Token = _pickerSettings.auth_token;
                        if (!String.IsNullOrEmpty(Token))
                        {
                            _logger.Information(string.Format("Token found"));
                        }
                        string PlaceOrderUrl = _pickerSettings.PlaceOrderUrl;
                        if (!String.IsNullOrEmpty(PlaceOrderUrl))
                        {
                            _logger.Information(string.Format("Place order url found"));
                        }
                        List<int> VendorIds = order.OrderItems.Select(b => b.Product.VendorId).Distinct().ToList();
                        if (VendorIds != null && VendorIds.Count > 0)
                        {
                            _logger.Information(string.Format("Vendorid found"));
                        }
                        if (!String.IsNullOrEmpty(Token))
                        {
                            int i = 0;
                            foreach (var id in VendorIds)
                            {                                
                                List<string> item_name = order.OrderItems.Where(h => h.Product.VendorId == id).Select(b => b.Product.Name).ToList();

                                if (item_name.Count > 1)
                                {
                                    i++;
                                    var VendorDetail = _vendorService.GetVendorById(id);
                                    if (VendorDetail != null)
                                    {
                                        _logger.Information(string.Format("VendorDetail found"));
                                    }
                                    var price = order.OrderItems.Where(h => h.Product.VendorId == id).Select(x => x.PriceExclTax).ToList();
                                    var actualprice = price.Sum();
                                    decimal totalPrice;
                                    if (i == 1)
                                    {
                                        totalPrice = actualprice + order.PaymentMethodAdditionalFeeExclTax;
                                    }
                                    else
                                    {
                                        totalPrice = actualprice;
                                    }
                                    int VendorAddresId = VendorDetail.AddressId;
                                    if (VendorAddresId != 0)
                                    {
                                        _logger.Information(string.Format("VendorAddresId found"));
                                    }
                                    var VendorAddress = _addressService.GetAddressById(VendorAddresId);
                                    if (VendorAddress != null)
                                    {
                                        _logger.Information(string.Format("VendorAddress found"));
                                    }
                                    if (VendorDetail != null)
                                    {                                                                                
                                        if (order.ShippingMethod == "Cash On Delivery")
                                        {
                                            PlacebulkOrderApiParameters = new
                                            {
                                                auth_token = Token,
                                                item_name = JsonConvert.SerializeObject(item_name),
                                                from_name = VendorDetail.Name,
                                                from_phone_number = VendorAddress.PhoneNumber,
                                                from_address = VendorAddress.Address1,
                                                from_pincode = VendorAddress.ZipPostalCode,
                                                to_name = order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName,
                                                to_phone_number = order.ShippingAddress.PhoneNumber,
                                                to_address = order.ShippingAddress.Address1 + ", " + order.ShippingAddress.City + ",  " + order.ShippingAddress.StateProvince.Name + ",  " + order.ShippingAddress.Country.Name,
                                                to_pincode = order.ShippingAddress.ZipPostalCode,
                                                invoice_value = totalPrice,
                                                cod_amount = totalPrice,
                                                Originalorderid = OrderId,
                                        };
                                            Placebulkapiparameters.Add(PlacebulkOrderApiParameters);
                                        }
                                        else
                                        {
                                            PlacebulkOrderApiParameters = new
                                            {
                                                auth_token = Token,
                                                item_name = JsonConvert.SerializeObject(item_name),
                                                from_name = VendorDetail.Name,
                                                from_phone_number = VendorAddress.PhoneNumber,
                                                from_address = VendorAddress.Address1,
                                                from_pincode = VendorAddress.ZipPostalCode,
                                                to_name = order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName,
                                                to_phone_number = order.ShippingAddress.PhoneNumber,
                                                to_address = order.ShippingAddress.Address1 + ", " + order.ShippingAddress.City + ",  " + order.ShippingAddress.StateProvince.Name + ",  " + order.ShippingAddress.Country.Name,
                                                to_pincode = order.ShippingAddress.ZipPostalCode,
                                                invoice_value = totalPrice,
                                                Originalorderid = OrderId,
                                            };
                                            Placebulkapiparameters.Add(PlacebulkOrderApiParameters);
                                        }                                        
                                    }
                                }
                                else
                                {
                                    i++;
                                    var VendorDetail = _vendorService.GetVendorById(id);
                                    var price = order.OrderItems.Where(h => h.Product.VendorId == id).Select(x => x.PriceExclTax).FirstOrDefault();
                                    decimal totalPrice;
                                    if (i == 1)
                                    {
                                        totalPrice = price + order.PaymentMethodAdditionalFeeExclTax;
                                    }
                                    else
                                    {
                                        totalPrice = price;
                                    }
                                    int VendorAddresId = VendorDetail.AddressId;
                                    var VendorAddress = _addressService.GetAddressById(VendorAddresId);
                                    if (VendorDetail != null)
                                    {
                                        if (order.ShippingMethod == "Cash On Delivery")
                                        {
                                            PlacebulkOrderApiParameters = new
                                            {
                                                auth_token = Token,
                                                item_name = JsonConvert.SerializeObject(item_name),
                                                from_name = VendorDetail.Name,
                                                from_phone_number = VendorAddress.PhoneNumber,
                                                from_address = VendorAddress.Address1,
                                                from_pincode = VendorAddress.ZipPostalCode,
                                                to_name = order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName,
                                                to_phone_number = order.ShippingAddress.PhoneNumber,
                                                to_address = order.ShippingAddress.Address1 + ", " + order.ShippingAddress.City + ",  " + order.ShippingAddress.Country.Name,
                                                to_pincode = order.ShippingAddress.ZipPostalCode,
                                                invoice_value = totalPrice,
                                                cod_amount = totalPrice,
                                                Originalorderid = OrderId,
                                            };
                                            Placebulkapiparameters.Add(PlacebulkOrderApiParameters);
                                        }
                                        else
                                        {
                                            PlacebulkOrderApiParameters = new
                                            {
                                                auth_token = Token,
                                                item_name = JsonConvert.SerializeObject(item_name),
                                                from_name = VendorDetail.Name,
                                                from_phone_number = VendorAddress.PhoneNumber,
                                                from_address = VendorAddress.Address1,
                                                from_pincode = VendorAddress.ZipPostalCode,
                                                to_name = order.ShippingAddress.FirstName + " " + order.ShippingAddress.LastName,
                                                to_phone_number = order.ShippingAddress.PhoneNumber,
                                                to_address = order.ShippingAddress.Address1 + ", " + order.ShippingAddress.City + ",  " + order.ShippingAddress.StateProvince.Name + ",  " + order.ShippingAddress.Country.Name,
                                                to_pincode = order.ShippingAddress.ZipPostalCode,
                                                invoice_value = totalPrice,
                                                Originalorderid = OrderId,
                                            };
                                            Placebulkapiparameters.Add(PlacebulkOrderApiParameters);
                                        }                                       
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        _logger.Error(exc.Message, exc);
                        throw;
                    }
                }
            }

            return Json(Placebulkapiparameters);
        }

        [HttpPost]
        public void Getplacebulkorderapiresponse(string Apiresponse = "", string orderId = "")
        {
            Rootobject rootobject = new Rootobject();
            rootobject = JsonConvert.DeserializeObject<Rootobject>(Apiresponse);
            var ordertrack = new OrderTrack
            {
                err = (string)rootobject.err,
                success = rootobject.success,
                tracking_id = rootobject.tracking_id,
                order_id = rootobject.order_id,
                order_pk = rootobject.order_pk,
                manifest_link = rootobject.manifest_link,
                routing_code = rootobject.routing_code,
                courier = rootobject.courier,
                dispatch_mode = rootobject.dispatch_mode,
                courier_id = rootobject.courier_id,
                manifest_img_link_v2 = rootobject.manifest_img_link_v2,
                manifest_link_pdf = rootobject.manifest_link_pdf,
                edd_stamp = rootobject.edd_stamp,
                manifest_img_link = rootobject.manifest_img_link,
                track_url = rootobject.track_url,
                OriginalOrderId = Convert.ToInt32(orderId),
            };
            _pickerService.InsertOrderDetails(ordertrack);
        }
    }
}
