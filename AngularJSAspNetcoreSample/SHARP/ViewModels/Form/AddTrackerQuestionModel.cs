namespace SHARP.ViewModels.Form
{
    public class AddTrackerQuestionModel : TrackerOptionModel
    {
        public int FormVersionId { get; set; }

        public int? FormGroupId { get; set; }

        public string Question { get; set; }

        public int FieldTypeId { get; set; }
    }
}
