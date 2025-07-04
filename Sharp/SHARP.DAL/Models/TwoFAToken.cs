using System;

namespace SHARP.DAL.Models
{
    public class TwoFAToken
    {
        public string Id { get; set; }

        public string Token { get; set; }

        public DateTime CreatedAt { get; set; }

        public ApplicationUser User { get; set; }
    }
}
