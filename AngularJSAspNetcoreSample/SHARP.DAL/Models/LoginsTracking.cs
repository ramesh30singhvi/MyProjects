using System;

namespace SHARP.DAL.Models
{
    public class LoginsTracking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Login { get; set; }
        public string Duration { get; set; }
        public DateTime? Logout { get; set; }

        public User User { get; set; }
    }
}
