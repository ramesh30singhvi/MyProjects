using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.SendGridEvents.Models
{
    public class OpenEvent : Event
    {
        public string UserAgent { get; set; }

        public string IP { get; set; }
    }
}
