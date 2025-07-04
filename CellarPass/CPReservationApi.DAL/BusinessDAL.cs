using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace CPReservationApi.DAL
{
    public class BusinessDAL : BaseDataAccess
    {
        public BusinessDAL(string connectionString) : base(connectionString)
        {

        }

        public List<SubscriptionPlans> GetSubscriptionPlans()
        {
            List<SubscriptionPlans> plansList = new List<SubscriptionPlans>();

            using (DbDataReader dataReader = GetDataReader("GetBusinessSubscriptionPlans", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        SubscriptionPlans plan = new SubscriptionPlans();
                        plan.id = Convert.ToInt32(dataReader["Id"]);
                        plan.name = Convert.ToString(dataReader["Name"]);
                        plan.monthly_fee = Convert.ToDecimal(dataReader["MonthlyFee"]);
                        plan.startup_fee = Convert.ToDecimal(dataReader["StartupFee"]);
                        plan.transaction_fee_widget = Convert.ToDecimal(dataReader["TransactionFeeWidget"]);
                        plan.plan_frequency = Convert.ToInt32(dataReader["PlanFrequency"]);
                        plansList.Add(plan);
                    }
                }
            }

            return plansList;
        }
    }
}
