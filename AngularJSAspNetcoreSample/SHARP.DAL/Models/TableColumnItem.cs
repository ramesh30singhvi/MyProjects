namespace SHARP.DAL.Models
{
    public class TableColumnItem
    {
        public int Id { get; set; }

        public int TableColumnId { get; set; }

        public string Value { get; set; }

        public int Sequence { get; set; }

        public TrackerOption TrackerOption { get; set; }

        public TableColumnItem() { }

        public TableColumnItem(string value, int sequence, TrackerOption trackerOption)
        {
            Value = value;
            Sequence = sequence;
            TrackerOption = trackerOption;
        }

        public TableColumnItem Clone(TrackerOption trackerOption)
        {
            return new TableColumnItem(Value, Sequence, trackerOption);
        }
    }
}
