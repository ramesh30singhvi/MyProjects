using System.Collections.Generic;
using System.Linq;

namespace SHARP.DAL.Models
{
    public class TrackerOption
    {
        public int TableColumnId { get; set; }

        public int FieldTypeId { get; set; }

        public bool IsRequired { get; set; } = true;

        public bool Compliance { get; set; } = true;

        public bool Quality { get; set; }

        public bool Priority { get; set; }

        public TableColumn TableColumn { get; set; }

        public FieldType FieldType { get; set; }

        public ICollection<TableColumnItem> Items { get; set; }

        public TrackerOption() { }

        public TrackerOption(int fieldTypeId, FieldType fieldType, bool isRequired, bool compliance, bool quality, bool priority)
        {
            FieldTypeId = fieldTypeId;
            IsRequired = isRequired;
            Compliance = compliance;
            Quality = quality;
            Priority = priority;
            FieldType = fieldType;

            Items = new List<TableColumnItem>();
        }

        public TrackerOption Clone()
        {
            var trackerOption = new TrackerOption(FieldTypeId, FieldType, IsRequired, Compliance, Quality, Priority);

            if (Items != null && Items.Any())
            {
                Items.ToList().ForEach(item => {
                    var fieldItem = item.Clone(trackerOption);
                    trackerOption.Items.Add(fieldItem);
                });
            }

            return trackerOption;
        }
    }
}