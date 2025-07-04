using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class ConciergeModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string HomePhoneStr { get; set; }
        public int MasterAccountId { get; set; }
    }
}
