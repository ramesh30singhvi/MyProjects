using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailChimp.Net;
using CPReservationApi.Model;
using CPReservationApi.Common;
using CPReservationApi.DAL;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace CPReservationApi.WebApi.Services
{
    public class MailChimpAPI
    {
        private string _apiKey = "";
        private string _storeId = "";
        private string _campaignId = "";
        private string _reservationstag = "";
        private string _ticketingstag = "";
        private string _rsvpListId = "";
        private string _ticketListId = "";
        private MailChimpManager _MCManager = null;
        public MailChimpAPI(string apiKey, string storeId, string listName, string mcreservationstag, string mcticketingstag, string mcrsvpListId, string mcticketListId)
        {
            this._apiKey = apiKey;
            this._storeId = storeId;
            this._campaignId = listName;
            this._reservationstag = mcreservationstag;
            this._ticketingstag = mcticketingstag;
            this._rsvpListId = mcrsvpListId;
            this._ticketListId = mcticketListId;
            _MCManager = new MailChimpManager(_apiKey);
        }

        public async Task<int> CheckAndCreateTag(string tagName, string email)
        {
            int id = 0;
            try
            {
                if (_MCManager != null && !string.IsNullOrWhiteSpace(_storeId))
                {
                    MailChimp.Net.Models.Tag tag = new MailChimp.Net.Models.Tag();

                    var memberTag = await Task.Run(() => _MCManager.Members.GetTagsAsync(_storeId, StringHelpers.GetMd5Hash(email.ToLower())));
                    //Task<IEnumerable<MailChimp.Net.Models.MemberTag>> memberTag = _MCManager.Members.GetTagsAsync(_storeId, StringHelpers.GetMd5Hash(email.ToLower()));

                    try
                    {
                        if (memberTag != null && memberTag != null)
                        {
                            foreach (var item in memberTag)
                            {
                                if (item.Name == tagName)
                                {
                                    id = item.Id;
                                    break;
                                }
                            }
                        }
                    }
                    catch { }


                    if (id == 0)
                    {
                        MailChimp.Net.Models.Tags tags = new MailChimp.Net.Models.Tags();
                        List<MailChimp.Net.Models.Tag> listtag = new List<MailChimp.Net.Models.Tag>();

                        tag.Name = tagName;
                        tag.Status = "active";
                        listtag.Add(tag);

                        tags.MemberTags = listtag;

                        await Task.Run(() => _MCManager.Members.AddTagsAsync(_storeId, StringHelpers.GetMd5Hash(email.ToLower()), tags));
                        //var ret = _MCManager.Members.AddTagsAsync(_storeId, StringHelpers.GetMd5Hash(email.ToLower()), tags);

                        var memberTag1 = await Task.Run(() => _MCManager.Members.GetTagsAsync(_storeId, StringHelpers.GetMd5Hash(email.ToLower())));

                        if (memberTag1 != null && memberTag1 != null)
                        {
                            foreach (var item in memberTag1)
                            {
                                if (item.Name == tagName)
                                {
                                    id = item.Id;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            return id;
        }

        public async Task<int> CheckAndCreateMember(string email, bool isTicket = false,int customertype = 0,bool Subscribed = true)
        {
            string tagName = "";
            string listId = "";
            string customertypetagName = "";

            if (!isTicket)
            {
                tagName = _reservationstag;
                listId = _rsvpListId;

                if (customertype == 0)
                    customertypetagName = "cp-consumer";
                else if (customertype == 1)
                    customertypetagName = "cp-admin";
                else if (customertype == 2)
                    customertypetagName = "cp-concierge";
            }
            else
            {
                tagName = _ticketingstag;
                listId = _ticketListId;
            }

            int id = 0;
            if (_MCManager != null && !string.IsNullOrWhiteSpace(listId))
            {
                try
                {
                    MailChimp.Net.Models.Member member = new MailChimp.Net.Models.Member();

                    member.EmailAddress = email;

                    if (Subscribed)
                        member.Status = MailChimp.Net.Models.Status.Subscribed;
                    else
                        member.Status = MailChimp.Net.Models.Status.Unsubscribed;

                    var tags = await _MCManager.Members.GetTagsAsync(listId, email);

                    if (tags != null && tags.Count() > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(tagName))
                        {
                            var tag = tags.Where(t => t.Name.Equals(tagName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                            if (tag == null || tag.Id == 0)
                            {
                                var newTaags = new MailChimp.Net.Models.Tags();
                                newTaags.MemberTags.Add(new MailChimp.Net.Models.Tag
                                {
                                    Name = tagName,
                                    Status = "active"
                                });
                                await _MCManager.Members.AddTagsAsync(listId, email, newTaags);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(customertypetagName))
                        {
                            var tag = tags.Where(t => t.Name.Equals(customertypetagName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                            if (tag == null || tag.Id == 0)
                            {
                                var newTaags = new MailChimp.Net.Models.Tags();
                                newTaags.MemberTags.Add(new MailChimp.Net.Models.Tag
                                {
                                    Name = customertypetagName,
                                    Status = "active"
                                });
                                await _MCManager.Members.AddTagsAsync(listId, email, newTaags);
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(tagName))
                        {
                            var newTaags = new MailChimp.Net.Models.Tags();
                            newTaags.MemberTags.Add(new MailChimp.Net.Models.Tag
                            {
                                Name = tagName,
                                Status = "active"
                            });
                            await _MCManager.Members.AddTagsAsync(listId, email, newTaags);
                        }

                        if (!string.IsNullOrWhiteSpace(customertypetagName))
                        {
                            var newTaags = new MailChimp.Net.Models.Tags();
                            newTaags.MemberTags.Add(new MailChimp.Net.Models.Tag
                            {
                                Name = customertypetagName,
                                Status = "active"
                            });
                            await _MCManager.Members.AddTagsAsync(listId, email, newTaags);
                        }
                    }

                    MailChimp.Net.Models.Member r = await _MCManager.Members.AddOrUpdateAsync(listId, member);

                    id = 1;
                }
                catch (Exception ex)
                {
                    string s = ex.Message;
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("WebApi", "CheckAndCreateMember:  " + ex.Message.ToString(), "", 1, 0);
                }
            }

            return id;
        }

        public async Task<bool> CheckMemberSubscribed(string email)
        {
            string listId = _rsvpListId;
            bool isSubscribed = false;
            try
            {
                var Members = await _MCManager.Members.GetAllAsync(listId);

                if (Members != null && Members.Count() > 0)
                {
                    var Member = Members.Where(t => t.EmailAddress.Equals(email, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                    if (Member.Status == MailChimp.Net.Models.Status.Subscribed)
                        isSubscribed = true;
                }
            }
            catch { }

            return isSubscribed;
        }

        public async Task<int> CheckAndCreateList(string email)
        {
            int id = 0;
            if (_MCManager != null && !string.IsNullOrWhiteSpace(_storeId))
            {
                try
                {
                    MailChimp.Net.Models.Member member = new MailChimp.Net.Models.Member();

                    member.EmailAddress = email;
                    member.Status = MailChimp.Net.Models.Status.Subscribed;
                    member.StatusIfNew = MailChimp.Net.Models.Status.Subscribed;
                    
                    MailChimp.Net.Models.Member r = await _MCManager.Members.AddOrUpdateAsync(_storeId, member);
                }
                catch (Exception ex)
                {
                    string s = ex.Message;
                    LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                    logDAL.InsertLog("WebApi", "CheckAndCreateList:  " + ex.Message.ToString(), "", 1, 0);
                }
            }

            return id;
        }

        public List<MailChimpListModel> GetAllList()
        {
            List<MailChimpListModel> ret = new List<MailChimpListModel>();
            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            if (_MCManager != null)
            {
                try
                {
                    var lists = _MCManager.Lists.GetAllAsync(new MailChimp.Net.Core.ListRequest
                    {
                        Limit = 500
                    }).Result;

                    ret = lists.Select(l => new MailChimpListModel { id = l.Id, name = l.Name }).OrderBy(l => l.name).ToList();
                }
                catch (Exception ex)
                {
                    logDAL.InsertLog("WebApi", "MailChimp API::Error in GetAllList: " + ex.Message.ToString(), "", 1, 0);
                }
            }
            return ret;
        }

        public MailChimp.Net.Models.Customer CheckAndCreateCustomer(string userId, string firstName, string lastName, string email,bool OptInStatus = true)
        {
            if (_MCManager != null && !string.IsNullOrWhiteSpace(_storeId))
            {
                var customers = _MCManager.ECommerceStores.Customers(_storeId);
                MailChimp.Net.Models.Customer cust = null;
                bool foundCustomer = false;
                try
                {
                    cust = Task.Run(() => customers.GetAsync(userId)).Result;
                    foundCustomer = true;
                }
                catch {
                    foundCustomer = false;
                }
                if (!foundCustomer || cust == null || string.IsNullOrWhiteSpace(cust.Id))
                {
                    //create customer
                    var result = Task.Run(() => customers.AddAsync(new MailChimp.Net.Models.Customer
                    {
                        EmailAddress = email,
                        Id = userId,
                        FirstName = firstName,
                        LastName = lastName,
                        OptInStatus = OptInStatus,
                        OrdersCount = 0,
                        TotalSpent = 0
                    })).Result;

                    return result;
                }
                else
                {
                    return cust;
                }
            }
            else
            {

                return null;
            }
        }

        public bool CheckAndCreateProduct(string productId, string productName, string vendor, string productVariantId, string productVariantTitle)
        {
            bool isSuccess = false;
            bool createProduct = false;
            bool createProductVariant = false;
            if (_MCManager != null && !string.IsNullOrWhiteSpace(_storeId))
            {
                var products = _MCManager.ECommerceStores.Products(_storeId);
                MailChimp.Net.Models.Product product = null;
                try
                {
                    product = Task.Run(() => products.GetAsync(productId)).Result;
                }
                catch { }
                if (product != null && !string.IsNullOrWhiteSpace(product.Id))
                {
                    var prodVariant = product.Variants.Where(p => p.Id.Equals(productVariantId)).FirstOrDefault();

                    if (prodVariant == null)
                    {
                        createProductVariant = true;
                    }
                }
                else
                {
                    createProduct = true;
                    createProductVariant = true;
                }

                if (createProduct || createProductVariant)
                {

                    //create product
                    var productVariant = new MailChimp.Net.Models.Variant
                    {
                        Id = productVariantId,
                        Title = productVariantTitle
                    };

                    if (createProduct && createProductVariant)
                    {
                        //both product and product variant not found, create them
                        List<MailChimp.Net.Models.Variant> lstVariants = new List<MailChimp.Net.Models.Variant>();
                        lstVariants.Add(productVariant);
                        var newProduct = new MailChimp.Net.Models.Product
                        {
                            Title = productName,
                            Id = productId,
                            Variants = lstVariants,
                            Vendor = vendor
                        };
                        var result = Task.Run(() => products.AddAsync(newProduct)).Result;

                        isSuccess = (!string.IsNullOrWhiteSpace(result.Id));
                    }
                    else
                    {
                        //only create product variant
                        product.Variants.Add(productVariant);
                        var result = Task.Run(() => products.UpdateAsync(productId, product)).Result;

                        isSuccess = (!string.IsNullOrWhiteSpace(result.Id));
                    }

                }
                else {
                    isSuccess = true;
                }
            }

            return isSuccess;
        }

        public bool CreateOrder(string orderId, string productId, string productTitle, string productVariantId, string productVariantTitle, int quantity, decimal orderTotal, DateTime orderDate, MailChimp.Net.Models.Customer customer)
        {
            bool isSuccess = false;
            if (_MCManager != null && !string.IsNullOrWhiteSpace(_storeId))
            {
                var orders = _MCManager.ECommerceStores.Orders(_storeId);

                List<MailChimp.Net.Models.Line> productLines = new List<MailChimp.Net.Models.Line>();
                productLines.Add(new MailChimp.Net.Models.Line {
                    ProductId = productId,
                    ProductVariantId = productVariantId,
                    ProductVariantTitle = productVariantTitle,
                    Price = orderTotal,
                    ProductTitle= productTitle,
                    Id = orderId,
                    Quantity = quantity,
                });
                var order = new MailChimp.Net.Models.Order
                {
                    Id = orderId,
                    CurrencyCode = MailChimp.Net.Core.CurrencyCode.USD,
                    OrderTotal = orderTotal,
                    Lines = productLines,
                    ProcessedAtForeign = orderDate.ToString("MM/dd/yyyy"),
                    Customer = customer
                };

                var result = Task.Run(() => orders.AddAsync(order)).Result;

                isSuccess = (result != null && !string.IsNullOrWhiteSpace(result.Id));
               
            }

            return isSuccess;
        }


        public bool CreateRSVPOrder(ReservationDetailModel rsvp)
        {
            bool isSuccess = true;

            if (rsvp != null)
            {

                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
                string rsvpJSON = Newtonsoft.Json.JsonConvert.SerializeObject(rsvp);
                try
                {
                    var cust = CheckAndCreateCustomer(rsvp.user_detail.user_id.ToString(), rsvp.user_detail.first_name, rsvp.user_detail.last_name, rsvp.user_detail.email);

                    if (cust != null && !string.IsNullOrWhiteSpace(cust.Id))
                    {
                        //SettingsDAL settingsDAL = new SettingsDAL(Common.Common.ConnectionString);

                        //List<Settings.Setting> settingsGroup = settingsDAL.GetSettingGroup(rsvp.member_id, (int)Common.Common.SettingGroup.mailchimp);
                        //string mcreservationstag = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_reservationstag);
                        //string mcCampaign = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_listname);
                        // string mcrsvpListId = Settings.GetStrValue(settingsGroup, Common.Common.SettingKey.mailchimp_rsvplistid);

                        //Upserted [[EmailAddress] added to MailChimp [[ListName]] on [{DateTime]]
                        
                        string productName = String.Format("{0} {1}", rsvp.event_start_date.ToShortDateString(), rsvp.event_start_date.ToShortTimeString());
                        bool isProductCreated = CheckAndCreateProduct("R" + rsvp.event_id.ToString(), "RSVP - " + rsvp.event_name, "CP: Reservations", "R" + rsvp.slot_id.ToString(), "RSVP - " + productName);

                        if (isProductCreated)
                        {
                            isSuccess = CreateOrder(rsvp.booking_code, "R" + rsvp.event_id.ToString(), "RSVP - " + rsvp.event_name, "R" + rsvp.slot_id.ToString(), "RSVP - " + productName, 1, rsvp.fee_due, rsvp.booking_date, cust);
                        }
                    }

                    UserDAL userDAL = new UserDAL(Common.Common.ConnectionString);
                    int roleId = userDAL.GetUserRoleIdById(rsvp.user_detail.email);

                    if (roleId == 4)
                    {
                        int member = Task.Run(() => CheckAndCreateMember(rsvp.user_detail.email, false, 0)).Result;
                    }
                    else
                    {
                        int member = Task.Run(() => CheckAndCreateMember(rsvp.user_detail.email, false, 1)).Result;
                    }

                    if (rsvp.referral_id > 0)
                    {
                        string ConciergeEmail = userDAL.GetUserEmailById(rsvp.referral_id);

                        int member = Task.Run(() => CheckAndCreateMember(ConciergeEmail, false,2)).Result;
                    }

                    string rsvpNote = "";

                    //string rsvpNote = "Upserted " + rsvp.user_detail.email + " added to MailChimp " + mcCampaign;
                    if (string.IsNullOrEmpty(_reservationstag))
                    {
                        rsvpNote = "Upserted " + rsvp.user_detail.email + " added to MailChimp " + _campaignId;
                    }
                    else
                    {
                        rsvpNote = "Upserted " + rsvp.user_detail.email + " added to MailChimp " + _campaignId + " with Tag of " + _reservationstag;
                    }

                    EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                    eventDAL.ReservationV2StatusNote_Create(rsvp.reservation_id, rsvp.status, rsvp.member_id, "", false, 0, 0, 0, rsvpNote);

                    isSuccess = true;
                }
                catch (Exception ex)
                {
                    logDAL.InsertLog("WebApi", "MailChimp API::Error in Create RSVP Order: " + ex.Message.ToString() + ", data:" + rsvpJSON, "",1,rsvp.member_id);
                    isSuccess = false;
                }
            }
            return isSuccess;
        }


        public bool CreateTicketOrder(TicketOrderModel ticketOrder)
        {
            bool isSuccess = true;
            int member_id = 0;

            LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);
            try

            {
                if (ticketOrder != null)
                {
                    member_id = ticketOrder.member_id;
                    foreach (var tix in ticketOrder.ticket_order_ticket)
                    {
                        var cust = CheckAndCreateCustomer(tix.email, tix.first_name, tix.last_name, tix.email);

                        if (cust != null && !string.IsNullOrWhiteSpace(cust.Id))
                        {
                            string productName = tix.ticket_name;
                            bool isProductCreated = CheckAndCreateProduct(tix.ticket_id.ToString(), productName, "CP: Tickets", ticketOrder.tickets_event_id.ToString(), ticketOrder.event_title);

                            if (isProductCreated)
                            {
                                isSuccess = CreateOrder(ticketOrder.id.ToString(), tix.ticket_id.ToString(), productName, ticketOrder.tickets_event_id.ToString(), ticketOrder.event_title, 1, tix.ticket_price, ticketOrder.order_date, cust);
                            }
                        }

                        int member = Task.Run(() => CheckAndCreateMember(tix.email, true)).Result;
                    }


                }

                isSuccess = true;
            }
            catch (Exception ex)
            {
                logDAL.InsertLog("WebApi", "MailChimp API::Error in CreateTicketOrder: " + ex.Message.ToString(), "", 1, member_id);
                isSuccess = false;
            }

            return isSuccess;
        }

    }
}
