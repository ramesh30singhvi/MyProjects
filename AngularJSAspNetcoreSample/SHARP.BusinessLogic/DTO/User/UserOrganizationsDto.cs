using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.User
{
    public class UserOrganizationsDto
    {
        public IReadOnlyCollection<OptionDto> Organizations { get; set; }

        public int? FilteredByUserId { get; set; }
    }
}
