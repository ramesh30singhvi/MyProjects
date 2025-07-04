using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Models
{
    public class AuditTriggeredByKeyword
    {
        public AuditTriggeredByKeyword(int auditId, int auditTableColumnValueId, int createdAuditId)
        {
            AuditId = auditId;
            AuditTableColumnValueId = auditTableColumnValueId;
            CreatedAuditId = createdAuditId;
        }
        public int Id { get; set; }

        public int AuditId { get; set; }

        public int AuditTableColumnValueId { get; set; }

        public int CreatedAuditId { get; set; }

        public Audit CreatedAuditByKeyword { get; set; }

        public AuditTriggeredByKeyword Clone()
        {
            var auditTriggerByKeyword = new AuditTriggeredByKeyword(AuditId, AuditTableColumnValueId, CreatedAuditId);

            return auditTriggerByKeyword;
        }
    }
}
