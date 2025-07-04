using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace CPReservationApi.DAL
{
    public class POSDAL : BaseDataAccess
    {
        public POSDAL(string connectionString) : base(connectionString)
        {

        }
        public bool UpdateReceiptSettings(WineryReceiptSetting receiptSettings)
        {
            string sql = "AddUpdateReceiptSettings";
            int ret = 0;
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", receiptSettings.member_id));
            parameterList.Add(GetParameter("@ShowLogo", receiptSettings.show_logo));
            parameterList.Add(GetParameter("@ShowLocationAddress", receiptSettings.show_location_address));
            parameterList.Add(GetParameter("@ShowLocationPhoneNum", receiptSettings.show_location_phone_number));
            parameterList.Add(GetParameter("@ShowCustomerInfo", receiptSettings.show_customer_information));
            parameterList.Add(GetParameter("@ShowOrderNote", receiptSettings.show_order_note));
            parameterList.Add(GetParameter("@ShowBarcode", receiptSettings.show_barcode));
            parameterList.Add(GetParameter("@ShowSoldBy", receiptSettings.show_sold_by));
            parameterList.Add(GetParameter("@HeaderText", receiptSettings.header_text));
            parameterList.Add(GetParameter("@FooterText", receiptSettings.footer_text));
            parameterList.Add(GetParameter("@CollectSignature", receiptSettings.collect_signature));
            parameterList.Add(GetParameter("@HasMinimumAmountSignatureRequirement", receiptSettings.has_minimum_amount_signature_requirement));
            parameterList.Add(GetParameter("@SignatureCaptureType", (int)receiptSettings.signature_capture_type));
            parameterList.Add(GetParameter("@ShowAddTipLine", receiptSettings.show_add_tip_line));
            parameterList.Add(GetParameter("@AddTip", receiptSettings.add_tip));

            try
            {
                ret = ExecuteNonQuery(sql, parameterList, CommandType.StoredProcedure);
            }
            catch (Exception e)
            {

            }
            return (ret > 0);
        }

        public WineryReceiptSettingsWithLogo GetReceiptSettingByMember(int memberId)
        {
            WineryReceiptSettingsWithLogo ret = null;

            string sql = "GetWineryReceiptSetting";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@MemberId", memberId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ret = new WineryReceiptSettingsWithLogo
                        {
                            show_barcode = Convert.ToBoolean(dataReader["ShowBarcode"]),
                            show_customer_information = Convert.ToBoolean(dataReader["ShowCustomerInfo"]),
                            show_location_address = Convert.ToBoolean(dataReader["ShowLocationAddress"]),
                            show_logo = Convert.ToBoolean(dataReader["ShowLogo"]),
                            show_location_phone_number = Convert.ToBoolean(dataReader["ShowLocationPhoneNum"]),
                            show_order_note = Convert.ToBoolean(dataReader["ShowOrderNote"]),
                            show_sold_by = Convert.ToBoolean(dataReader["ShowSoldBy"]),
                            header_text = Convert.ToString(dataReader["HeaderText"]),
                            footer_text = Convert.ToString(dataReader["FooterText"]),
                            logo_url = Convert.ToString(dataReader["LogoUrl"]),
                            collect_signature = Convert.ToBoolean(dataReader["CollectSignature"]),
                            has_minimum_amount_signature_requirement = Convert.ToBoolean(dataReader["HasMinimumAmountSignatureRequirement"]),
                            signature_capture_type = (Common.SignatureCaptureType)Convert.ToInt32(dataReader["SignatureCaptureType"]),
                            show_add_tip_line = Convert.ToBoolean(dataReader["ShowAddTipLine"]),
                            add_tip = Convert.ToBoolean(dataReader["AddTip"]),
                            member_id = memberId
                        };

                    }
                }
            }

            return ret;
        }

        public bool UpdateReceiptLogoUrl(int member_id, string filename)
        {
            string sql = "Update Winery_Receipt_Settings set LogoUrl=@logoFile where WineryId=@WineryId";
            int ret = 0;
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", member_id));
            parameterList.Add(GetParameter("@logoFile", filename));
            try
            {
                ret = ExecuteNonQuery(sql, parameterList, CommandType.Text);
            }
            catch (Exception e)
            {

            }
            return (ret > 0);
        }

        public int AddOrderSignature(int OrderId, int OrderPaymentId, string SignatureUrl, int SignatureType, Guid SignatureGUID)
        {
            string sql = "AddOrderSignature";
            int Id = 0;

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@OrderId", OrderId));
            parameterList.Add(GetParameter("@OrderPaymentId", OrderPaymentId));
            parameterList.Add(GetParameter("@SignatureUrl", SignatureUrl));
            parameterList.Add(GetParameter("@SignatureType", SignatureType));
            parameterList.Add(GetParameter("@SignatureGUID", SignatureGUID));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        Id = Convert.ToInt32(dataReader["Id"]);
                    }
                }
            }

            return Id;
        }
    }
}
