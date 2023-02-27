using Nop.Core.Data;
using Nop.Plugin.Misc.Picker.Models;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.Picker.Services
{
    public partial class PickerService : IPickerService
    {
        private readonly IRepository<OrderTrack> _ordertrackRepository;
        private readonly IRepository<PickerShippingInformation> _pickershippinginformationrepository;

        public PickerService(IRepository<OrderTrack> ordertrackRepository,
            IRepository<PickerShippingInformation> pickershippinginformationrepository)
        {
            this._ordertrackRepository = ordertrackRepository;
            this._pickershippinginformationrepository = pickershippinginformationrepository;
        }

        public virtual void InsertOrderDetails(OrderTrack ordertrack)
        {
            if (ordertrack == null)
                throw new ArgumentNullException("ordertrack");

            _ordertrackRepository.Insert(ordertrack);

            //event notification
            //_eventPublisher.EntityInserted(ordertrack);
        }

        public virtual IList<OrderTrack> GetAllOrderDetails()
        {
            var query = _ordertrackRepository.Table;

            var details = query.ToList();
            return details;
        }

        public virtual void InsertShippingDetails(PickerShippingInformation pickershippinginformation)
        {
            if (pickershippinginformation == null)
                throw new ArgumentNullException("pickershippinginformation");

            _pickershippinginformationrepository.Insert(pickershippinginformation);
        }

        public virtual void UpdateShippingDetails(PickerShippingInformation pickershippinginformation)
        {
            if (pickershippinginformation == null)
                throw new ArgumentNullException("pickershippinginformation");

            _pickershippinginformationrepository.Update(pickershippinginformation);
        }

        public virtual List<PickerShippingInformation> GetPickerShippingInformationByTrackingId(string trackingId)
        {
            var query = _pickershippinginformationrepository.Table.Where(x => x.tracking_id == trackingId).ToList();
            var details = query.ToList();
            return details;
        }

        public virtual List<OrderTrack> GetOrderTrackInformationByOrderId(int OrderId)
        {
            var query = _ordertrackRepository.Table.Where(x => x.OriginalOrderId == OrderId).ToList();
            var details = query.ToList();
            return details;
        }
    }
}
