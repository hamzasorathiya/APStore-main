using Nop.Plugin.Misc.Picker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.Picker.Services
{
    public partial interface IPickerService
    {
        void InsertOrderDetails(OrderTrack ordertrack);

        void InsertShippingDetails(PickerShippingInformation pickershippinginformation);

        void UpdateShippingDetails(PickerShippingInformation pickershippinginformation);

        IList<OrderTrack> GetAllOrderDetails();

        List<PickerShippingInformation> GetPickerShippingInformationByTrackingId(string trackingId);

        List<OrderTrack> GetOrderTrackInformationByOrderId(int OrderId);
    }
}
