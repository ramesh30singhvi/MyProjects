using System.Collections.Generic;

namespace SHARP.ViewModels.User
{
    public class UserOrganizationsModel
    {
        public IReadOnlyCollection<OptionModel>  Organizations { get; set; }

        public int? FilteredByUserId { get; set; }
    }
}
