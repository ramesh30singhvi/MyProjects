using System;
namespace SHARP.BusinessLogic.DTO.Dashboard
{
	public class SaveDashboardInputValuesDto
	{
		public int Id { get; set; }
		public int Value { get; set; }
		public int FacilityId { get; set; }
		public int ElementId { get; set; }
		public DateTime Date { get; set; }
	}
}

