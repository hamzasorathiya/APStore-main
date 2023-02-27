//using Newtonsoft.Json;
//using Nop.Core.Data;
//using Nop.Core.Domain.Orders;
//using Nop.Plugin.Misc.Picker.Models;
//using Nop.Services.Tasks;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;

namespace Nop.Plugin.Misc.Picker.Services
{
    public class SynchronizationTask /*: ITask*/
    {
        //private readonly IRepository<OrderTrack> _ordertrackRepository;
        //private readonly PickerSettings _pickerSettings;
        //private readonly IRepository<Order> _orderRepository;
        //private readonly IPickerService _pickerservice;


        //public SynchronizationTask(
        //    IRepository<Order> orderRepository,
        //    IRepository<OrderTrack> ordertrackRepository,
        //    IPickerService pickerservice,
        //    PickerSettings pickerSettings)
        //{
        //    this._orderRepository = orderRepository;
        //    this._ordertrackRepository = ordertrackRepository;
        //    this._pickerSettings = pickerSettings;
        //    this._pickerservice = pickerservice;
        //}
        //public void Execute()
        //{
        //    var query =
        //    from ordertrack in _ordertrackRepository.Table.ToList()
        //    join order in _orderRepository.Table.ToList() on ordertrack.OriginalOrderId equals order.Id
        //    select new { OrderTrack = ordertrack, Order = order };

        //    string TrackOrderUrl = _pickerSettings.TrackOrderUrl;
        //    var raw = query.Select(m => m.OrderTrack.tracking_id).ToList();
        //    if (raw.Count != 0)
        //    {
        //        foreach (var trackingid in raw)
        //        {
        //            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        //            HttpClient client = new HttpClient();
        //            client.BaseAddress = new Uri(TrackOrderUrl);
        //            var Token = _pickerSettings.auth_token;
        //            var trackingId = trackingid;
        //            var result = System.Threading.Tasks.Task.Run(async () => await client.GetAsync("https://async.pickrr.com/track/tracking/?tracking_id=" + trackingId + "&auth_token=" + Token + ""));
        //            string resultContent = System.Threading.Tasks.Task.Run(async () => await result.Result.Content.ReadAsStringAsync()).Result;
        //            Shippingdata shippingdata = new Shippingdata();
        //            shippingdata = JsonConvert.DeserializeObject<Shippingdata>(resultContent);

        //            List<PickerShippingInformation> listOfShippingInfo = _pickerservice.GetPickerShippingInformationByTrackingId(trackingid);

        //            if (shippingdata?.track_arr?.Length > 0)
        //            {

        //                foreach (Track_Arr item in shippingdata?.track_arr?.ToList())
        //                {
        //                    var pickershippinginformation = new PickerShippingInformation();
        //                    if (item != null)
        //                    {
        //                        if (listOfShippingInfo != null && listOfShippingInfo.Count > 0)
        //                        {
        //                            var shippingInfo = listOfShippingInfo.Where(x => x.status_name == item.status_name).FirstOrDefault();
        //                            if (shippingInfo != null)
        //                            {
        //                                //Update
        //                                pickershippinginformation.tracking_id = shippingdata.tracking_id;
        //                                pickershippinginformation.status_name = Convert.ToString(item.status_name);
        //                                pickershippinginformation.status_body = Convert.ToString(item.status_array.Select(l => l.status_body).FirstOrDefault());
        //                                pickershippinginformation.status_time = Convert.ToString(item.status_array.Select(l => l.status_time).FirstOrDefault());
        //                                pickershippinginformation.status_location = Convert.ToString(item.status_array.Select(l => l.status_location).FirstOrDefault());
        //                                pickershippinginformation.current_status_body = shippingdata.status.current_status_body;
        //                                pickershippinginformation.current_status_type = shippingdata.status.current_status_type;
        //                                pickershippinginformation.current_status_location = shippingdata.status.current_status_location;
        //                                pickershippinginformation.to_city = shippingdata.info.to_city;
        //                                pickershippinginformation.to_pincode = shippingdata.info.to_pincode;
        //                                pickershippinginformation.from_pincode = shippingdata.info.from_pincode;
        //                                pickershippinginformation.from_city = shippingdata.info.from_city;
        //                                _pickerservice.UpdateShippingDetails(pickershippinginformation);
        //                            }
        //                            else
        //                            {
        //                                //Insert
        //                                pickershippinginformation.tracking_id = shippingdata.tracking_id;
        //                                pickershippinginformation.status_name = Convert.ToString(item.status_name);
        //                                pickershippinginformation.status_body = Convert.ToString(item.status_array.Select(l => l.status_body).FirstOrDefault());
        //                                pickershippinginformation.status_time = Convert.ToString(item.status_array.Select(l => l.status_time).FirstOrDefault());
        //                                pickershippinginformation.status_location = Convert.ToString(item.status_array.Select(l => l.status_location).FirstOrDefault());
        //                                pickershippinginformation.current_status_body = shippingdata.status.current_status_body;
        //                                pickershippinginformation.current_status_type = shippingdata.status.current_status_type;
        //                                pickershippinginformation.current_status_location = shippingdata.status.current_status_location;
        //                                pickershippinginformation.to_city = shippingdata.info.to_city;
        //                                pickershippinginformation.to_pincode = shippingdata.info.to_pincode;
        //                                pickershippinginformation.from_pincode = shippingdata.info.from_pincode;
        //                                pickershippinginformation.from_city = shippingdata.info.from_city;
        //                                _pickerservice.InsertShippingDetails(pickershippinginformation);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            pickershippinginformation.tracking_id = shippingdata.tracking_id;
        //                            pickershippinginformation.status_name = Convert.ToString(item.status_name);
        //                            pickershippinginformation.status_body = Convert.ToString(item.status_array.Select(l => l.status_body).FirstOrDefault());
        //                            pickershippinginformation.status_time = Convert.ToString(item.status_array.Select(l => l.status_time).FirstOrDefault());
        //                            pickershippinginformation.status_location = Convert.ToString(item.status_array.Select(l => l.status_location).FirstOrDefault());
        //                            pickershippinginformation.current_status_body = shippingdata.status.current_status_body;
        //                            pickershippinginformation.current_status_type = shippingdata.status.current_status_type;
        //                            pickershippinginformation.current_status_location = shippingdata.status.current_status_location;
        //                            pickershippinginformation.to_city = shippingdata.info.to_city;
        //                            pickershippinginformation.to_pincode = shippingdata.info.to_pincode;
        //                            pickershippinginformation.from_pincode = shippingdata.info.from_pincode;
        //                            pickershippinginformation.from_city = shippingdata.info.from_city;
        //                            _pickerservice.InsertShippingDetails(pickershippinginformation);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
