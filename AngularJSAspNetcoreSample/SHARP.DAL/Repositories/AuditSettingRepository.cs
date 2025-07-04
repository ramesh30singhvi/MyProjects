using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
    public class AuditSettingRepository : GenericRepository<AuditSetting>, IAuditSettingRepository
    {
        public AuditSettingRepository(SHARPContext context) : base(context)
        {
        }
    }
}