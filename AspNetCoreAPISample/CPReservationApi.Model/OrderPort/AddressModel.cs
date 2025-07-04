using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class AddressModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
    }
}
