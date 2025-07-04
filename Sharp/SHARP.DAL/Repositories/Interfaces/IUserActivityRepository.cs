using System.Collections.Generic;
using SHARP.DAL.Models;
using SHARP.DAL.Models.UserActivityModels;

namespace SHARP.DAL.Repositories.Interfaces
{
    public interface IUserActivityRepository : IRepository<UserActivity>
    {
        public UserActivitiesModel GetUserActivities(string userIds, string fromDate, string toDate);
    }
}
