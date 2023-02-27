using Newtonsoft.Json;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.Picker.Models;
using Nop.Services.Events;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.Picker.Events
{
    //public class OrderEventconsumer : IConsumer<OrderCancelledEvent>
    //{
    //    private readonly PickerSettings _pickerSettings;
    //    private readonly IRepository<OrderTrack> _ordertrackRepository;

    //    public OrderEventconsumer(
    //        IRepository<OrderTrack> ordertrackRepository,
    //        PickerSettings pickerSettings)
    //    {
    //        this._pickerSettings = pickerSettings;
    //        this._ordertrackRepository = ordertrackRepository;
    //    }


    //    //public void HandleEvent(OrderCancelledEvent eventMessage)
    //    //{
    //    //    var order = eventMessage.Order;
    //    //    var products = order.OrderItems;
    //    //    var raw = _ordertrackRepository.Table.Where(x => x.OriginalOrderId == order.Id).ToList();
    //    //    string Token = _pickerSettings.auth_token;
    //    //    string CancelOrderUrl = _pickerSettings.CancelOrderUrl;

    //    //    foreach (var item in raw)
    //    //    {
    //    //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
    //    //        HttpClient client = new HttpClient();
    //    //        client.BaseAddress = new Uri(CancelOrderUrl);
    //    //        var CancelOrderApiParameters = new
    //    //        {
    //    //            auth_token = Token,
    //    //            tracking_id = item.tracking_id,
    //    //        };
    //    //        var json = JsonConvert.SerializeObject(CancelOrderApiParameters);
    //    //        var Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
    //    //        var result = Task.Run(async () => await client.PostAsync(CancelOrderUrl, Content));
    //    //        string resultContent = Task.Run(async () => await result.Result.Content.ReadAsStringAsync()).Result;


    //    //        //var httpWebRequest = (HttpWebRequest)WebRequest.Create(CancelOrderUrl);
    //    //        //httpWebRequest.ContentType = "application/x-www-form-urlencoded";
    //    //        //httpWebRequest.Method = "POST";
    //    //        //var CancelOrderApiParameters = new
    //    //        //{
    //    //        //    auth_token = Token,
    //    //        //    tracking_id = item.tracking_id,
    //    //        //};
    //    //        //byte[] byteArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(CancelOrderApiParameters));
    //    //        //httpWebRequest.ContentLength = byteArray.Length;
    //    //        //Stream dataStream = httpWebRequest.GetRequestStream();
    //    //        //// Write the data to the request stream.
    //    //        //dataStream.Write(byteArray, 0, byteArray.Length);
    //    //        //// Close the Stream object.
    //    //        //dataStream.Close();
    //    //        //// Get the response.
    //    //        //WebResponse response = httpWebRequest.GetResponse();
    //    //        //dataStream = response.GetResponseStream();
    //    //        //// Open the stream using a StreamReader for easy access.
    //    //        //StreamReader reader = new StreamReader(dataStream);
    //    //        //// Read the content.
    //    //        //string responseFromServer = reader.ReadToEnd();
    //    //        //// Clean up the streams.
    //    //        //reader.Close();
    //    //        //dataStream.Close();
    //    //        //response.Close();
    //    //    }
    //    //}
    //}
}
