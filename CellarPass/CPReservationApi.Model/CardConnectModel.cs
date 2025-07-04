using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
          public class CardConnectModel
          {
                    public string Merchid { get; set; }
                    public string Accttype { get; set; }
                    public string Account { get; set; }
                    public string Expiry { get; set; }
                    public string Cvv2 { get; set; }
                    public string Amount { get; set; }
                    public string Currency { get; set; }
                    public string Orderid { get; set; }
                    public string Name { get; set; }
                    public string Address { get; set; }
                    public string Street { get; set; }
                    public string City { get; set; }
                    public string Region { get; set; }
                    public string Country { get; set; }
                    public string Postal { get; set; }
                    public string Tokenize { get; set; }
                    public string Capture { get; set; }
                    public string Defaultacct { get; set; }

                    public string Profile { get; set; }

                    public string retref { get; set; }

                    public string Ponumber { get; set; } //Purchase Order Numer
                    public string Taxamnt { get; set; } //Tax Amount
                    public string Shipfromzip { get; set; }
                    public string Shiptozip { get; set; }
                    public string Shiptocountry { get; set; }

                    //Line items
                    public string Lineno { get; set; } 
                    public string Material { get; set; }
                    public string Description { get; set; }
                    public string Upc { get; set; }
                    public string Quantity { get; set; }
                    public string Uom { get; set; } //Value is "each"
                    public string Unitcost { get; set; }

                    // Authorization Code from auth response
                    public string authcode { get; set; }
                    public string Invoiceid { get; set; }
                    public string Orderdate { get; set; }
                    public string Frtamnt { get; set; }  // Total Order Freight Amount
                    public string Dutyamnt { get; set; }// Total Duty Amount

                    public string Accountid { get; set; }
                    public string Profileid { get; set; }

                    //responses
                    public string respcode { get; set;}
                    public string commcard { get; set;}
                    public string respstat { get; set; }
                    public string token { get; set; }
                    public string respproc { get; set; }
                    public string resptext { get; set; }
          }
}
