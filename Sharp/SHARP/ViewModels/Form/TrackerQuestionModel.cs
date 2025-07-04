namespace SHARP.ViewModels.Form
{
    public class TrackerQuestionModel
    {
        public int Id { get; set; }

        public string Question { get; set; }

        public int? Sequence { get; set; }

        public TrackerOptionModel TrackerOption { get; set; }
    }
}
