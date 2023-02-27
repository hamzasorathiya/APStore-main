using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Core.Configuration;
using Nop.Web.Framework;

namespace Nop.Plugin.Misc.Picker.Models
{
    public class PickerMiscModel
    {
        [NopResourceDisplayName("Plugins.Misc.Picker.Fields.auth_Token")]
        public string auth_Token { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Picker.Fields.PlaceOrderUrl")]
        public string PlaceOrderUrl { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Picker.Fields.CancelOrderUrl")]
        public string CancelOrderUrl { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Picker.Fields.TrackOrderUrl")]
        public string TrackOrderUrl { get; set; }
    }
}