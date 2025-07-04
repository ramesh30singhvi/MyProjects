using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
    public class AuditTypeRepository : GenericRepository<AuditType>, IAuditTypeRepository
    {
        public AuditTypeRepository(SHARPContext context) : base(context)
        {
        }
    }
}