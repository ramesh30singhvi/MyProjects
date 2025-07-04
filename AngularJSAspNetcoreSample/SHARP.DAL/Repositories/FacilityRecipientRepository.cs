using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Repositories
{
    public class FacilityRecipientRepository : GenericRepository<FacilityRecipient>, IFacilityRecipientRepository
    {
        public FacilityRecipientRepository(SHARPContext context) : base(context)
        {
        }
    }
}
