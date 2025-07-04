namespace SHARP.DAL.Models
{
    public class OrganizationRecipient
    {
        public int Id { get; set; }

        public string Recipient { get; set; }

        public int OrganizationId { get; set; }
    }
}
