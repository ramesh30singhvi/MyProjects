using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Portal
{
    public class HighAlertCategoryPotentialAreaDto 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string[] HighAlertCategoryWithPotentialAreas { get; set; }
    }
}
