using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class ZoomToken
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public DateTime Expires { get; set; }
        public string Scope { get; set; }
    }


    public class RequestAccessTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string refresh_token { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
    }

    public class ZoomMeetingInfo
    {
        public int Id { get; set; }
        public long MeetingId { get; set; }
        public int ReservationId { get; set; }
        public string MeetingURL { get; set; }
        public string RegistrantId { get; set; }
        public int MeetingBehavior { get; set; }
        public int SlotId { get; set; }
        public int SlotType { get; set; }
        public DateTime StartDate { get; set; }
    }
}
