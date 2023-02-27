//------------------------------------------------------------------------------
// Contributor(s): mb, New York. 
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Web.Routing;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Tasks;
using Nop.Core.Plugins;
using Nop.Plugin.Misc.Picker.Data;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Tasks;

namespace Nop.Plugin.Misc.Picker
{
    /// <summary>
    /// Picker computation method
    /// </summary>
    public class PickerComputationMethod : BasePlugin, IWidgetPlugin
    {
        private readonly ISettingService _settingService;
        private readonly PickerSettings _pickerSettings;
        private readonly PickerObjectContext _pickerobjectcontext;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly WidgetSettings _widgetSettings;

        public PickerComputationMethod(
            ISettingService settingService,
            PickerSettings pickerSettings,
            IScheduleTaskService scheduleTaskService,
            PickerObjectContext _pickerobjectcontext,
            WidgetSettings widgetSettings)
        {
            this._settingService = settingService;
            this._scheduleTaskService = scheduleTaskService;
            this._pickerSettings = pickerSettings;
            this._pickerobjectcontext = _pickerobjectcontext;
            this._widgetSettings = widgetSettings;
        }


        public IList<string> GetWidgetZones()
        {
            return new List<string> { "orderdetails_page_beforeproducts", "admin_header_after" };
        }

        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "";
            controllerName = "";
            routeValues = new RouteValueDictionary()
            {
                {"namespace","Nop.Plugin.Misc.Picker.Controllers"},
                { "area", null},
                {"widgetZone", ""}
            };

            if (widgetZone == "orderdetails_page_beforeproducts")
            {
                actionName = "PublicInfo";
                controllerName = "MiscPicker";
                routeValues = new RouteValueDictionary
                {
                    {"Namespaces", "Nop.Plugin.Misc.Picker.Controllers"},
                    {"area", null},
                    {"widgetZone", widgetZone}
                };
            }
            if (widgetZone == "admin_header_after")
            {
                actionName = "Orderplaceonpicker";
                controllerName = "MiscPicker";
                routeValues = new RouteValueDictionary
                {
                    {"Namespaces", "Nop.Plugin.Misc.Picker.Controllers"},
                    {"area", null},
                    {"widgetZone", widgetZone}
                };
            }

        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new PickerSettings
            {
                auth_token = _pickerSettings.auth_token,
                PlaceOrderUrl = _pickerSettings.PlaceOrderUrl,
                CancelOrderUrl = _pickerSettings.CancelOrderUrl,
                TrackOrderUrl = _pickerSettings.TrackOrderUrl
            };

            //install synchronization task
            if (_scheduleTaskService.GetTaskByType(SendinBlueDefaults.SynchronizationTask) == null)
            {
                _scheduleTaskService.InsertTask(new ScheduleTask
                {
                    Enabled = true,
                    Seconds = SendinBlueDefaults.DefaultSynchronizationPeriod * 25,
                    Name = SendinBlueDefaults.SynchronizationTaskName,
                    Type = SendinBlueDefaults.SynchronizationTask,
                });
            }

            _settingService.SaveSetting(settings);
            _widgetSettings.ActiveWidgetSystemNames.Add("Misc.Picker");
            _settingService.SaveSetting(_widgetSettings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.Picker.Fields.auth_token", "Authentication Token");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.Picker.Fields.auth_token.Hint", "Specify pickrr Token.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.Picker.Fields.PlaceOrderUrl", "PlaceOrderUrl");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.Picker.Fields.PlaceOrderUrl.Hint", "Specify Pickrr PlaceOrder API URL.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.Picker.Fields.CancelOrderUrl", "CancelOrderUrl");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.Picker.Fields.CancelOrderUrl.Hint", "Specify Pickrr CancelOrder API URL.");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.Picker.Fields.TrackOrderUrl", "TrackOrderUrl");
            this.AddOrUpdatePluginLocaleResource("Plugins.Misc.Picker.Fields.TrackOrderUrl.Hint", "Specify Pickrr TrackOrder API URL.");

            _pickerobjectcontext.Install();

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
            this.DeletePluginLocaleResource("Plugins.Misc.Picker.Fields.auth_token");
            this.DeletePluginLocaleResource("Plugins.Misc.Picker.Fields.auth_token.Hint");
            this.DeletePluginLocaleResource("Plugins.Misc.Picker.Fields.PlaceOrderUrl");
            this.DeletePluginLocaleResource("Plugins.Misc.Picker.Fields.PlaceOrderUrl.Hint");
            this.DeletePluginLocaleResource("Plugins.Misc.Picker.Fields.CancelOrderUrl");
            this.DeletePluginLocaleResource("Plugins.Misc.Picker.Fields.CancelOrderUrl.Hint");
            this.DeletePluginLocaleResource("Plugins.Misc.Picker.Fields.TrackOrderUrl");
            this.DeletePluginLocaleResource("Plugins.Misc.Picker.Fields.TrackOrderUrl.Hint");

            _pickerobjectcontext.Uninstall();

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