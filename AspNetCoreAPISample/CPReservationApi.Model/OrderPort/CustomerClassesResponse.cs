using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class CustomerClassesResponse :ResponseBase
    {
        public List<CustomerClass> Data { get; set; }

        public CustomerClassesResponse()
        {
            this.Data = new List<CustomerClass>();
        }
    }

    public class CustomerClass
    {
        public int Id { get; set; }
        public string ClassName { get; set; }
        public string ClassType { get; set; }
        public double DiscountRate { get; set; }
        public double CaseDiscountRate { get; set; }
    }
}
