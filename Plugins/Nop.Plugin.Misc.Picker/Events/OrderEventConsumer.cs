using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Plugins;
using Nop.Services.Events;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.Picker.Events
{
    //public class OrderEventConsumer : IConsumer<OrderPlacedEvent>
    //{
    //    private readonly IPluginFinder _pluginFinder;
    //    private readonly IOrderService _orderService;
    //    private readonly IStoreContext _storeContext;
    //    public OrderEventConsumer(
    //        IPluginFinder pluginFinder,
    //        IOrderService orderService,
    //        IStoreContext storeContext)
    //    {
    //        this._pluginFinder = pluginFinder;
    //        this._orderService = orderService;
    //        this._storeContext = storeContext;
    //    }

    //    /// <summary>
    //    /// Handles the event.
    //    /// </summary>
    //    /// <param name="eventMessage">The event message.</param>
    //    ///    [HubMethodName("liveNotification")]
    //    ///    
    //    public void HandleEvent(OrderPlacedEvent eventMessage)
    //    {
    //        var order = eventMessage.Order;
    //        var products = order.OrderItems;
    //    }
    //}
}
