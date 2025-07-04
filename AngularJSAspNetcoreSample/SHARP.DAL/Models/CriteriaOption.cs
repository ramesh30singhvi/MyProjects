namespace SHARP.DAL.Models
{
    public class CriteriaOption
    {
        public int TableColumnId { get; set; }

        public bool ShowNA { get; set; } = true;

        public bool Compliance { get; set; } = true;

        public bool Quality { get; set; }

        public bool Priority { get; set; }

        public TableColumn TableColumn { get; set; }

        public CriteriaOption() { }

        public CriteriaOption(bool showNA, bool compliance, bool quality, bool priority)
        {
            ShowNA = showNA;
            Compliance = compliance;
            Quality = quality;
            Priority = priority;
        }

        public CriteriaOption Clone()
        {
            return new CriteriaOption(ShowNA, Compliance, Quality, Priority);
        }
    }
}