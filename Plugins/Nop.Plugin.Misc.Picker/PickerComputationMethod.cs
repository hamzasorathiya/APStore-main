//------------------------------------------------------------------------------
// Contributor(s): mb, New York. 
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Routing;
using System.Web.Services.Protocols;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Shipping;
using Nop.Core.Plugins;
using Nop.Services.Common;
//using Nop.Plugin.Misc.Picker.Domain;
//using Nop.Plugin.Misc.Picker.RateServiceWebReference;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;

namespace Nop.Plugin.Misc.Picker
{
    /// <summary>
    /// Fedex computation method
    /// </summary>
    public class PickerComputationMethod : BasePlugin, IMiscPlugin
    {
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        private readonly PickerSettings _pickerSettings;

        public PickerComputationMethod(
        ISettingService settingService,
            ILogger logger, PickerSettings pickerSettings)
        {
            this._settingService = settingService;
            this._logger = logger;
            this._pickerSettings = pickerSettings;
        }


        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new PickerSettings
            {
                Url = "https://gatewaybeta.fedex.com:443/web-services/rate",
                auth_token = ""
            };
            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Picker.Fields.Url", "URL");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Picker.Fields.Url.Hint", "Specify Pickrr URL.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Picker.Fields.auth_token", "Token");
            this.AddOrUpdatePluginLocaleResource("Plugins.Shipping.Picker.Fields.auth_token.Hint", "Specify pickrr Token.");

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<PickerSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Shipping.Picker.Fields.Url");
            this.DeletePluginLocaleResource("Plugins.Shipping.Picker.Fields.Url.Hint");
            this.DeletePluginLocaleResource("Plugins.Shipping.Picker.Fields.auth_token");
            this.DeletePluginLocaleResource("Plugins.Shipping.Picker.Fields.auth_token.Hint");


            base.Uninstall();
        }
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "MiscPicker";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Misc.Picker.Controllers" }, { "area", null } };
        }
    }
}