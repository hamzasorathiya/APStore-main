using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.Picker
{
    public class PickerSettings : ISettings
    {
        public string Url { get; set; }
        public string auth_token { get; set; }

    }
}