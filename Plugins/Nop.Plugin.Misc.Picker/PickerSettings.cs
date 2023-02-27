using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.Picker
{
    public class PickerSettings : ISettings
    {
        public string auth_token { get; set; }

        public string PlaceOrderUrl { get; set; }

        public string CancelOrderUrl { get; set; }

        public string TrackOrderUrl { get; set; }
    }
}