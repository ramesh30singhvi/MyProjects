using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Repositories
{
    public  class TeamRepository : GenericRepository<Team>, ITeamRepository
    {
        public TeamRepository(SHARPContext context) : base(context)
        {
        }
    }
}
