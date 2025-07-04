namespace SHARP.BusinessLogic.DTO.Facility
{
    public class TimeZoneOptionDto : OptionDto
    {
        public int TimeZoneOffset { get; set; }

        public string TimeZoneShortName { get; set; }

        public string  OriginalTimeZoneName { get; set; }
    }
}
