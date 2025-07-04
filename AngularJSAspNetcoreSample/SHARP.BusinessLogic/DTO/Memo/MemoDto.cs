using SHARP.BusinessLogic.DTO.User;
using System;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Memo
{
    public class MemoDto
    {
        public int Id { get; set; }

        public UserOptionDto User { get; set; }

        public IReadOnlyCollection<OptionDto> Organizations { get; set; }

        public string Text { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ValidityDate { get; set; }
    }
}
