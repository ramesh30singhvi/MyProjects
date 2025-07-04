using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.ViewModels;
using Microsoft.Extensions.Options;
using CPReservationApi.WebApi.Services;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CPReservationApi.WebApi.Controllers
{
    [Route("api/tablestatus")]
    public class TableStatusController : BaseController
    {
        public static IOptions<AppSettings> _appSetting;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appSetting"></param>
        public TableStatusController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }

        /// <summary>
        /// This method gives list of tablestatusgroup
        /// </summary>
        /// <returns></returns>
        [Route("tablestatusgroup")]
        [HttpGet]
        public IActionResult GetTableStatusGroup(int member_id, bool include_default = false)
        {
            TableStatusGroupDAL tableStatusGroupDAL = new TableStatusGroupDAL(Common.Common.ConnectionString);
            var tableStatusGroupResponse = new TableStatusGroupResponse();

            try
            {
                var model = new List<TableStatusGroupModel>();
                model = tableStatusGroupDAL.GetTableStatusGroup(member_id, include_default);

                if (model != null)
                {
                    tableStatusGroupResponse.success = true;
                    tableStatusGroupResponse.data = model;
                }
                else
                {
                    tableStatusGroupResponse.success = true;
                    tableStatusGroupResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    tableStatusGroupResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                tableStatusGroupResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                tableStatusGroupResponse.error_info.extra_info = Common.Common.InternalServerError;
                tableStatusGroupResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTableStatusGroup:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(tableStatusGroupResponse);
        }

        /// <summary>
        /// This method gives list of tablestatusgroupitem
        /// </summary>
        /// <param name="table_status_group_id">Id of table status group id (Required)</param>
        /// <returns></returns>
        [Route("tablestatusgroupitem")]
        [HttpGet]
        public IActionResult GetTableStatusGroupItem(int table_status_group_id)
        {
            TableStatusGroupDAL tableStatusGroupDAL = new TableStatusGroupDAL(Common.Common.ConnectionString);
            var tableStatusGroupItemResponse = new TableStatusGroupItemResponse();

            try
            {
                var model = new List<TableStatusGroupItem>();
                model = tableStatusGroupDAL.GetTableStatusGroupItemByTableStatusGroupId(table_status_group_id);

                if (model != null)
                {
                    tableStatusGroupItemResponse.success = true;
                    tableStatusGroupItemResponse.data = model;
                }
                else
                {
                    tableStatusGroupItemResponse.success = true;
                    tableStatusGroupItemResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    tableStatusGroupItemResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                tableStatusGroupItemResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                tableStatusGroupItemResponse.error_info.extra_info = Common.Common.InternalServerError;
                tableStatusGroupItemResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTableStatusGroupItem:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(tableStatusGroupItemResponse);
        }

        [Route("savetablestatusgroup")]
        [HttpPost]
        public async Task<IActionResult> SaveTableStatusGroup([FromBody]CreateTableStatusGroupRequest model)
        {
            var tableStatusGroupDAL = new TableStatusGroupDAL(Common.Common.ConnectionString);
            var response = new CreateTableStatusGroupResponse();
            try
            {
                if (model.id > 0)
                {
                    response.data.id = model.id;
                    response.success = tableStatusGroupDAL.UpdateTableStatusGroup(model);
                }
                else
                {
                    response.data.id = tableStatusGroupDAL.SaveTableStatusGroup(model);
                    response.success = true;
                }

                foreach (var item in model.table_status_group_item)
                {
                    item.TableStatusGroupId = response.data.id;
                    response.success = await Task.Run(() => tableStatusGroupDAL.UpdateTableStatusGroupItem(item));
                }
            }
            catch (Exception ex)
            {
                response.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                response.error_info.extra_info = Common.Common.InternalServerError;
                response.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "SaveTableStatusGroup:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,model.member_id);
            }
            return new ObjectResult(response);
        }

        [Route("blocktables")]
        [HttpPost]
        public async Task<IActionResult> BlockTables([FromBody]BlockTablesRequest model)
        {
            var tableStatusGroupDAL = new TableStatusGroupDAL(Common.Common.ConnectionString);
            var blockTablesResponse = new BlockTablesResponse();
            try
            {
                if (model.force)
                {
                    List<int> ids = new List<int>();
                    foreach (var item in model.table_ids)
                    {
                        int id = tableStatusGroupDAL.ForceTableBlocked(item, model.member_id, Convert.ToDateTime(model.start_date),Convert.ToDateTime(model.end_date));
                        if (id > 0)
                            ids.Add(id);
                    }
                    blockTablesResponse.data.ids = ids;
                    blockTablesResponse.success = true;
                }
                else
                {
                    EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                    WaitlistModel waitlistModel = new WaitlistModel();
                    
                    List<int> ids = new List<int>();
                    foreach (var item in model.table_ids)
                    {
                        var seatingSessionModel = new List<SeatingSessionModel>();
                        seatingSessionModel = eventDAL.GetSeatingSession(item, Convert.ToDateTime(model.start_date),Convert.ToDateTime(model.end_date));
                        foreach (var ss in seatingSessionModel)
                        {
                            if (ss.TransactionCategory == 1) //Waitlists = 1
                            {
                                waitlistModel = eventDAL.GetWaitlistById(ss.TransactionId);
                                if (waitlistModel != null && waitlistModel.Id > 0)
                                {
                                    if (waitlistModel.WaitlistStatus == 2) //Seated = 2
                                    {
                                        blockTablesResponse.error_info.error_type = (int)Common.Common.ErrorType.TableAlreadyInUse;
                                        blockTablesResponse.error_info.extra_info = "Table " + eventDAL.GetTableNameById(item) + " is already assigned to a party in the specified time interval.";
                                    }
                                }
                                else if (waitlistModel.PreAssign_Table_Id.Length > 0 && waitlistModel.PreAssign_Table_Id.IndexOf(item.ToString()) > -1)
                                {
                                    blockTablesResponse.error_info.error_type = (int)Common.Common.ErrorType.TableAlreadyInUse;
                                    blockTablesResponse.error_info.extra_info = "Table " + eventDAL.GetTableNameById(item) + " is already preassigned to a party in the specified time interval.";
                                }
                            }
                            else if (ss.TransactionCategory == 2)  //Reservations = 2
                            {
                                ReservationStatusModel reservationDetailModel = new ReservationStatusModel();
                                bool IsAdmin = Convert.ToBoolean((Request.Headers["IsAdmin"]));
                                reservationDetailModel = eventDAL.GetReservationDetailsbyId(ss.TransactionId);
                                if (reservationDetailModel != null && reservationDetailModel.user_id > 0)
                                {
                                    if (reservationDetailModel.seated_status == 1)
                                    {
                                        blockTablesResponse.error_info.error_type = (int)Common.Common.ErrorType.TableAlreadyInUse;
                                        blockTablesResponse.error_info.extra_info = "Table " + eventDAL.GetTableNameById(item) + " is already assigned to a party in the specified time interval.";
                                    }
                                }
                                else if (reservationDetailModel.pre_assign_table_ids.Count > 0 && reservationDetailModel.pre_assign_table_ids.Contains(item))
                                {
                                    blockTablesResponse.error_info.error_type = (int)Common.Common.ErrorType.TableAlreadyInUse;
                                    blockTablesResponse.error_info.extra_info = "Table " + eventDAL.GetTableNameById(item) + " is already preassigned to a party in the specified time interval.";
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(blockTablesResponse.error_info.extra_info))
                        {
                            DateTime startTime = Convert.ToDateTime("1/1/1900");
                            DateTime endTime = Convert.ToDateTime("1/1/1900");
                            string status = string.Empty;

                            int TableBlockedCount = tableStatusGroupDAL.GetTableBlocked(item, Convert.ToDateTime(model.start_date), Convert.ToDateTime(model.end_date), ref startTime, ref endTime, ref status);
                            
                            if (TableBlockedCount == 0)
                            {
                                int id = tableStatusGroupDAL.ForceTableBlocked(item, model.member_id, Convert.ToDateTime(model.start_date), Convert.ToDateTime(model.end_date));
                                if (id > 0)
                                    ids.Add(id);
                            }
                            else
                            {
                                blockTablesResponse.error_info.error_type = (int)Common.Common.ErrorType.TableCoflict;
                                blockTablesResponse.error_info.extra_info = "Table " + eventDAL.GetTableNameById(item) + " is already blocked between " + startTime.ToString() + " and " + endTime.ToString() + ".";
                            }
                        }
                    }
                    if (ids != null && ids.Count > 0)
                    {
                        blockTablesResponse.data.ids = ids;
                        blockTablesResponse.success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().IndexOf("already assigned") > -1)
                {
                    blockTablesResponse.error_info.error_type = (int)Common.Common.ErrorType.TableAlreadyInUse;
                    blockTablesResponse.error_info.extra_info = ex.Message.ToString();
                }
                else
                {
                    blockTablesResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                    blockTablesResponse.error_info.extra_info = Common.Common.InternalServerError;
                    blockTablesResponse.error_info.description = ex.Message.ToString();
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                    logDAL.InsertLog("WebApi", "BlockTables:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, model.member_id);
                }
            }
            return new ObjectResult(blockTablesResponse);
        }

        [Route("tableblockedbymemberid")]
        [HttpGet]
        public IActionResult GetTableBlockedByMemberId(int member_id, DateTime start_date, DateTime end_date)
        {
            TableStatusGroupDAL tableStatusGroupDAL = new TableStatusGroupDAL(Common.Common.ConnectionString);
            var blockTablesByMemberIdResponse = new BlockTablesByMemberIdResponse();

            try
            {
                var model = new List<BlockTables>();
                model = tableStatusGroupDAL.GetTableBlockedByMemberId(member_id, start_date, end_date);

                if (model != null)
                {
                    blockTablesByMemberIdResponse.success = true;
                    blockTablesByMemberIdResponse.data = model;
                }
                else
                {
                    blockTablesByMemberIdResponse.success = true;
                    blockTablesByMemberIdResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    blockTablesByMemberIdResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                blockTablesByMemberIdResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                blockTablesByMemberIdResponse.error_info.extra_info = Common.Common.InternalServerError;
                blockTablesByMemberIdResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTableBlockedByMemberId:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(blockTablesByMemberIdResponse);
        }

        [Route("deleteblocktables")]
        [HttpPost]
        public async Task<IActionResult> DeleteBlockTables([FromBody]DeleteBlockTablesRequest model)
        {
            var tableStatusGroupDAL = new TableStatusGroupDAL(Common.Common.ConnectionString);
            var blockTablesResponse = new BlockTablesResponse();
            try
            {
                List<int> ids = new List<int>();
                foreach (var item in model.ids)
                {
                    bool ret = tableStatusGroupDAL.DeleteBlockTables(item);
                    if (ret)
                        ids.Add(item);
                }
                blockTablesResponse.data.ids = ids;
                blockTablesResponse.success = true;
            }
            catch (Exception ex)
            {
                blockTablesResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                blockTablesResponse.error_info.extra_info = Common.Common.InternalServerError;
                blockTablesResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "DeleteBlockTables:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(blockTablesResponse);
        }

        [Route("tabledetails")]
        [HttpGet]
        public IActionResult GetTableDetails(int table_id, DateTime start_date, DateTime end_date)
        {
            TableStatusGroupDAL tableStatusGroupDAL = new TableStatusGroupDAL(Common.Common.ConnectionString);
            var tableDetailsResponse = new TableDetailsResponse();

            try
            {
                var model = new TableDetails();
                model = tableStatusGroupDAL.GetTableDetailsById(table_id, start_date, end_date);

                if (model != null)
                {
                    tableDetailsResponse.success = true;
                    tableDetailsResponse.data = model;
                }
                else
                {
                    tableDetailsResponse.success = true;
                    tableDetailsResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    tableDetailsResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                tableDetailsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                tableDetailsResponse.error_info.extra_info = Common.Common.InternalServerError;
                tableDetailsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetTableDetails:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(tableDetailsResponse);
        }
    }
}
