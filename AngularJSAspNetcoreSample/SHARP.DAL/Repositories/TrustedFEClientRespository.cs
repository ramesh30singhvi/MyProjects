using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
    public class TrustedFEClientRespository : GenericRepository<TrustedFEClient>, ITrustedFEClientRepository
    {
        public TrustedFEClientRespository(SHARPContext context) : base(context)
        {
        }
    }
}
