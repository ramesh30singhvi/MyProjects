namespace SHARP.BusinessLogic.DTO.Audit
{
    public class CriteriaAnswerDto
    {
        public int Id { get; set; }

        public int TableColumnId { get; set; }

        public string Value { get; set; }

        public string AuditorComment { get; set; }

        public CriteriaAnswerDto(int tableColumnId)
        {
            TableColumnId = tableColumnId;
        }
    }
}
