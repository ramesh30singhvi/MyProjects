using System;

namespace SHARP.BusinessLogic.DTO.Dashboard
{
    public class DashboardInputFilterDto
    {
        public int organizationId { get; set; }
        public DateTime dateFrom {  get; set; }
        public DateTime dateTo { get; set; }
    }
}
