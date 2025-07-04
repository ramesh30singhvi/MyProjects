using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class WineryReceiptSetting
    {
        public int member_id { get; set; }
        public bool show_logo { get; set; }
        public bool show_location_address { get; set; }
        public bool show_location_phone_number { get; set; }
        public bool show_customer_information { get; set; }
        public bool show_order_note { get; set; }
        public bool show_barcode { get; set; }
        public bool show_sold_by { get; set; }
        public string header_text { get; set; }
        public string footer_text { get; set; }
        public bool collect_signature { get; set; }
        public bool has_minimum_amount_signature_requirement { get; set; }
        public Common.SignatureCaptureType signature_capture_type { get; set; }
        public bool show_add_tip_line { get; set; }
        public bool add_tip { get; set; }
    }

    public class WineryReceiptSettingsWithLogo : WineryReceiptSetting
    {
        public string logo_url { get; set; }
    }

    public class WIneryReceiptResponseModel
    {
        public int member_id { get; set; }
    }

    public class UploadReceiptLogoResponseModel
    {
        public string logo_url { get; set; }
    }

    public class OrderSignatureResponseModel
    {
        public Guid signature_guid { get; set; }
    }
}
