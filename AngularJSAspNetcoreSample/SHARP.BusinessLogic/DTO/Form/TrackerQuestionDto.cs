namespace SHARP.BusinessLogic.DTO.Form
{
    public class TrackerQuestionDto
    {
        public int Id { get; set; }

        public string Question { get; set; }

        public int? Sequence { get; set; }

        public TrackerOptionDto TrackerOption { get; set; }
    }
}
