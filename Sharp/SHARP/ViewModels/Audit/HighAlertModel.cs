using SHARP.BusinessLogic.DTO;
using System;

namespace SHARP.ViewModels.Audit
{
    public class HighAlertValueModel
    {
        public int Id { get; set; }

        public OptionModel HighAlertCategory { get; set; }

        public string HighAlertDescription { get; set; }

        public string HighAlertNotes { get; set; }

        public OptionModel HighAlertStatus { get; set; }

        public int ChangeByUserId { get; set; }

        public int CreatedByUserId { get; set; }

        public string UserNotes { get; set; }

        public DateTime CreatedAt { get; set; }


    }
}
