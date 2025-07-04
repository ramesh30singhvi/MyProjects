using SHARP.Common.Enums;
using System;
using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
    public class FormVersionModel
    {
        public int Id { get; set; }

        public int Version { get; set; }

        public FormOptionModel Form { get; set; }

        public FormVersionStatus Status { get; set; }

        public DateTime? CreatedDate { get; set; }

        public OptionModel Organization { get; set; }

       

        public IReadOnlyCollection<OptionModel> Organizations { get; set; }

        public UserOptionModel CreatedBy { get; set; }
    }
}
