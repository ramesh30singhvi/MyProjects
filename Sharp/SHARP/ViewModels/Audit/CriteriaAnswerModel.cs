namespace SHARP.ViewModels.Audit
{
    public class CriteriaAnswerModel
    {
        public int Id { get; set; }

        public int TableColumnId { get; set; }

        public string Value { get; set; }

        public string AuditorComment { get; set; }
    }
}
