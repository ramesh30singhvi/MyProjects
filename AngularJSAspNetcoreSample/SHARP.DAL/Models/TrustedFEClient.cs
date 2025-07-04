namespace SHARP.DAL.Models
{
    public class TrustedFEClient
    {
        public int ID { get; set; }

        public string UserAgent { get; set; }

        public string IP { get; set; }
        public int UserID { get; set; }
        public User User { get; set; }
    }
}
