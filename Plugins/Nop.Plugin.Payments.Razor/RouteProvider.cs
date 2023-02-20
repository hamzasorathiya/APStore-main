using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.Razor
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.Razor.Configure",
                 "Plugins/PaymentRazor/Configure",
                 new { controller = "Payment", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.Razor.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.Razor.PaymentInfo",
                 "Plugins/PaymentRazor/PaymentInfo",
                 new { controller = "Payment", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.Razor.Controllers" }
            );

            //Return
            routes.MapRoute("Plugin.Payments.Razor.Return",
                 "Plugins/Payment/Return",
                 new { controller = "Payment", action = "Complete" },
                 new[] { "Nop.Plugin.Payments.Razor.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.Razor.LoadRazorPay",
                 "Plugins/Payment/LoadRazorPay",
                 new { controller = "Payment", action = "LoadRazorPay" },
                 new[] { "Nop.Plugin.Payments.Razor.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.Razor.Success",
                 "Plugins/Payment/Success",
                 new { controller = "Payment", action = "Success" },
                 new[] { "Nop.Plugin.Payments.Razor.Controllers" }
            );
            routes.MapRoute("Plugin.Payments.Razor.Failed",
                 "Plugins/Payment/Failed",
                 new { controller = "Payment", action = "Failed" },
                 new[] { "Nop.Plugin.Payments.Razor.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
