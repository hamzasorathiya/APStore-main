
namespace Nop.Plugin.Misc.Picker.Models
{
    public class Shippingdata
    {
        public Status status { get; set; }
        public Info info { get; set; }
        public Track_Arr[] track_arr { get; set; }
        public string tracking_id { get; set; }
    }

    public class Status
    {
        public string current_status_type { get; set; }
        public string current_status_body { get; set; }
        public string current_status_location { get; set; }
    }

    public class Info
    {
        public string to_pincode { get; set; }
        public string to_city { get; set; }
        public string from_city { get; set; }
        public string from_pincode { get; set; }
    }

    public class Track_Arr
    {
        public string status_name { get; set; }
        public Status_Array[] status_array { get; set; }
    }

    public class Status_Array
    {
        public string status_body { get; set; }

        public string status_time { get; set; }

        public string status_location { get; set; }
    }
}
