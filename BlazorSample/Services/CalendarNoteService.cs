using CellarPassAppAdmin.Client.Exceptions;
using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.Services
{
    public class CalendarNoteService : ICalendarNoteService
    {
        private readonly IApiManager _apiManager;
        private readonly IConfiguration _configuration;
        private static string _calendarBaseUrl;
        public CalendarNoteService(IApiManager apiManager, IConfiguration configuration)
        {
            _apiManager = apiManager;
            _configuration = configuration;
            _calendarBaseUrl = _configuration["App:CalendarApiUrl"];
        }
        public async Task<GetCalendarNotesResponse> GetCalendarNotes()
        {
            try
            {
                return await _apiManager.GetAsync<GetCalendarNotesResponse>(string.Format("{0}CalendarNote/get-calendar-notes", _calendarBaseUrl));
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new GetCalendarNotesResponse();
            }
        }

        public async Task<AddUpdateCalendarNoteResponse> AddUpdateCalendarNote(CalendarNoteModel model)
        {
            try
            {
                return await _apiManager.PostAsync<CalendarNoteModel, AddUpdateCalendarNoteResponse>(string.Format("{0}CalendarNote/add-update-calendar-note", _calendarBaseUrl), model);
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new AddUpdateCalendarNoteResponse();
            }
        }

        public async Task<GetCalendarNotesResponse> DeleteCalendarNote(int id)
        {
            try
            {
                return await _apiManager.DeleteAsync<GetCalendarNotesResponse>(string.Format("{0}CalendarNote/delete-calendar-note/{1}", _calendarBaseUrl, id));
            }
            catch (HttpRequestExceptionEx ex)
            {
                Debug.WriteLine(ex.HttpCode);
                return new GetCalendarNotesResponse();
            }
        }
    }
}
