using System;
using System.Collections.Generic;
using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.Data.SqlClient;
using SHARP.Common.Enums;
using SHARP.DAL.Models.UserActivityModels;

namespace SHARP.DAL.Repositories
{
    public class UserActivityRepository : GenericRepository<UserActivity>, IUserActivityRepository
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public UserActivityRepository(SHARPContext context, IConfiguration configuration) : base(context)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("ConnStr");
        }

        public UserActivitiesModel GetUserActivities(string userIds, string fromDate, string toDate)
        {
            UserActivitiesModel userActivitiesModel = new UserActivitiesModel();
            var toDateTime = DateTime.Parse(toDate).AddDays(1);
            toDate = toDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

            List<UserActivity> activities = new List<UserActivity>();
            List<User> users = new List<User>();
            List<UserAuditLogModel> userAuditLogModel = new List<UserAuditLogModel>();
            List<UserSummaryModel> userSummary = new List<UserSummaryModel>();

            var resultDataSet = new DataSet();

            using var connection = new SqlConnection();

            connection.ConnectionString = _connectionString;
            connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = "GetUserActivities";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add("@userIds", SqlDbType.VarChar).Value = userIds;
            command.Parameters.Add("@fromDate", SqlDbType.Date).Value = fromDate;
            command.Parameters.Add("@toDate", SqlDbType.Date).Value = toDate;
            command.CommandTimeout = 1200;

            using var dataAdapter = new SqlDataAdapter(command);
            dataAdapter.Fill(resultDataSet);


            if (resultDataSet.Tables != null && resultDataSet.Tables.Count > 0 && resultDataSet.Tables.Count == 4)
            {
                DataTable userActivitiesDT = new DataTable();
                userActivitiesDT = resultDataSet.Tables[0];

                DataTable activeUsersDT = new DataTable();
                activeUsersDT = resultDataSet.Tables[1];

                DataTable userAuditLogDT = new DataTable();
                userAuditLogDT = resultDataSet.Tables[2];

                DataTable userSummaryDT = new DataTable();
                userSummaryDT = resultDataSet.Tables[3];

                foreach (DataRow dr in userActivitiesDT.Rows)
                {
                    UserActivity userActivity = new UserActivity();
                    userActivity.Id = Convert.ToInt32(dr["Id"].ToString());
                    userActivity.UserId = dr["UserId"] != DBNull.Value ? Convert.ToInt32(dr["UserId"]) : 0;
                    int? auditId = dr["AuditId"] != DBNull.Value ? Convert.ToInt32(dr["AuditId"]) : 0;
                    userActivity.AuditId = auditId == 0 ? null : auditId;
                    userActivity.ActionType = (ActionType)Enum.Parse(typeof(ActionType), dr["ActionType"].ToString(), true);
                    userActivity.ActionTime = Convert.ToDateTime(dr["ActionTime"]);
                    userActivity.UserAgent = dr["UserAgent"].ToString();
                    userActivity.IP = dr["IP"].ToString();
                    userActivity.User = new User { FullName = dr["UserName"].ToString() ?? "" };
                    userActivity.Audit = new Audit
                    {
                        FormVersion = new FormVersion
                        {
                            Form = new Form
                            {
                                AuditType = new AuditType
                                {
                                    Name = dr["AuditType"].ToString()
                                },
                                Name = dr["AuditName"].ToString()
                            }
                        }
                    };
                    int? updatedUserId = dr["UpdatedUserId"] != DBNull.Value ? Convert.ToInt32(dr["UpdatedUserId"]) : 0;
                    userActivity.UpdatedUserId = updatedUserId == 0 ? null : updatedUserId;
                    userActivity.UpdatedUser = new User { FullName = dr["UpdatedUserName"].ToString() ?? "" };
                    userActivity.LoginUsername = dr["LoginUsername"].ToString();

                    activities.Add(userActivity);
                }

                foreach (DataRow dr in activeUsersDT.Rows)
                {
                    users.Add(new User
                    {
                        Id = Convert.ToInt32(dr["Id"]),
                        FirstName = dr["FirstName"].ToString(),
                        LastName = dr["LastName"].ToString(),
                        Email = dr["Email"].ToString(),
                        FullName = dr["FullName"].ToString(),
                        TimeZone = dr["TimeZone"].ToString(),
                    });
                }

                foreach (DataRow dr in userAuditLogDT.Rows)
                {
                    userAuditLogModel.Add(new UserAuditLogModel
                    {
                        SubmittedByUser = dr["SubmittedByUser"].ToString(),
                        AuditId = Convert.ToInt32(dr["ID"]),
                        FormName = dr["Name"].ToString(),
                        Status = (AuditStatus)Convert.ToInt32(dr["Status"]),
                        AuditorsTime = dr["AuditorsTime"].ToString(),
                        Duration = dr["Duration"].ToString()
                    });
                }

                foreach (DataRow dr in userSummaryDT.Rows)
                {
                    userSummary.Add(new UserSummaryModel
                    {
                        FullName = dr["FullName"].ToString(),
                        SentForApprovalCount = dr["SentForApprovalCount"].ToString(),
                    });
                }
            }
            else
            {
                throw new Exception("DataSet empty on GetUserActivities()");
            }

            userActivitiesModel.UserActivity = activities;
            userActivitiesModel.User = users;
            userActivitiesModel.UserAuditLog = userAuditLogModel;
            userActivitiesModel.UserSummary = userSummary;

            return userActivitiesModel;
        }
    }
}