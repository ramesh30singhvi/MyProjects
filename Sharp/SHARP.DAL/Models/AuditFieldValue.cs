using SHARP.Common.Constants;

namespace SHARP.DAL.Models
{
    public class AuditFieldValue : IIdModel<int>
    {
        public int Id { get; set; }

        public int FormFieldId { get; set; }

        public int AuditId { get; set; }

        public string Value { get; set; }

        public FormField FormField { get; set; }

        public Audit Audit { get; set; }

        public AuditFieldValue() { }

        public AuditFieldValue(int formFieldId, string value, Audit audit)
        {
            FormFieldId = formFieldId;
            Value = value;
            Audit = audit;
        }

        public AuditFieldValue Clone(Audit audit)
        {
            return new AuditFieldValue(FormFieldId, Value, audit);
        }
    }
}
