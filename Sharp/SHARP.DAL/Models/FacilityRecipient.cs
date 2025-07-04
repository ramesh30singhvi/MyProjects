namespace SHARP.DAL.Models
{
    public class FacilityRecipient
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public int FacilityId { get; set; }

        public Facility Facility { get; set; }
    }
}
