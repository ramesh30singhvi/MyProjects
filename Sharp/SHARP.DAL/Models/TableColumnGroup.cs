using SHARP.Common.Constants;
using System;
using System.Collections.Generic;

namespace SHARP.DAL.Models
{
    public class TableColumnGroup : IIdModel<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? FormVersionId { get; set; }

        public int Sequence { get; set; }

        public FormVersion FormVersion { get; set; }

        public IReadOnlyCollection<TableColumn> TableColumns { get; set; }

        public TableColumnGroup() { }

        public TableColumnGroup(string name, FormVersion formVersion, int sequence)
        {
            Name = name;
            FormVersion = formVersion;
            Sequence = sequence;

            TableColumns = new List<TableColumn>();
        }

        public TableColumnGroup Clone(FormVersion formVersion)
        {
            return new TableColumnGroup(Name, formVersion, Sequence);
        }
    }
}
