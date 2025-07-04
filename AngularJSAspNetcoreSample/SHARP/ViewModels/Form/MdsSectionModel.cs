using System;
using System.Collections.Generic;

namespace SHARP.ViewModels.Form
{
	public class MdsSectionModel
	{
        public int Id { get; set; }

        public string Name { get; set; }

        public IReadOnlyCollection<MdsGroupModel> Groups { get; set; }
    }
}

