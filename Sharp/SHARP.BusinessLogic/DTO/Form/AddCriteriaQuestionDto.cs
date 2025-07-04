using SHARP.BusinessLogic.DTO.Audit;

namespace SHARP.BusinessLogic.DTO.Form
{
    public class AddCriteriaQuestionDto : CriteriaOptionDto
    {
        public int FormVersionId { get; set; }

        public string Question { get; set; }

        public GroupDto Group { get; set; }

        public int? ParentId { get; set; }
    }
}
