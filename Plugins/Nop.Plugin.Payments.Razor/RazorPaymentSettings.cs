using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Razor
{
    public class RazorPaymentSettings : ISettings
    {

        public string Key { get; set; }
        public string Secret { get; set; }
        public decimal AdditionalFee { get; set; }

    }
}
