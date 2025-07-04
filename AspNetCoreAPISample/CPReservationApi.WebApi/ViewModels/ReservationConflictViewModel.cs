using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CPReservationApi.WebApi.ViewModels
{
    /// <summary>
    /// Reservation Conflict Request Model
    /// </summary>
    public class ReservationConflictRequest
    {
        /// <summary>
        /// For search limiting to specific slot of an event, need to pass both slot Id and slot type (Required)
        /// </summary>
        [Required]
        public int slot_id { get; set; }

        /// <summary>
        /// Slot type of the event slot (Rule=0, Exception=1) (Required)
        /// </summary>
        [Required]
        public int slot_type { get; set; }
        /// <summary>
        /// Guest Count for which reservation needs to be done (Required)
        /// </summary>
        [Required]
        public int no_of_guests { get; set; }

        /// <summary>
        /// Date to be searched for the reservation (Required)
        /// </summary>
        [Required]
        public DateTime req_date { get; set; }

        /// <summary>
        /// Id of User (Required)
        /// </summary>
        [Required]
        public int user_id { get; set; }
        /// <summary>
        /// Id of Member (Required)
        /// </summary>
        [Required]
        public int member_id { get; set; }

        /// <summary>
        /// Id of Reservation (Optional)
        /// </summary>
        public int reservation_id { get; set; } = 0;
    }

    public class ReservationConflictResponse : BaseResponse
    {
        
    }
}
