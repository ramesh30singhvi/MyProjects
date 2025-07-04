using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
    public class TwoFATokenRepository : GenericRepository<TwoFAToken>, ITwoFATokenRepository
    {
        public TwoFATokenRepository(SHARPContext context) : base(context)
        {
        }
    }
}
