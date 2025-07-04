using SHARP.Common.Constants;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Form
{
    public class EditQuestionGroupsDto
    {
        public IReadOnlyCollection<EditQuestionGroupDto> Sections { get; set; }
    }

    public class EditQuestionGroupDto : IIdModel<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Sequence { get; set; }
    }
}
