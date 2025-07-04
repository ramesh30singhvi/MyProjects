using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Repositories
{
    public class UserTeamRepository : GenericRepository<UserTeam>, IUserTeamRepository
    {
        public UserTeamRepository(SHARPContext context) : base(context)
        {
        }
    }

}
