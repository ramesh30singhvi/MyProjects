using CPReservationApi.Common;
using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using static CPReservationApi.Common.Common;

namespace CPReservationApi.DAL
{
    public class SettingsDAL : BaseDataAccess
    {
        public SettingsDAL(string connectionString) : base(connectionString)
        {
        }

        public CustomSetting GetCustomSettingByMember(SettingType settingType, int memberId)
        {
            CustomSetting setting = null;
            try
            {
                var parameterList = new List<DbParameter>();
                string sql = "SELECT Id,Label1,Value1,Label2,Value2,Label3,Value3,Label4,Value4,Label5,Value5 FROM dbo.CustomSettings where Member_Id=@memberId and CustomSetting_Id=@settingId";
                parameterList.Add(GetParameter("@memberId", memberId));
                parameterList.Add(GetParameter("@settingId", (int)settingType));
                using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            setting = new CustomSetting();
                            setting.id = Convert.ToInt32(dataReader["Id"]);
                            setting.member_id = memberId;
                            setting.custom_setting_id = settingType;
                            setting.label_1 = Convert.ToString(dataReader["Label1"]);
                            setting.value_1 = Convert.ToString(dataReader["Value1"]);
                            setting.label_2 = Convert.ToString(dataReader["Label2"]);
                            setting.value_2 = Convert.ToString(dataReader["Value2"]);
                            setting.label_3 = Convert.ToString(dataReader["Label3"]);
                            setting.value_3 = Convert.ToString(dataReader["Value3"]);
                            setting.label_4 = Convert.ToString(dataReader["Label4"]);
                            setting.value_4 = Convert.ToString(dataReader["Value4"]);
                            setting.label_4 = Convert.ToString(dataReader["Label5"]);
                            setting.value_4 = Convert.ToString(dataReader["Value5"]);

                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return setting;
        }
        public List<Settings.Setting> GetSettingGroup(int memberId, int key)
        {
            var settinglist = new List<Settings.Setting>();
            try
            {
                var parameterList = new List<DbParameter>();
                string sql = "select Id,Winery_Id,[Group],[Key],[Value] from Settings (nolock) where [Group]=@Group and Winery_Id=@Winery_Id";
                parameterList.Add(GetParameter("@Winery_Id", memberId));
                parameterList.Add(GetParameter("@Group", key));
                using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            var setting = new Settings.Setting();
                            setting.Id = Convert.ToInt32(dataReader["Id"]);
                            setting.MemberId = Convert.ToInt32(dataReader["Winery_Id"]);
                            setting.Group = (Common.Common.SettingGroup)(dataReader["Group"]);
                            setting.Key = (Common.Common.SettingKey)(dataReader["Key"]);
                            setting.Value = Convert.ToString(dataReader["Value"]);
                            settinglist.Add(setting);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return settinglist;
        }

        public string GetContent(SiteContentType contentType, int memberId = 0)
        {
            string content = "";
            var parameterList = new List<DbParameter>();

            string sql = "select Content from SiteContent where ContentID=@ContentId and WineryId=@Winery_Id";
            parameterList.Add(GetParameter("@Winery_Id", memberId));
            parameterList.Add(GetParameter("@ContentId", (int)contentType));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        content = Convert.ToString(dataReader["Content"]);
                    }
                }
            }

            return content;

        }

        public bool SaveSetting(Settings.Setting setting)
        {
            var parameterList = new List<DbParameter>();

            string sql = "update settings set [Value]=@Value from Settings where [Group]=@Group and Winery_Id=@Winery_Id and [Key]=@Key";
            parameterList.Add(GetParameter("@Winery_Id", setting.MemberId));
            parameterList.Add(GetParameter("@Group", setting.Group));
            parameterList.Add(GetParameter("@Key", setting.Key));
            parameterList.Add(GetParameter("@Value", setting.Value));

            int ret = ExecuteNonQuery(sql, parameterList, CommandType.Text);


            return (ret > 0);

        }

        public bool InsertSetting(Settings.Setting setting)
        {
            var parameterList = new List<DbParameter>();
            int retval = 0;
            parameterList.Add(GetParameter("@Winery_Id", setting.MemberId));
            parameterList.Add(GetParameter("@Group", setting.Group));
            parameterList.Add(GetParameter("@Key", setting.Key));
            parameterList.Add(GetParameter("@Value", setting.Value));

            retval = ExecuteNonQuery("SaveSetting", parameterList, CommandType.StoredProcedure);

            return (retval > 0);
        }

        public bool UpdateUserConfig1(int paymentGateway, string token, int memberId)
        {
            var parameterList = new List<DbParameter>();
            int ret = 0;
            string sql = "update PaymentConfig set [UserConfig1]=@UserConfig1 where [PaymentGateway]=@PaymentGateway and Winery_ID=@MemberId";
            parameterList.Add(GetParameter("@UserConfig1", token));
            parameterList.Add(GetParameter("@PaymentGateway", paymentGateway));
            parameterList.Add(GetParameter("@MemberId", memberId));
            try
            {
                ret = ExecuteNonQuery(sql, parameterList, CommandType.Text);
            }
            catch (Exception e)
            {

            }
            return (ret > 0);
        }

        public List<Settings.Setting> GetSetting(int memberId,int group, int key)
        {
            var settinglist = new List<Settings.Setting>();
            try
            {
                var parameterList = new List<DbParameter>();
                parameterList.Add(GetParameter("@WineryId", memberId));
                parameterList.Add(GetParameter("@Group", group));
                parameterList.Add(GetParameter("@Key", key));
                using (DbDataReader dataReader = GetDataReader("GetSetting", parameterList, CommandType.StoredProcedure))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            var setting = new Settings.Setting();
                            setting.Id = Convert.ToInt32(dataReader["Id"]);
                            setting.MemberId = Convert.ToInt32(dataReader["Winery_Id"]);
                            setting.Group = (Common.Common.SettingGroup)(dataReader["Group"]);
                            setting.Key = (Common.Common.SettingKey)(dataReader["Key"]);
                            setting.Value = Convert.ToString(dataReader["Value"]);
                            settinglist.Add(setting);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return settinglist;
        }
    }
}
