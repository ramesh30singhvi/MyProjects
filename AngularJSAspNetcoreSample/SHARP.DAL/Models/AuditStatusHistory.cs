using SHARP.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Models
{
    public class AuditStatusHistory
    {
        public int Id { get; set; }
        public int AuditId { get; set; }
        public AuditStatus Status { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public Audit Audit { get; set; }
    }
}
