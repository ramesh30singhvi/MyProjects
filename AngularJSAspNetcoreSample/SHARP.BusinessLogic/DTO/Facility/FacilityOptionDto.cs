namespace SHARP.BusinessLogic.DTO.Facility
{
    public class FacilityOptionDto : OptionDto
    {
        public int TimeZoneOffset { get; set; }

        public string TimeZoneShortName { get; set; }

        public int OrganizationId { get; set; }

        public string LegalName { get; set; }
    }
}
