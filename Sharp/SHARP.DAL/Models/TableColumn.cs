using SHARP.Common.Constants;
using System;
using System.Collections.Generic;

namespace SHARP.DAL.Models
{
    public class TableColumn : IIdModel<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Sequence { get; set; }

        public int FormVersionId { get; set; }

        public int? GroupId { get; set; }

        public int? LegacyFormRowId { get; set; }

        public int? LegacyRowId { get; set; }

        public int? LegacyFormColumnId { get; set; }

        public int? ParentId { get; set; }

        public int? Hidden { get; set; }


        public FormVersion FormVersion { get; set; }

        public TableColumnGroup Group { get; set; }

        public TableColumn Parent { get; set; }

        public CriteriaOption CriteriaOption { get; set; }

        public TrackerOption TrackerOption { get; set; }

        public ICollection<TableColumn> SubQuestions { get; set; }

        public IReadOnlyCollection<AuditTableColumnValue> TableColumnValues { get; set; }

        public ICollection<KeywordTrigger> KeywordTrigger { get; set; }

       
        public TableColumn() { }

        public TableColumn(string name, int sequence, FormVersion formVersion, TableColumnGroup group, int? parentId, TableColumn parent, CriteriaOption criteriaOption, TrackerOption trackerOption, int? hidden)
        {
            Name = name;
            Sequence = sequence;
            FormVersion = formVersion;
            Group = group;
            ParentId = parentId;
            Parent = parent;
            Hidden = hidden;

            CriteriaOption = criteriaOption;
            TrackerOption = trackerOption;

            SubQuestions = new List<TableColumn>();
            TableColumnValues = new List<AuditTableColumnValue>();

            KeywordTrigger = new List<KeywordTrigger>();


        }

        public TableColumn Clone(FormVersion formVersion, TableColumnGroup formGroup = null)
        {
            var criteriaOption = CriteriaOption != null ? CriteriaOption.Clone() : null;

            var trackerOption = TrackerOption != null ? TrackerOption.Clone() : null;
           
            return new TableColumn(Name, Sequence, formVersion, formGroup, ParentId, Parent, criteriaOption, trackerOption, Hidden);
        }
    }
}
