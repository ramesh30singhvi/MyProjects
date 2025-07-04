using CellarPassAppAdmin.Shared.Models;
using CellarPassAppAdmin.Shared.Models.ViewModel;
using System.Threading.Tasks;

namespace CellarPassAppAdmin.Client.ViewModels
{
    public interface ICalendarNoteViewModel
    {
        Task<GetCalendarNotesResponse> GetCalendarNotes();
        Task<AddUpdateCalendarNoteResponse> AddUpdateCalendarNote(CalendarNoteModel model);
        Task<GetCalendarNotesResponse> DeleteCalendarNote(int id);
    }
}
