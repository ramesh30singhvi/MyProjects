using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SHARP.ViewModels.Organization
{
    public class AddOrganizationModel
    {
        [Required]
        public string Name { get; set; }

        public string OperatorName { get; set; }
        public string OperatorEmail { get; set; }

        public bool AttachPortalReport { get; set; }
        public IEnumerable<string> Recipients { get; set; }
    }
}