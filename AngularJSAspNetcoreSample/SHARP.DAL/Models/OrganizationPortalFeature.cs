using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Models
{
    public class OrganizationPortalFeature
    {
        public int Id { get; set; }

        public int OrganizationId { get; set; }

        public int PortalFeatureId { get; set; }

        public PortalFeature PortalFeature { get; set; }

        public bool Available { get; set; }
    }
}
