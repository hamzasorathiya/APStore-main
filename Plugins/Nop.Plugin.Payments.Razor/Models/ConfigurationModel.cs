using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Payments.Razor.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Razor.Key")]
        public string Key { get; set; }
        public bool Key_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Razor.Secret")]
        public string Secret { get; set; }
        public bool Secret_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payment.Razor.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }
    }
}