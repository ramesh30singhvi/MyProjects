using SHARP.BusinessLogic.DTO;

namespace SHARP.ViewModels.Audit
{
    public class AuditTrackerAnswerModel
    {
        public int? Id { get; set; }

        public int AuditId { get; set; }

        public int QuestionId { get; set; }

        public string Answer { get; set; }

        public string FormattedAnswer { get; set; }

    }
}
