using System;
using System.Collections.Generic;

namespace SHARP.DAL.Models
{
	public class DashboardInputElement
	{
        public int Id { get; set; }

        public int GroupId { get; set; }

        public int? FormId { get; set; }

        public Form Form { get; set; }

        public string Name { get; set; }

        public string Keyword { get; set; }

        public DashboardInputGroups DashboardInputGroups { get; set; }

        public ICollection<DashboardInputValues> DashboardInputValues { get; set; }

    }
}

