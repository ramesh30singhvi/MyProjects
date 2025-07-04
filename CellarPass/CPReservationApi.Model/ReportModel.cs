using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class ReportModel
    {
        public int id { get; set; }
        public int? server_id { get; set; }
        public string guest_first_name { get; set; }
        public string guest_last_name { get; set; }
        public int party_size { get; set; }
        public string server_color { get; set; }
        //public double seating_reset_time { get; set; }
        public double start_time { get; set; }
        public double? end_time { get; set; }
        public int index { get; set; }
        public int number_seated { get; set; } = 0;
    }

    public class ReportModelV3
    {
        public int id { get; set; }
        public int? server_id { get; set; }
        public string guest_first_name { get; set; }
        public string guest_last_name { get; set; }
        public int party_size { get; set; }
        public string server_color { get; set; }
        public DateTime start_time { get; set; }
        public DateTime? end_time { get; set; }
        public int index { get; set; }
        public int number_seated { get; set; } = 0;
        public bool is_walk_in { get; set; } = false;
    }

    public class CoversReport
    {
        public CoversReport()
        {
            rsvps = new List<ReportModel>();
            waitlists = new List<ReportModel>();
        }
        public int table_id { get; set; }
        public string table_name { get; set; }
        public List<ReportModel> rsvps { get; set; }
        public List<ReportModel> pre_assign_rsvps { get; set; }
        public List<ReportModel> waitlists { get; set; }
        public List<ReportModel> pre_assign_waitlists { get; set; }
    }

    public class CoversReportV3
    {
        public CoversReportV3()
        {
            rsvps = new List<ReportModelV3>();
            waitlists = new List<ReportModelV3>();
        }
        public int table_id { get; set; }
        public string table_name { get; set; }
        public List<ReportModelV3> rsvps { get; set; }
        public List<ReportModelV3> pre_assign_rsvps { get; set; }
        public List<ReportModelV3> waitlists { get; set; }
        public List<ReportModelV3> pre_assign_waitlists { get; set; }
    }

    public class CoversReportLocations
    {
        public CoversReportLocations()
        {
            tables = new List<CoversReport>();
        }
        public int location_id { get; set; }
        public string location_name { get; set; }
        public double seating_reset_time { get; set; }
        public List<CoversReport> tables { get; set; }
    }

    public class CoversReportLocationsV2
    {
        public CoversReportLocationsV2()
        {
            tables = new List<CoversReport>();
        }
        public int floor_plan_id { get; set; }
        public string floor_plan_name { get; set; }
        public double seating_reset_time { get; set; }
        public List<CoversReport> tables { get; set; }
    }

    public class CoversReportLocationsV3
    {
        public CoversReportLocationsV3()
        {
            tables = new List<CoversReportV3>();
        }
        public int floor_plan_id { get; set; }
        public string floor_plan_name { get; set; }
        public double seating_reset_time { get; set; }
        public int offset_minutes { get; set; }
        public List<CoversReportV3> tables { get; set; }
    }

    public class ShiftReport
    {
        public ShiftReport()
        {
            summary = new List<ShiftReportSummary>();
        }

        public int user_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        //public int total_guests { get; set; }
        public string color { get; set; }
        public List<ShiftReportSummary> summary { get; set; }
    }

    public class ShiftReportSummary
    {
        public string title { get; set; }
        public int guest_count { get; set; }
    }

    public class DailyReport
    {
        public DailyReport()
        {
            notes = new List<CalendarModel>();
            events = new List<EventsModel>();
            guest_requests = new List<GuestRequestsModel>();
            special_events = new List<SpecialEventsModel>();
            special_guests = new List<SpecialGuestsModel>();
        }
        public int total_guest_count { get; set; }
        public int total_party_count { get; set; }
        public int total_seated_count { get; set; }
        public int total_seated_party_count { get; set; }    
        public int total_clubmember_guest_count { get; set; }
        public int total_reservation_guest_count { get; set; }
        public int total_walkin_guest_number { get; set; }
        public int total_waitlist_guest_count { get; set; }
        public List<CalendarModel> notes { get; set; }
        public List<EventsModel> events { get; set; }
        public List<GuestRequestsModel> guest_requests { get; set; }
        public List<SpecialEventsModel> special_events { get; set; }
        public List<SpecialGuestsModel> special_guests { get; set; }
    }

    public class EventsModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public int guest_count { get; set; }
        public int party_count { get; set; }
        public string first_arrival_first_name { get; set; }
        public string first_arrival_last_name { get; set; }
        public DateTime event_start_time { get; set; }
        public int customer_type { get; set; }
    }

    public class GuestRequestsModel
    {
        public int id { get; set; }
        public int reservation_id { get; set; }
        public int event_id { get; set; }
        public DateTime event_start_time { get; set; }
        public string guest_first_name { get; set; }
        public string guest_last_name { get; set; }
        public string guest_notes { get; set; }
    }

    public class SpecialEventsModel
    {
        public int id { get; set; }
        public int reservation_id { get; set; }
        public int event_id { get; set; }
        public DateTime event_start_time { get; set; }
        public string guest_first_name { get; set; }
        public string guest_last_name { get; set; }
        //public string guest_notes { get; set; }
        public List<GuestTagsModel> guest_tags { get; set; }
    }

    public class SpecialGuestsModel
    {
        public int id { get; set; }
        public int reservation_id { get; set; }
        public int event_id { get; set; }
        public DateTime event_start_time { get; set; }
        public string guest_first_name { get; set; }
        public string guest_last_name { get; set; }
        //public string guest_notes { get; set; }
        public List<GuestTagsModel> guest_tags { get; set; }
    }

    public class GuestTagsModel
    {
        public int id { get; set; }
        public int member_id { get; set; }
        public string tag { get; set; }
        public int tag_type { get; set; }
    }

    public class ExportReservationDetail
    {
        public string guest_name { get; set; }

        public string phone_number { get; set; }

        public int total_guests { get; set; }

        public decimal fee_per_person { get; set; }

        public string account_type { get; set; }

        public string referred_by { get; set; }

        public string internal_note { get; set; }

        public string guest_note { get; set; }

        public int max_persons { get; set; }

        public string event_name { get; set; }

        public string event_time { get; set; }

        public int guests { get; set; }

        public int sort_order { get; set; }

        public DateTime start { get; set; }

        public string location_name { get; set; }
    }

    public class ExportReservation
    {
        public string last_name { get; set; }
        public string first_name { get; set; }
        public string phone_number { get; set; }
        public int total_guests { get; set; }
        public decimal fee_per_person { get; set; }
        public decimal discount { get; set; }
        public decimal purchase_total { get; set; }
        public string account_type { get; set; }
        public string booking_date { get; set; }
        public string referred_by { get; set; }
        public string internal_note { get; set; }
        public int max_persons { get; set; }
        public string event_name { get; set; }
        public string event_time { get; set; }
        public int guests { get; set; }
        public int sort_order { get; set; }
        public DateTime start { get; set; }
        public string hdyh { get; set; }
    }
}
