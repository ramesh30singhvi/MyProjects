using System;
using SHARP.DAL.Models;
using System.Collections.Generic;

namespace SHARP.BusinessLogic.DTO.Dashboard
{
	public class DashboardInputDto: OptionDto
    {
		public DashboardInputTableDto[] DashboardInputTables { get; set; }
        public OptionDto[] Facilities { get; set; }
        public DashboardInputSummary[] DashboardInputSummaries { get; set; }
    }

	public class DashboardInputTableDto: OptionDto
    {
		public DashboardInputGroupsDto[] DashboardInputGroups { get; set; }
    }

	public class DashboardInputGroupsDto: OptionDto
	{
        public DashboardInputElementsDto[] DashboardInputElements { get; set; }
    }

    public class DashboardInputElementsDto : OptionDto
    {
        public DashboardInputValuesDto[] DashboardInputValues { get; set; }
        public int? FormId { get; set; }
    }

    public class DashboardInputValuesDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Value { get; set; }
        public OptionDto Facility { get; set; }
        public int ElementId { get; set; }
        public int FacilityId { get; set; }
    }
}

