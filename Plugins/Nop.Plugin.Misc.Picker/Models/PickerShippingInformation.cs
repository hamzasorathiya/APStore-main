using Nop.Core;

namespace Nop.Plugin.Misc.Picker.Models
{
    public partial class PickerShippingInformation : BaseEntity
    {
        public string tracking_id { get; set; }

        public string status_name { get; set; }

        public string status_body { get; set; }

        public string status_time { get; set; }

        public string status_location { get; set; }

        public string current_status_body { get; set; }

        public string current_status_type { get; set; }

        public string current_status_location { get; set; }

        public string to_city { get; set; }

        public string to_pincode { get; set; }

        public string from_pincode { get; set; }

        public string from_city { get; set; }

        //public int ShippingStatusId { get; set; }

        /// <summary>
        /// Gets or sets the Shipping Status
        /// </summary>
        //public string shippingstatus { get; set; }
    }
}
