using SHARP.Common.Constants;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Common
{
    public class RearrangeItemsDto
    {
        public IReadOnlyCollection<RearrangeDto> Items { get; set; }
    }

    public class RearrangeDto : IIdModel<int>
    {
        public int Id { get; set; }

        public int Sequence { get; set; }
    }
}
