using System;
using System.Collections.Generic;
using System.Text;
using SHARP.DAL.Models;

namespace SHARP.BusinessLogic.Interfaces
{
    public interface IUserInfoService
    {
        User GetUserByAspnetUserID(string aspnetUserId);
    }
}
