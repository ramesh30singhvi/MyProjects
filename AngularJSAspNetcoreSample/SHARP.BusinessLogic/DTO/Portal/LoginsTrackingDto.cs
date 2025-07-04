using System;

namespace SHARP.BusinessLogic.DTO.Portal
{
    public class LoginsTrackingDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Login { get; set; }
        public string Duration { get; set; }
        public DateTime? Logout { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
    }
}
