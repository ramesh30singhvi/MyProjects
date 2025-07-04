using CPReservationApi.Common;
using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Text;

namespace CPReservationApi.DAL
{
    public class EmailServiceDAL : BaseDataAccess
    {
        public EmailServiceDAL(string connectionString) : base(connectionString)
        {
        }

        public EmailContent GetEmailContentByID(int ID)
        {
            var model = new EmailContent();
            try
            {
                var parameterList = new List<DbParameter>();
                string sql = "select Id, TemplateID, EmailFormat, EmailFrom, EmailSubject, EmailBody, WineryID, Active, dateCreated, createdByUser, dateModified, modifiedByUser, EmailTo, EmailName, SystemDefault, EmailSubjectAdmin, EmailBodyAdmin from EmailContent where Id=@ID";
                parameterList.Add(GetParameter("@ID", ID));
                using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            model.Id = Convert.ToInt32(dataReader["Id"]);
                            //model.TemplateID = Convert.ToInt32(dataReader["TemplateID"]);
                           // model.EmailFormat = Convert.ToInt32(dataReader["EmailFormat"]);
                            model.EmailFrom = Convert.ToString(dataReader["EmailFrom"]);
                            model.EmailSubject = Convert.ToString(dataReader["EmailSubject"]);
                            model.EmailBody = Convert.ToString(dataReader["EmailBody"]);
                            //model.WineryID = Convert.ToInt32(dataReader["WineryID"]);
                            model.Active = Convert.ToBoolean(dataReader["Active"]);
                            //model.dateCreated = Convert.ToDateTime(dataReader["dateCreated"]);
                            //model.createdByUser = Convert.ToString(dataReader["createdByUser"]);
                            //model.dateModified = Convert.ToDateTime(dataReader["dateModified"]);
                            //model.modifiedByUser = Convert.ToString(dataReader["modifiedByUser"]);
                            model.EmailTo = Convert.ToString(dataReader["EmailTo"]);
                            //model.EmailName = Convert.ToString(dataReader["EmailName"]);
                            //model.SystemDefault = Convert.ToBoolean(dataReader["SystemDefault"]);
                            model.EmailSubjectAdmin = Convert.ToString(dataReader["EmailSubjectAdmin"]);
                            model.EmailBodyAdmin = Convert.ToString(dataReader["EmailBodyAdmin"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return model;
        }

        public EmailContent GetEmailContent(int templateId, int wineryId)
        {
            var model = new EmailContent();
            try
            {
                var parameterList = new List<DbParameter>();
                string sql = "select Id, TemplateID, EmailFormat, EmailFrom, EmailSubject, EmailBody, WineryID, Active, dateCreated, createdByUser, dateModified, modifiedByUser, EmailTo, EmailName, SystemDefault, EmailSubjectAdmin, EmailBodyAdmin,BusinessMessage from EmailContent where WineryID=@WineryID and TemplateID=@TemplateID and Active = 1";
                parameterList.Add(GetParameter("@TemplateID", templateId));
                parameterList.Add(GetParameter("@WineryID", wineryId));
                using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            model.Id = Convert.ToInt32(dataReader["Id"]);
                            //model.TemplateID = Convert.ToInt32(dataReader["TemplateID"]);
                            //model.EmailFormat = Convert.ToInt32(dataReader["EmailFormat"]);
                            model.EmailFrom = Convert.ToString(dataReader["EmailFrom"]);
                            model.EmailSubject = Convert.ToString(dataReader["EmailSubject"]);
                            model.EmailBody = Convert.ToString(dataReader["EmailBody"]);
                            //model.WineryID = Convert.ToInt32(dataReader["WineryID"]);
                            model.Active = Convert.ToBoolean(dataReader["Active"]);
                            //model.dateCreated = Convert.ToDateTime(dataReader["dateCreated"]);
                            //model.createdByUser = Convert.ToString(dataReader["createdByUser"]);
                            //model.dateModified = Convert.ToDateTime(dataReader["dateModified"]);
                            //model.modifiedByUser = Convert.ToString(dataReader["modifiedByUser"]);
                            model.EmailTo = Convert.ToString(dataReader["EmailTo"]);
                            //model.EmailName = Convert.ToString(dataReader["EmailName"]);
                            //model.SystemDefault = Convert.ToBoolean(dataReader["SystemDefault"]);
                            model.EmailSubjectAdmin = Convert.ToString(dataReader["EmailSubjectAdmin"]);
                            model.EmailBodyAdmin = Convert.ToString(dataReader["EmailBodyAdmin"]);
                            model.BusinessMessage = Convert.ToString(dataReader["BusinessMessage"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return model;
        }

        public int CheckFAQExistsForWinery(int wineryId)
        {
            var isfaq = 0;
            try
            {
                var parameterList = new List<DbParameter>();
                string sql = "select count(Id) as FaqCount from Winery_FAQ where WineryId=@WineryID";              
                parameterList.Add(GetParameter("@WineryID", wineryId));
                using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            int count = Convert.ToInt32(dataReader["FaqCount"]);
                            isfaq = (count > 0) ? 1 : 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
            return isfaq;
        }

        public bool SaveEmailLog(EmailLogModel model)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@RefId", model.RefId));
            parameterList.Add(GetParameter("@EmailType", model.EmailType));
            parameterList.Add(GetParameter("@EmailProvider", model.EmailProvider));
            parameterList.Add(GetParameter("@EmailStatus", model.EmailStatus));
            parameterList.Add(GetParameter("@EmailSender", model.EmailSender));
            parameterList.Add(GetParameter("@EmailRecipient", model.EmailRecipient));
            parameterList.Add(GetParameter("@LogNote", model.LogNote ?? ""));
            parameterList.Add(GetParameter("@MemberId", model.MemberId));
            parameterList.Add(GetParameter("@EmailContentId", model.EmailContentId));
            int retvalue = ExecuteNonQuery("InsertEmailLog", parameterList);
            return retvalue > 0;
        }

        public int SaveTempQueueData(int EventId,string GuestName,string GuestEmailAddress,string ContactReason,string ContactMessage)
        {
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@EventId", EventId));
            parameterList.Add(GetParameter("@GuestName", GuestName));
            parameterList.Add(GetParameter("@GuestEmailAddress", GuestEmailAddress));
            parameterList.Add(GetParameter("@ContactReason", ContactReason));
            parameterList.Add(GetParameter("@ContactMessage", ContactMessage));

            int retvalue = Convert.ToInt32(ExecuteScalar("InsertTempQueueData", parameterList));
            return retvalue;
        }

        public void DeleteTempQueueData(int ID)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ID", ID));

            string sql = "DELETE FROM [dbo].[TempQueueData] WHERE Id = @ID";

            ExecuteNonQuery(sql, parameterList, CommandType.Text);
        }

        public TempQueueDataModel GetTempQueueDataByID(int ID)
        {
            var model = new TempQueueDataModel();
            try
            {
                var parameterList = new List<DbParameter>();
                string sql = "SELECT tqd.Id,[GuestName],[GuestEmailAddress],[ContactReason],[ContactMessage],EventTitle,EventOrganizerName,EventOrganizerPhone,EventOrganizerEmail,StartDateTime,TimeZoneId,Winery_Id FROM [dbo].[TempQueueData] tqd join Tickets_Event e on tqd.[EventId] = e.Id where tqd.Id= @Id";
                parameterList.Add(GetParameter("@Id", ID));
                using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            model.Id = Convert.ToInt32(dataReader["Id"]);
                            model.GuestName = Convert.ToString(dataReader["GuestName"]);
                            model.GuestEmailAddress = Convert.ToString(dataReader["GuestEmailAddress"]);
                            model.ContactReason = Convert.ToString(dataReader["ContactReason"]);
                            model.ContactMessage = Convert.ToString(dataReader["ContactMessage"]);
                            model.EventTitle = Convert.ToString(dataReader["EventTitle"]);
                            model.EventOrganizerName = Convert.ToString(dataReader["EventOrganizerName"]);
                            model.EventOrganizerPhone = Convert.ToString(dataReader["EventOrganizerPhone"]);
                            model.EventOrganizerEmail = Convert.ToString(dataReader["EventOrganizerEmail"]);
                            model.StartDateTime = Convert.ToDateTime(dataReader["StartDateTime"]);
                            model.TimeZoneId = (Times.TimeZone)Convert.ToInt32(dataReader["TimeZoneId"]);
                            model.member_id = Convert.ToInt32(dataReader["Winery_Id"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            return model;
        }
    }
}
