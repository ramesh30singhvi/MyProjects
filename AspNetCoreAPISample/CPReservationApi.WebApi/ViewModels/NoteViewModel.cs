using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class UpdateNoteRequest : AccountNote
    {
        public string contact_id { get; set; }

        public int member_id { get; set; }

    }
}
