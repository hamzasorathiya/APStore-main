using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.Picker.Models
{
    public partial class ShippingInformation
    {
        public List<statusinfo> statusinfo { get; set; }
    }

    public class statusinfo
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
    }
}
