using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Models
{
    public class HighAlertCategory
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool Active { get; set; }

        public ICollection<HighAlertCategoryToPotentialAreas> HighAlertCategoryToPotentialAreas { get; set; }

        public HighAlertCategory()
        {
            HighAlertCategoryToPotentialAreas = new List<HighAlertCategoryToPotentialAreas>(); ;
        }
    }
}
