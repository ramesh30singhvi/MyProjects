using System;
using System.Collections.Generic;

namespace SHARP.ViewModels.User
{
	public class UserFacilitiesModel
	{
        public IReadOnlyCollection<OptionModel> Facilities { get; set; }

        public int? FilteredByUserId { get; set; }
    }
}

