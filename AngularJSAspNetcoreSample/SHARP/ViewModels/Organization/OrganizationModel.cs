namespace SHARP.ViewModels.Organization
{
    public class OrganizationModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string OperatorName { get; set; }
        public string OperatorEmail { get; set; }

        public bool AttachPortalReport { get; set; }
        public bool Unlimited { get; set; }
    }
}
