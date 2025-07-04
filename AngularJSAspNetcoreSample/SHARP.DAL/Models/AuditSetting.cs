using SHARP.Common.Enums;

namespace SHARP.DAL.Models
{
    public class AuditSetting
    {
        public int Id { get; set; }

        public int AuditId { get; set; }

        public AuditSettingType Type { get; set; }

        public string Value { get; set; }

        public Audit Audit { get; set; }

        public AuditSetting() { }

        public AuditSetting(Audit audit, AuditSettingType type, string value)
        {
            Audit = audit;
            Type = type;
            Value = value;
        }

        public AuditSetting Clone(Audit audit)
        {
            return new AuditSetting(audit, Type, Value );
        }
    }
}
