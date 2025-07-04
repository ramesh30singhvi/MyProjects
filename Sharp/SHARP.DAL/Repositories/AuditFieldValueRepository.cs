using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
    public class AuditFieldValueRepository : GenericRepository<AuditFieldValue>, IAuditFieldValueRepository
    {
        public AuditFieldValueRepository(SHARPContext context) : base(context)
        {
        }
    }
}
