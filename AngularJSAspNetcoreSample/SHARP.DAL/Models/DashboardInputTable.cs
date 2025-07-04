using System;
using System.Collections.Generic;

namespace SHARP.DAL.Models
{
	public class DashboardInputTable
	{
		public int Id { get; set; }

		public string Name { get; set; }

        public int OrganizationId { get; set; }

        public Organization Organization { get; set; }

		public ICollection<DashboardInputGroups> DashboardInputGroups { get; set; }
		
	}
}

