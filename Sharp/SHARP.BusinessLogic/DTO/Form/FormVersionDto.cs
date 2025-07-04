using SHARP.BusinessLogic.DTO.User;
using SHARP.Common.Enums;
using System;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Form
{
    public class FormVersionDto
    {
        public int Id { get; set; }

        public int Version { get; set; }

        public FormOptionDto Form { get; set; }

        //public OptionDto AuditType { get; set; }

        public FormVersionStatus Status { get; set; }

        public DateTime? CreatedDate { get; set; }

        public OptionDto Organization { get; set; }

        public IEnumerable<OptionDto> Organizations { get; set; }

        public UserOptionDto CreatedBy { get; set; }
    }
}
