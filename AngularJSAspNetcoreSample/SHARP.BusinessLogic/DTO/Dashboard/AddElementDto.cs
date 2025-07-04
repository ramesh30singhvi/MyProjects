using System;
namespace SHARP.BusinessLogic.DTO.Dashboard
{
	public class AddElementDto
	{
        public int OrganizationId { get; set; }
        public int GroupId { get; set; }
        public int FormId { get; set; }
        public String Name { get; set; }
        public String Keyword { get; set; }
    }
}

