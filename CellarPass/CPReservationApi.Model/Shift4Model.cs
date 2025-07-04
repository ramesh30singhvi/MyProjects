using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class Shift4Credential
    {
        public string authToken { get; set; }
        public string clientGuid { get; set; }
    }
    public class Shift4Model
    {
        public Shift4Credential credential { get; set; }
        public string dateTime { get; set; }
        //public result result { get; set; }
    }
    public class TokenAddReq
    {
        public string dateTime { get; set; }
        public CardAuth card { get; set; }
    }
    public class CardAuth
    {
        public string number { get; set; }
        public string expirationDate { get; set; }

        public SecurityCode securityCode { get; set; }
    }
    public class result
    {
        public string dateTime { get; set; }
        public CredentResult credential { get; set; }
        public Server server { get; set; }
        public Card card { get; set; }
    }
    public class CredentResult
    {
        public string accessToken { get; set; }
    }
    public class Card
    {
        public Token token { get; set; }
    }
    public class Token
    {
        public string value { get; set; }
    }
    public class AccessResult
    {
        public List<result> result { get; set; }
    }
    public class Server
    {
        public string name { get; set; }
    }

    //Class for Sale Transaction
    public class SaleClass
    {
        public string dateTime { get; set; }
        public Shift4Amount amount { get; set; }
        public Clerk clerk { get; set; }
        public Shift4Transaction transaction { get; set; }
        public SaleCard card { get; set; }
        public Shift4Customer customer { get; set; }
    }

    public class Shift4Customer
    {
        public string addressLine1 { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string postalCode { get; set; }

    }

    public class Shift4Amount
    {
        public string tax { get; set; }
        public string total { get; set; }
    }

    public class Clerk
    {
        public int numericId { get; set; }
    }

    public class Shift4Transaction
    {
        public string invoice { get; set; }
        public Shift4PurchaseCard purchaseCard { get; set; }
    }

    public class Shift4PurchaseCard
    {
        public string customerReference { get; set; }
        public string destinationPostalCode { get; set; }
        public List<string> productDescriptors { get; set; }
    }
    public class SaleCard
    {
        //public string number { get; set; }
        //public int expirationDate { get; set; }
        public Token token { get; set; }
        public string present { get; set; } = "N";
        public SecurityCode securityCode { get; set; }
    }

    public class SecurityCode
    {
        public string indicator { get; set; } = "1";
        public string value { get; set; }
    }

    public class AccesssaleResult
    {
        public List<saleResult> result { get; set; }
    }



    public class saleResult
    {
        public Shift4TransactionResult transaction { get; set; }
    }
    public class Shift4TransactionResult
    {
        public string authorizationCode { get; set; }
        public string authSource { get; set; }
        public string invoice { get; set; }
        public string responseCode { get; set; }
        public string saleFlag { get; set; }
    }


}
