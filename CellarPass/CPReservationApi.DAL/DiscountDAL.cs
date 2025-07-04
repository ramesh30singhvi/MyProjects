using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace CPReservationApi.DAL
{
    public class DiscountDAL : BaseDataAccess
    {
        public DiscountDAL(string connectionString) : base(connectionString)
        {
        }
        public EventDiscount GetDiscountDetail(int EventId, string DiscountCode)
        {
            EventDiscount model = new EventDiscount();
            string sql = "select d.[Id],d.[MemberId],d.[Active],d.[DiscountName],d.DiscountType,d.[DiscountCode],d.[DiscountAmount],d.[DiscountPercent],d.[NumberOfUses],d.[StartDateTime],d.[EndDateTime]";
            sql += ",d.[RequiredMinimum],d.[RequiredMaximum],d.DateType from Discounts (nolock) d join Event_Discounts (nolock) ed on d.Id=ed.Discounts_Id where DiscountCode= @DiscountCode and Events_EventId = @EventId";


            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", EventId));
            parameterList.Add(GetParameter("@DiscountCode", DiscountCode));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.Id = Convert.ToInt32(dataReader["Id"]);
                        model.Active = Convert.ToBoolean(dataReader["Active"]);
                        model.MemberId = Convert.ToInt32(dataReader["MemberId"]);
                        model.DiscountName = Convert.ToString(dataReader["DiscountName"]);
                        model.DiscountCode = Convert.ToString(dataReader["DiscountCode"]);
                        model.DiscountAmount = Convert.ToDecimal(dataReader["DiscountAmount"]);
                        model.DiscountPercent = Convert.ToDecimal(dataReader["DiscountPercent"]);
                        model.NumberOfUses = Convert.ToInt32(dataReader["NumberOfUses"]);
                        model.StartDateTime = Convert.ToDateTime(dataReader["StartDateTime"]);
                        model.EndDateTime = Convert.ToDateTime(dataReader["EndDateTime"]);
                        model.RequiredMinimum = Convert.ToInt32(dataReader["RequiredMinimum"]);
                        model.RequiredMaximum = Convert.ToInt32(dataReader["RequiredMaximum"]);
                        model.DiscountType = (Common.Common.DiscountOption)Convert.ToInt32(dataReader["DiscountType"]);
                        model.DateType = (Common.Common.DateType)Convert.ToInt32(dataReader["DateType"]);
                    }
                }
                return model;
            }
        }

        public int GetDiscountUseCountByCode(int EventId, string DiscountCode)
        {
            EventDiscount model = new EventDiscount();
            string sql = "select count(ReservationId) as DiscountUseCountByCode from ReservationV2 (nolock) where [Status] not in (2,7,8) and DiscountCode=@DiscountCode and EventId = @EventId";
            int DiscountUseCountByCode = 0;

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@EventId", EventId));
            parameterList.Add(GetParameter("@DiscountCode", DiscountCode));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        DiscountUseCountByCode = Convert.ToInt32(dataReader["DiscountUseCountByCode"]);
                    }
                }
                return DiscountUseCountByCode;
            }
        }
    }
}
