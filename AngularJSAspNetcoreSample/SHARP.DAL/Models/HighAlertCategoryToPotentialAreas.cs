using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Models
{
    public class HighAlertCategoryToPotentialAreas
    {
        public int Id { get; set; }
        public int HighAlertCategoryID { get; set; }
        public int HighAlertPotentialAreasID { get; set; }

        public HighAlertPotentialAreas HighAlertPotentialAreas { get; set; }
    }
}
