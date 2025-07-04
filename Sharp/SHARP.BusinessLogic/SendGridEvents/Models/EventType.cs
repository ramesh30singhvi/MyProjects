using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SHARP.BusinessLogic.SendGridEvents.Models
{
    public enum EventType
    {
        Processed,
        Deferred,
        Delivered,
        Open,
        Click,
        Bounce,
        Dropped,
        SpamReport,
        Unsubscribe,
        [EnumMember(Value = "group_unsubscribe")]
        GroupUnsubscribe,
        [EnumMember(Value = "group_resubscribe")]
        GroupResubscribe
    }
}
