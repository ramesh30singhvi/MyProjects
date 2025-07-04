using System.Collections.Generic;

namespace SHARP.ViewModels.Portal
{
    public class PortalReportSelectedModel
    {
        public IReadOnlyCollection<int> SelectedIds { get; set; }

        public IReadOnlyCollection<string> UserEmails { get; set; }

        public string Message { get; set; }
    }
}
