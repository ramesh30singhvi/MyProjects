using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class RsvpConfirmationEmailTemplateModel
    {
        public int id { get; set; }
        public string email_name { get; set; }
        public bool system_default { get; set; }
        public bool is_default { get; set; } = false;
    }

    
}
