namespace SHARP.ViewModels.Portal
{
    public class HighAlertStatusChangeModel
    {
        public int HighAlertId { get; set; }

        public OptionModel Status { get; set; }

        public string UserNotes { get; set; }

        public string ChangeBy { get; set; }
    }
}
