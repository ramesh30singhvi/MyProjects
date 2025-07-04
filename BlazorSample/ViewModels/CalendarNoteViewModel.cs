using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using CellarPassAppAdmin.Shared.Services;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public class CalendarNoteViewModel : ICalendarNoteViewModel
    {
        private readonly ICalendarNoteService _calendarNoteService;
        public CalendarNoteViewModel(ICalendarNoteService calendarNoteService)
        {
            _calendarNoteService = calendarNoteService;
        }

        public async Task<AddUpdateCalendarNoteResponse> AddUpdateCalendarNote(CalendarNoteModel model)
        {
            return await _calendarNoteService.AddUpdateCalendarNote(model);
        }

        public async Task<GetCalendarNotesResponse> GetCalendarNotes()
        {
            return await _calendarNoteService.GetCalendarNotes();
        }

        public async Task<GetCalendarNotesResponse> DeleteCalendarNote(int id)
        {
            return await _calendarNoteService.DeleteCalendarNote(id);
        }
    }
}
