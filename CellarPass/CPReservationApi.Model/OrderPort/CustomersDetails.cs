using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class Customer
    {
        public string CustomerUuid { get; set; }
        public int CustomerNumber { get; set; }
        public int CustomerClassId { get; set; }
        public string CustomerType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ResourceUri { get; set; }
        public string AltCustomerNumber { get; set; }
        public string Company { get; set; }
        public string Address2 { get; set; }
    }

    public class Data
    {
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public List<Customer> Customers { get; set; }

        public Data()
        {
            this.Customers = new List<Customer>();
        }
    }

    public class CustomersDetails
    {
        public string Status { get; set; }
        public int RequestId { get; set; }
        public List<object> Messages { get; set; }
        public Data Data { get; set; }
        public CustomersDetails()
        {
            this.Data = new Data();
        }
    }
}
