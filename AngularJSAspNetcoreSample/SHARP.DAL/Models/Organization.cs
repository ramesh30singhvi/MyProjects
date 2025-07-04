using System;
using System.Collections.Generic;

namespace SHARP.DAL.Models
{
    public class Organization
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string ApplicationName { get; set; }

        public int? OrgId { get; set; }

        public Guid? OrgUuid { get; set; }

        public string Scope { get; set; }

        public string OperatorName { get; set; }
        public string OperatorEmail { get; set; }

        public bool? AttachPortalReport { get; set; }

        public ICollection<Facility> Facilities { get; set; }

        public ICollection<Form> Forms { get; set; }
        
        public ICollection<FormOrganization> FormOrganizations { get; set; }

        public ICollection<OrganizationRecipient> Recipients { get; set; }

        public ICollection<OrganizationMemo> OrganizationMemos { get; set; }

        public ICollection<UserOrganization> UserOrganizations { get; set; }

        public ICollection<DashboardInputTable> DashboardInputTables { get; set; }


        public ICollection<OrganizationPortalFeature> PortalFeatures { get; set;}

    }
}
