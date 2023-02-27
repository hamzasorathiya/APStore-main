using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.Picker.Models
{

    public class Rootobject
    {
        public object err { get; set; }
        public bool success { get; set; }
        public string order_id { get; set; }
        public string courier { get; set; }
        public string dispatch_mode { get; set; }
        public int courier_id { get; set; }
        public Courier_Allocation courier_allocation { get; set; }
        public string tracking_id { get; set; }
        public string manifest_img_link_v2 { get; set; }
        public string manifest_link_pdf { get; set; }
        public string edd_stamp { get; set; }
        public string routing_code { get; set; }
        public string manifest_img_link { get; set; }
        public int order_pk { get; set; }
        public string track_url { get; set; }
        public string manifest_link { get; set; }
    }

    public class Courier_Allocation
    {
        public string selected_type { get; set; }
    }

}
