using System;

namespace SHARP.BusinessLogic.Extensions
{
    public class NotActiveAuditException : Exception
    {
        public NotActiveAuditException()
        {
        }

        public NotActiveAuditException(string message) : base(message)
        {
        }
    }
}
