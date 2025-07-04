using SHARP.Common.Enums;

namespace SHARP.BusinessLogic.DTO.Audit
{
    public class TrackerAnswerDto
    {
        public int? Id { get; set; }

        public int AuditId { get; set; }

        public int QuestionId { get; set; }

        public string Answer { get; set; }

        public FieldTypes FieldType { get; set; }

        public string FormattedAnswer { get; set; }

        public HighAlertValueDto HighAlertAuditValue { get; set; }


    }
}
