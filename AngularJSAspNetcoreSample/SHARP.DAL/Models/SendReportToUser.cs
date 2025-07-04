using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Models
{
    public class SendReportToUser
    {
        public int Id { get; set; }

        public int PortalReportId { get; set; }

        public PortalReport PortalReport { get; set; }

        public int UserId { get; set; }

        public string UserEmail { get; set; }

        public User User { get; set; }

        public int SendByUserId { get; set; }

        public User SendByUser { get; set; }

        public DateTime CreatedAt {  get; set; }

        public int FacilityId { get; set; }

        public Facility Facility { get; set; }

        public string Token { get; set; }
	    
        public bool Status { get; set; }

    }
}
