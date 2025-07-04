namespace SHARP.DAL.Models
{
    public class UserOrganization
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int OrganizationId { get; set; }

        public User User { get; set; }

        public Organization Organization { get; set; }
    }
}
