using System;
using System.Collections.Generic;
using System.Text;
using uc = CPReservationApi.Common;

namespace CPReservationApi.Model
{
    public class SettingModel
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string Value { get; set; }
        public uc.Common.SettingGroup Group { get; set; }
        public uc.Common.SettingKey Key { get; set; }
    }

    public class SeatingSessionModel
    {
        public int Id { get; set; }
        public DateTime SessionDateTime { get; set; }
        public int LocationId { get; set; }
        public int TableId { get; set; }
        public int NumberSeated { get; set; }
        public int TransactionCategory { get; set; }
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public int FloorPlanId { get; set; }
    }

    public class WaitlistModel
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int LocationId { get; set; }
        public DateTime WaitStartDateTime { get; set; }
        public int WaitlistStatus { get; set; }
        public int PartySize { get; set; }
        public int UserId { get; set; }
        public string PreAssign_Table_Id { get; set; }

        public int ValidMinutes { get; set; }

        public int FloorPlanId { get; set; } = 0;
        public int AssignedFloorPlanId { get; set; } = 0;

        public DateTime WaitEndDateTime { get; set; }
    }

    public class RsvpModel
    {
        public int MemberId { get; set; }
        public int LocationId { get; set; }
        public DateTime EventDate { get; set; }
        public int SeatedStatus { get; set; }

        public int FloorPlanId { get; set; }

        public int AssignedFloorPlanId { get; set; }
    }

    public class TableLayoutModel
    {
        public int LocationId { get; set; }
        public int MinParty { get; set; }
        public int MaxParty { get; set; }
    }

    public class TableDetailModel
    {
        public int TableId { get; set; }
        public int MinParty { get; set; }
        public int MaxParty { get; set; }
    }
    public class TableAvailableModel
    {
        public int SessionId { get; set; }
        public int status { get; set; }
        public int TransactionId { get; set; }
        public int TransactionCategory { get; set; }
    }

    public class TableBlockedStatusModel
    {
        public int Id { get; set; }
        public DateTime? BlockStartDate { get; set; }
        public DateTime? BlockEndDate { get; set; }
    }

    public class TablePreassigendStatusModel
    {
        public int ReservationCount { get; set; } = 0;
        public int WaitlistCount { get; set; } = 0;

    }

}
