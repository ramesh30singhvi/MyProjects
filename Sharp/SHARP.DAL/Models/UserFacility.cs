namespace SHARP.DAL.Models
{
    public class UserFacility
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int FacilityId { get; set; }

        public User User { get; set; }

        public Facility Facility { get; set; }
    }
}
