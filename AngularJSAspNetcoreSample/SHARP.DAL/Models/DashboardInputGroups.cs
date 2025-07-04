using System;
using System.Collections.Generic;

namespace SHARP.DAL.Models
{
	public class DashboardInputGroups
	{
        public int Id { get; set; }

        public int TableId { get; set; }

        public string Name { get; set; }

        public DashboardInputTable DashboardInputTable { get; set; }

        public ICollection<DashboardInputElement> DashboardInputElements { get; set; }
    }
}

