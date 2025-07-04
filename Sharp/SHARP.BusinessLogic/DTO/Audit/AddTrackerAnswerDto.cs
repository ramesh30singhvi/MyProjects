namespace SHARP.BusinessLogic.DTO.Audit
{
    public class AddEditTrackerAnswerDto: TrackerAnswerDto
    {
        public string GroupId { get; set; }

        public OptionDto HighAlertCategory { get; set; }

        public string HighAlertDescription { get; set; }

        public string HighAlertNotes { get; set; }
    }
}
