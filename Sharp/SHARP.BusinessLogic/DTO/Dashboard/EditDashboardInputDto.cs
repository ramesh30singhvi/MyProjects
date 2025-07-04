using System;
namespace SHARP.BusinessLogic.DTO.Dashboard
{
	public class EditDashboardInputDto
	{
        public int OrganizationId { get; set; }
        public int Id { get; set; }
        public int FormId { get; set; }
		public String Name { get; set; }
        public String Keyword { get; set; }
    }
}

