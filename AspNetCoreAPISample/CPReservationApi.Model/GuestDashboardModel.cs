using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class GuestDashboardModel
    {
        public RSVPSummary summary { get; set; }

        public List<ReservationData> reservation { get; set; }

        public List<RSVPGraph> reservation_graph { get; set; }
        public List<RevenueData> revenue { get; set; }
        //public List<RevenueGraph> revenue_graph { get; set; }

        public List<RevenueGraph> revenue_reservation_graph { get; set; }

        public List<RevenueGraph> revenue_ticket_graph { get; set; }
    }

    public class ReservationData
    {
        public string title { get; set; }
        public int count { get; set; }
        public decimal total { get; set; }
        public string color { get; set; }
    }


    public class RevenueData
    {
        public string title { get; set; }
        public decimal count { get; set; }
        public decimal total { get; set; }

        public string color { get; set; }
    }

    public class RSVPGraph
    {
        public DateTime date { get; set; }
        public int value { get; set; }
    }

    public class RevenueGraph
    {
        public DateTime date { get; set; }
        public decimal value { get; set; }
    }

    public class RSVPSummary
    {
        public decimal reservation_increase_percentage { get; set; }

        public bool reservation_total_has_increased { get; set; }

        public int[] reservation_history { get; set; }

        public int ticket_total { get; set; }

        public int[] ticket_history { get; set; }

        public decimal revenue_total { get; set; }
        public decimal[] revenue_history { get; set; }

        public decimal new_guest_percentage { get; set; }

        public decimal new_guest_increase { get; set; }
        public int region_ranking { get; set; }

        public decimal region_ranking_increase { get; set; }

        public int experience_review_total { get; set; }

        public decimal experience_review_increase { get; set; }

        public int favorite_total { get; set; }

        public decimal favorite_increase { get; set; }

        public int total_guests_checked_in { get; set; } = 0;

        public int total_guests_seated { get; set; } = 0;

        public int total_guests_rsvp { get; set; } = 0;

        public int total_guests_walkins { get; set; } = 0;

        public int total_guests_not_checked_in { get; set; } = 0;
    }


    public class DashboardMetrics
    {
        public DateTime booking_date { get; set; }
        public int rsvp_count { get; set; }
        public decimal rsvp_revenue { get; set; }
        public int ticket_count { get; set; }
        public decimal ticket_revenue { get; set; }
        public int rsvp_guest_count { get; set; }
    }
}
