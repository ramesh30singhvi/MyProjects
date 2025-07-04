using System;
using System.Collections.Generic;
using System.Text;
using static CPReservationApi.Common.Common;

namespace CPReservationApi.Model
{
    public class CustomSetting
    {
        public int id { get; set; }
        public int member_id { get; set; }
        public string member_name { get; set; }
        public SettingType custom_setting_id { get; set; }
        public string custom_setting_name { get; set; }
        public string label_1 { get; set; }
        public string value_1 { get; set; }
        public string label_2 { get; set; }
        public string value_2 { get; set; }
        public string label_3 { get; set; }
        public string value_3 { get; set; }
        public string label_4 { get; set; }
        public string value_4 { get; set; }
        public string label_5 { get; set; }
        public string value_5 { get; set; }
    }

}
