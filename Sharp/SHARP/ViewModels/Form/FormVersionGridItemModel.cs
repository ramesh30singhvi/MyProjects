using SHARP.Common.Enums;
using System;
using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
    public class FormVersionGridItemModel
    {
        public int Id { get; set; }

        public int FormId { get; set; }

        public string Name { get; set; }

        public IEnumerable<string> Organizations { get; set; }

        public string AuditType { get; set; }

        public FormVersionStatus Status { get; set; }

        public bool IsFormActive { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
