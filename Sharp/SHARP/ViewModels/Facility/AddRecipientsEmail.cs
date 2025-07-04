using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SHARP.ViewModels.Facility
{
    public class AddRecipientsEmail
    {
        public int? Id { get; set; }

        public IReadOnlyCollection<string> Emails { get; set; }
    }
}
