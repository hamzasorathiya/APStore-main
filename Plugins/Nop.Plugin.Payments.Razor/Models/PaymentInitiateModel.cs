using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nop.Admin.Models.Settings;

namespace Nop.Plugin.Payments.Razor.Models
{
    public class PaymentInitiateModel
    {
        public string name { get; set; }
        public string email { get; set; }
        public string contactNumber { get; set; }
        public string address { get; set; }
        public decimal amount { get; set; }

        public string FormName { get; set; }
        //public string amount { get; set; }
        public string Currency { get; set; }
        public string Order_Id { get; set; }
        public string txnid { get; set; }
        public string surl { get; set; }
        public string furl { get; set; }

        public string hash { get; set; }

        //Billing details
        public string firstname { get; set; }
        public string billing_cust_address { get; set; }
        public string phone { get; set; }
        //public string email { get; set; }
        public string billing_cust_city { get; set; }
        public string billing_cust_state { get; set; }
        public string billing_zip_code { get; set; }
        public string billing_cust_country { get; set; }


        //Delivery details
        public string delivery_cust_name { get; set; }
        public string delivery_cust_address { get; set; }
        public string delivery_cust_notes { get; set; }
        public string delivery_cust_tel { get; set; }
        public string delivery_cust_city { get; set; }
        public string delivery_cust_state { get; set; }
        public string delivery_zip_code { get; set; }
        public string delivery_cust_country { get; set; }

        public string PreviewImageUrl { get; set; }
    }
}