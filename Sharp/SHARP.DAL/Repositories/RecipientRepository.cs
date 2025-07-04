using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
    public class RecipientRepository : GenericRepository<OrganizationRecipient>, IRecipientRepository
    {
        public RecipientRepository(SHARPContext context) : base(context)
        {
        }
    }
}
