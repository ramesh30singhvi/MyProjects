namespace SHARP.BusinessLogic.DTO.Facility
{
    public class FacilityDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string OrganizationName { get; set; }

        public string TimeZoneName { get; set; }

        public string OriginalTimeZoneName { get; set; }

        public int RecipientsCount { get; set; }

        public bool Active { get; set; }
    }
}
