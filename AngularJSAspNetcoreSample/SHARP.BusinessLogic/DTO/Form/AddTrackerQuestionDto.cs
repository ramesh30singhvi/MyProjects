namespace SHARP.BusinessLogic.DTO.Form
{
    public class AddTrackerQuestionDto : TrackerOptionDto
    {
        public int FormVersionId { get; set; }

        public int? FormGroupId { get; set; }

        public string Question { get; set; }

        public int FieldTypeId { get; set; }
    }
}
