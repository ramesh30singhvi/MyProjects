namespace SHARP.ViewModels.Form
{
    public class AddCriteriaQuestionModel : CriteriaOptionModel
    {
        public int FormVersionId { get; set; }

        public string Question { get; set; }

        public int? GroupId { get; set; }

        public string GroupName { get; set; }

        public int? ParentId { get; set; }
    }
}
