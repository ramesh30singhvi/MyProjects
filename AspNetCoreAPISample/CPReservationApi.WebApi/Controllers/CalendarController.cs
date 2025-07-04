using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CPReservationApi.WebApi.ViewModels;
using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.Services;
using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Cryptography;

namespace CPReservationApi.WebApi.Controllers
{
    [Route("api/calendar")]
    public class CalendarController : BaseController
    {
        public static IOptions<AppSettings> _appSetting;
        public CalendarController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }
        


        [Route("notes")]
        [HttpGet]
        public IActionResult GetCalendarNotes(int member_id, DateTime note_date)
        {
            EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

            var calendarResponse = new CalendarResponse();

            try
            {
                var model = new List<CalendarModel>();

                model = eventDAL.GetCalendarNotes(member_id, note_date);

                if (model != null)
                {
                    calendarResponse.success = true;
                    calendarResponse.data = model;
                }
                else
                {
                    calendarResponse.success = true;
                    calendarResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    calendarResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                calendarResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                calendarResponse.error_info.extra_info = Common.Common.InternalServerError;
                calendarResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetCalendarNotes:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(calendarResponse);
        }

        [Route("createupdatenote")]
        [HttpPost]
        public IActionResult CreateUpdateNote([FromBody] NoteRequest model)
        {
            var noteResponse = new NoteResponse();
            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);

                int Id = eventDAL.CreateUser(model.id, model.member_id, Convert.ToDateTime(model.start_date).Date,Convert.ToDateTime(model.start_date).TimeOfDay, Convert.ToDateTime(model.end_date).TimeOfDay, model.title, model.text);
                if (Id > 0)
                {
                    noteResponse.success = true;
                    noteResponse.data.id = Id;

                    var notificationDAL = new NotificationDAL(Common.Common.ConnectionString_tablepro);
                    var model1 = new CreateDeltaRequest();
                    model1.item_id = Id;
                    model1.item_type = (int)Common.ItemType.Serversessions;
                    model1.location_id = eventDAL.GetCalendarNoteLocationIdByWineryID(model.member_id);
                    model1.member_id = model.member_id;
                    model1.action_date =Convert.ToDateTime(model.start_date).Date;
                    notificationDAL.SaveDelta(model1);
                }
                else
                {
                    noteResponse.success = false;
                    noteResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                }
            }
            catch (Exception ex)
            {
                noteResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                noteResponse.error_info.extra_info = Common.Common.InternalServerError;
                noteResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "CreateUpdateNote:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,model.member_id);
            }
            return new ObjectResult(noteResponse);
        }
    }
}