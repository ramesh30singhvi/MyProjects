using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using uc = CPReservationApi.Common;

namespace CPReservationApi.DAL
{
    public class LogDAL : BaseDataAccess
    {
        public LogDAL(string connectionString) : base(connectionString)
        {
        }

        public void InsertLog(string LogSummary, string LogMsg,string currentUser,int LogType, int MemberId = 0)
        {
            if (string.IsNullOrWhiteSpace(currentUser))
            {
                currentUser = "CoreApi User";
            }
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@LogType", LogType));  //AppError = 1
            parameterList.Add(GetParameter("@LogSummary", LogSummary));
            parameterList.Add(GetParameter("@LogMsg", LogMsg));
            parameterList.Add(GetParameter("@LogDate", DateTime.UtcNow));
            parameterList.Add(GetParameter("@CurrentUser", currentUser));
            parameterList.Add(GetParameter("@MemberId", MemberId));

            string sql = "INSERT INTO [Logging] ([LogType],[LogSummary],[LogMsg],[LogDate],[CurrentUser],WineryId)";
            sql = sql + "VALUES (@LogType,@LogSummary,@LogMsg,@LogDate,@CurrentUser,@MemberId)";

            ExecuteNonQuery(sql, parameterList, CommandType.Text);
        }
    }
}
