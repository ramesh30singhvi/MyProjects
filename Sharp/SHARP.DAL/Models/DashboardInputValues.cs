using System;
using System.ComponentModel.DataAnnotations;

namespace SHARP.DAL.Models
{
	public class DashboardInputValues
	{
        public int Id { get; set; }

        public int ElementId { get; set; }

        public int FacilityId { get; set; }

        public DateTime Date { get; set; }

        public int Value { get; set; }

        public DashboardInputElement DashboardInputElement { get; set; }

        public Facility Facility { get; set; }
    }
}

