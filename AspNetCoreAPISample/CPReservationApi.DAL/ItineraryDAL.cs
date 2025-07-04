using CPReservationApi.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Text;
using System.Linq;
using static CPReservationApi.Common.Common;
using Google.Maps;
using Google.Maps.Direction;

namespace CPReservationApi.DAL
{
    public class ItineraryDAL : BaseDataAccess
    {
        public ItineraryDAL(string connectionString) : base(connectionString)
        {

        }

        public int SaveItinerary(ItineraryPlannerRequest reqModel)
        {
            int id = 0;
            bool isNew = false;
            if (reqModel.id == 0)
                isNew = true;

            if (string.IsNullOrWhiteSpace(reqModel.itinerary_guid) && isNew)
                reqModel.itinerary_guid = Guid.NewGuid().ToString();

            var parameterList = new List<SqlParameter>();
            string regionIds = "";
            if (reqModel.regions != null && reqModel.regions.Count > 0)
                regionIds = String.Join(",", reqModel.regions);

            parameterList.Add(GetParameter("@Id", reqModel.id));
            parameterList.Add(GetParameter("@ItineraryGUID", reqModel.itinerary_guid));
            parameterList.Add(GetParameter("@ItineraryName", reqModel.itinerary_name));
            parameterList.Add(GetParameter("@ItineraryStatus", (int)reqModel.itinerary_status));
            parameterList.Add(GetParameter("@UserId", reqModel.user_id));
            parameterList.Add(GetParameter("@ConciergeId", reqModel.concierge_id));
            parameterList.Add(GetParameter("@PartyAdults", reqModel.party_adults));
            parameterList.Add(GetParameter("@PartyChildren", reqModel.party_children));
            parameterList.Add(GetParameter("@PartyKids", reqModel.party_kids));
            parameterList.Add(GetParameter("@ItineraryStartDateTime", reqModel.start_date));
            parameterList.Add(GetParameter("@ItineraryEndDateTime", reqModel.end_date));
            parameterList.Add(GetParameter("@RegionIds", regionIds));


            id = Convert.ToInt32(ExecuteScalar("AddUpdateItinerary", parameterList));

            if (id > 0 && reqModel.items != null && reqModel.items.Count > 0)
            {
                try
                {
                    foreach (ItineraryPlannerItem item in reqModel.items)
                    {
                        if (item.id == 0 && string.IsNullOrWhiteSpace(item.item_guid))
                        {
                            item.item_guid = Guid.NewGuid().ToString();
                        }
                        int itemId = SaveItineraryItem(item, id);
                    }
                }
                catch (Exception ex)
                {
                    if (isNew)
                        DeleteItinerary(id);
                    throw;
                }
            }

            return id;

        }

        public bool DeleteItinerary(int itineraryId)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", itineraryId));

            int ret = ExecuteNonQuery("DeleteItinerary", parameterList);

            return (ret > 0);
        }

        public bool DeleteItineraryItem(int itemId, string itemGUID)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", itemId));
            parameterList.Add(GetParameter("@ItemGUID", itemGUID));

            int ret = ExecuteNonQuery("DeleteItineraryItem", parameterList);

            return (ret > 0);
        }

        public bool UpdateItineraryStatus(int itineraryId, Itinerary_Status status)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", itineraryId));
            parameterList.Add(GetParameter("@Status", (int)status));

            int ret = ExecuteNonQuery("UpdateItineraryStatus", parameterList);

            return (ret > 0);
        }

        public int SaveItineraryItem(ItineraryPlannerItem reqModel, int itineraryId)
        {
            int id = 0;
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@Id", reqModel.id));
            parameterList.Add(GetParameter("@ItemGUID", reqModel.item_guid));
            parameterList.Add(GetParameter("@ItineraryPlannerId ", itineraryId));
            if (reqModel.ota_info != null)
            {
                parameterList.Add(GetParameter("@OtaId", reqModel.ota_info.id));
            }
            else
            {
                parameterList.Add(GetParameter("@OtaId", 1));
            }
            parameterList.Add(GetParameter("@BookingTypeId", reqModel.booking_type_id));
            parameterList.Add(GetParameter("@CellarPassMemberId", reqModel.cellarpass_member_id));
            parameterList.Add(GetParameter("@CellarPassReservationId", reqModel.cellarpass_reservation_id));
            parameterList.Add(GetParameter("@CellarPassTicketOrderId", reqModel.cellarpass_ticket_orderid));
            parameterList.Add(GetParameter("@ItemConfirmation", reqModel.item_confirmation));
            parameterList.Add(GetParameter("@ItemName", reqModel.business_name));
            parameterList.Add(GetParameter("@ItemCity", reqModel.city));
            parameterList.Add(GetParameter("@ItemState", reqModel.state));
            parameterList.Add(GetParameter("@ItemGeoLat", reqModel.latitude));
            parameterList.Add(GetParameter("@ItemGeoLong", reqModel.longitude));
            parameterList.Add(GetParameter("@ItemPartyFirstName", reqModel.party_first_name));
            parameterList.Add(GetParameter("@ItemPartyLastName", reqModel.party_last_name));
            parameterList.Add(GetParameter("@ItemPartySize", reqModel.party_size));
            parameterList.Add(GetParameter("@ItemAmount", reqModel.item_amount));
            parameterList.Add(GetParameter("@ItemNotes ", reqModel.item_notes));
            parameterList.Add(GetParameter("@RegionId", reqModel.regionid));
            parameterList.Add(GetParameter("@ItemStartDateTime", reqModel.item_start_date));
            parameterList.Add(GetParameter("@ItemEndDateTime", reqModel.item_end_date));
            parameterList.Add(GetParameter("@ItemAddress1", reqModel.address1));
            parameterList.Add(GetParameter("@ItemAddress2", reqModel.address2));
            parameterList.Add(GetParameter("@ItemZip", reqModel.zip));
            parameterList.Add(GetParameter("@ItemPhone", reqModel.phone));


            id = Convert.ToInt32(ExecuteScalar("AddUpdateItineraryItem", parameterList));

            return id;

        }


        public int AddReservationToItinerary(int reservationId, int itineraryId, string itemGUID)
        {
            int id = 0;
            var parameterList = new List<SqlParameter>();
            parameterList.Add(GetParameter("@ReservationId", reservationId));
            parameterList.Add(GetParameter("@ItineraryId ", itineraryId));
            parameterList.Add(GetParameter("@ItemGUID", itemGUID));

            id = Convert.ToInt32(ExecuteScalar("AddReservationToItinerary", parameterList));

            return id;

        }

        public List<ItineraryPlannerViewModel> GetItineraryListByUser(int userId)
        {
            List<ItineraryPlannerViewModel> data = new List<ItineraryPlannerViewModel>();

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@userId", userId));

            using (DbDataReader dataReader = GetDataReader("GetItinerariesbyUser", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ItineraryPlannerViewModel itinerary = new ItineraryPlannerViewModel();
                        itinerary.id = Convert.ToInt32(dataReader["Id"]);
                        itinerary.itinerary_guid = Convert.ToString(dataReader["ItineraryGUID"]);
                        itinerary.itinerary_name = Convert.ToString(dataReader["ItineraryName"]);
                        itinerary.itinerary_status = (Itinerary_Status)Convert.ToInt32(dataReader["ItineraryStatus"]);
                        itinerary.user_id = Convert.ToInt32(dataReader["UserId"]);
                        itinerary.concierge_id = Convert.ToInt32(dataReader["ConciergeId"]);
                        itinerary.party_adults = Convert.ToInt32(dataReader["PartyAdults"]);
                        itinerary.party_children = Convert.ToInt32(dataReader["PartyChildren"]);
                        itinerary.party_kids = Convert.ToInt32(dataReader["PartyKids"]);
                        itinerary.items_count = Convert.ToInt32(dataReader["ItemsCount"]);

                        itinerary.start_date = Convert.ToDateTime(dataReader["ItineraryStartDateTime"]).ToString();
                        itinerary.end_date = Convert.ToDateTime(dataReader["ItineraryEndDateTime"]).ToString();
                        string regions = Convert.ToString(dataReader["RegionList"]);
                        if (!string.IsNullOrWhiteSpace(regions))
                        {
                            itinerary.regions = JsonConvert.DeserializeObject<List<ItineraryPlanner_Region>>(regions);
                        }

                        data.Add(itinerary);

                    }
                }
            }

            return data;

        }

        public PassportItineraryResponseModel CreatePassportInventory(AddPassportItinerary request, string apiKey = "",bool DisableTravelTimeRestrictions = false)
        {
            PassportItineraryResponseModel responseModel = null;
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@TicketEventId", request.ticket_event_id));
            parameterList.Add(GetParameter("@ItineraryId", request.itinerary_id));
            parameterList.Add(GetParameter("@UserId", request.user_id));
            parameterList.Add(GetParameter("@Guests", request.guests));
            parameterList.Add(GetParameter("@RequestDate", Convert.ToDateTime(request.request_date)));
            parameterList.Add(GetParameter("@SlotId", request.slot_id));
            parameterList.Add(GetParameter("@SlotType", request.slot_type));

            using (DbDataReader dataReader = GetDataReader("AddPassportItemToItinerary", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        responseModel = new PassportItineraryResponseModel
                        {
                            itinerary_id = Convert.ToInt32(dataReader["ItineraryId"]),
                            itinerary_item_id = Convert.ToInt32(dataReader["ItineraryItemId"]),
                            sort_order = Convert.ToInt32(dataReader["SortOrder"])

                        };
                    }

                    if (responseModel != null && responseModel.sort_order >= 2)
                    {
                        if (dataReader.NextResult())
                        {
                            List<ItineraryPlannerItemSorted> itemsList = new List<ItineraryPlannerItemSorted>();
                            while (dataReader.Read())
                            {
                                itemsList.Add(new ItineraryPlannerItemSorted
                                {
                                    id = Convert.ToInt32(dataReader["ItemId"]),
                                    sort_order = Convert.ToInt32(dataReader["SortOrder"]),
                                    latitude = Convert.ToString(dataReader["ItemGeoLat"]),
                                    longitude = Convert.ToString(dataReader["ItemGeoLong"]),
                                    item_start_date = Convert.ToDateTime(dataReader["ItemStartDateTime"]).ToString()
                                });
                            }

                            if (itemsList.Count > 0)
                            {
                                string travelTime = "", distance = "";

                                if (DisableTravelTimeRestrictions == false)
                                {
                                    string origin = string.Format("{0},{1}", itemsList[responseModel.sort_order - 2].latitude, itemsList[responseModel.sort_order - 2].longitude);
                                    string dest = string.Format("{0},{1}", itemsList[responseModel.sort_order - 1].latitude, itemsList[responseModel.sort_order - 1].longitude);

                                    CalculateDistanceAndDuration(apiKey, origin, dest, Convert.ToDateTime(itemsList[responseModel.sort_order - 1].item_start_date), ref travelTime, ref distance);
                                }
                                

                                responseModel.travel_time_prev_dest = travelTime;
                                responseModel.dist_prev_destination = distance;
                            }
                        }
                    }
                }
            }

            return responseModel;

        }

        public PassportItineraryResponseModel UpdatePassportInventory(UpdatePassportItinerary request, string apiKey = "")
        {
            PassportItineraryResponseModel responseModel = null;
            var parameterList = new List<DbParameter>();

            int TicketEventId = 0;

            parameterList.Add(GetParameter("@ItineraryItemId", request.itinerary_item_id));
            parameterList.Add(GetParameter("@RequestDate", request.request_date));
            parameterList.Add(GetParameter("@SlotId", request.slot_id));
            parameterList.Add(GetParameter("@SlotType", request.slot_type));

            using (DbDataReader dataReader = GetDataReader("UpdatePassportItemToItinerary", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        responseModel = new PassportItineraryResponseModel
                        {
                            itinerary_id = Convert.ToInt32(dataReader["ItineraryId"]),
                            itinerary_item_id = Convert.ToInt32(dataReader["ItineraryItemId"]),
                            sort_order = Convert.ToInt32(dataReader["SortOrder"])

                        };

                        TicketEventId = Convert.ToInt32(dataReader["TicketEventId"]);
                    }

                    if (responseModel != null && responseModel.sort_order >= 2)
                    {
                        if (dataReader.NextResult())
                        {
                            List<ItineraryPlannerItemSorted> itemsList = new List<ItineraryPlannerItemSorted>();
                            while (dataReader.Read())
                            {
                                itemsList.Add(new ItineraryPlannerItemSorted
                                {
                                    id = Convert.ToInt32(dataReader["ItemId"]),
                                    sort_order = Convert.ToInt32(dataReader["SortOrder"]),
                                    latitude = Convert.ToString(dataReader["ItemGeoLat"]),
                                    longitude = Convert.ToString(dataReader["ItemGeoLong"]),
                                    item_start_date = Convert.ToDateTime(dataReader["ItemStartDateTime"]).ToString()
                                });
                            }

                            if (itemsList.Count > 0)
                            {
                                
                                string travelTime = "", distance = "";
                                bool DisableTravelTimeRestrictions = false;

                                if (TicketEventId > 0)
                                {
                                    TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                                    DisableTravelTimeRestrictions = ticketDAL.CheckDisableTravelTimeRestrictionsByEventId(TicketEventId);

                                    if (DisableTravelTimeRestrictions == false)
                                    {
                                        string origin = string.Format("{0},{1}", itemsList[responseModel.sort_order - 2].latitude, itemsList[responseModel.sort_order - 2].longitude);
                                        string dest = string.Format("{0},{1}", itemsList[responseModel.sort_order - 1].latitude, itemsList[responseModel.sort_order - 1].longitude);

                                        CalculateDistanceAndDuration(apiKey, origin, dest, Convert.ToDateTime(itemsList[responseModel.sort_order - 1].item_start_date), ref travelTime, ref distance);
                                    }
                                }

                                responseModel.travel_time_prev_dest = travelTime;
                                responseModel.dist_prev_destination = distance;
                            }
                        }
                    }
                }
            }

            return responseModel;

        }

        public CheckDistanceTravelTimeResponseModel CheckTravelTimeDistance(int itineraryId, int slotId, int slotType, DateTime requestDate, string apiKey = "", int itemId = 0)
        {
            bool DisableTravelTimeRestrictions = false;

            CheckDistanceTravelTimeResponseModel responseModel = new CheckDistanceTravelTimeResponseModel();
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@ItineraryId", itineraryId));
            parameterList.Add(GetParameter("@RequestDate", requestDate));
            parameterList.Add(GetParameter("@SlotId", slotId));
            parameterList.Add(GetParameter("@SlotType", slotType));
            parameterList.Add(GetParameter("@ItineraryItemId", itemId));

            using (DbDataReader dataReader = GetDataReader("GetItineraryIDetailsForNewItem", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    List<ItineraryPlannerItemSorted> lstItems = new List<ItineraryPlannerItemSorted>();
                    while (dataReader.Read())
                    {
                        lstItems.Add(new ItineraryPlannerItemSorted
                        {
                            id = Convert.ToInt32(dataReader["ItemId"]),
                            sort_order = Convert.ToInt32(dataReader["SortOrder"]),
                            latitude = Convert.ToString(dataReader["ItemGeoLat"]),
                            longitude = Convert.ToString(dataReader["ItemGeoLong"]),
                            item_start_date = Convert.ToDateTime(dataReader["ItemStartDateTime"]).ToString(),
                            item_end_date = Convert.ToDateTime(dataReader["ItemEndDateTime"]).ToString()
                        });
                        DisableTravelTimeRestrictions = Convert.ToBoolean(dataReader["DisableTravelTimeRestrictions"]);
                    }

                    if (lstItems != null && lstItems.Count > 0)
                    {
                        //var itemTobeAdded = lstItems.Where(i => i.id == 0).FirstOrDefault();
                        var itemTobeAdded = lstItems.Where(i => i.id == itemId).FirstOrDefault();
                        ItineraryPlannerItemSorted prevItem = null;
                        if (itemTobeAdded != null)
                        {
                            if (itemTobeAdded.sort_order >= 2)
                            {
                                prevItem = lstItems.Where(i => i.sort_order == itemTobeAdded.sort_order - 1).FirstOrDefault();

                                if (prevItem != null)
                                {
                                    var diffSeconds = (Convert.ToDateTime(itemTobeAdded.item_start_date) - Convert.ToDateTime(prevItem.item_end_date)).TotalSeconds;
                                    
                                    string travelTime = "", distance = "";

                                    if (DisableTravelTimeRestrictions == false)
                                    {
                                        string origin = string.Format("{0},{1}", prevItem.latitude, prevItem.longitude);
                                        string dest = string.Format("{0},{1}", itemTobeAdded.latitude, itemTobeAdded.longitude);

                                        long travelTimeSeconds = CalculateDistanceAndDuration(apiKey, origin, dest, Convert.ToDateTime(itemTobeAdded.item_start_date), ref travelTime, ref distance);

                                        responseModel.is_valid = travelTimeSeconds <= long.Parse(diffSeconds.ToString());
                                    }
                                    else
                                        responseModel.is_valid = true;


                                    responseModel.travel_time_prev_dest = travelTime;
                                    responseModel.dist_prev_destination = distance;
                                }
                            }
                            else if (lstItems.Count > 1)
                            {
                                prevItem = itemTobeAdded;

                                itemTobeAdded = lstItems.Where(i => i.sort_order == itemTobeAdded.sort_order + 1).FirstOrDefault();

                                if (prevItem != null)
                                {
                                    var diffSeconds = (Convert.ToDateTime(itemTobeAdded.item_start_date) - Convert.ToDateTime(prevItem.item_end_date)).TotalSeconds;
                                    string travelTime = "", distance = "";

                                    if (DisableTravelTimeRestrictions == false)
                                    {
                                        string origin = string.Format("{0},{1}", prevItem.latitude, prevItem.longitude);
                                        string dest = string.Format("{0},{1}", itemTobeAdded.latitude, itemTobeAdded.longitude);

                                        long travelTimeSeconds = CalculateDistanceAndDuration(apiKey, origin, dest, Convert.ToDateTime(itemTobeAdded.item_start_date), ref travelTime, ref distance);

                                        responseModel.is_valid = travelTimeSeconds <= long.Parse(diffSeconds.ToString());
                                    }
                                    else
                                        responseModel.is_valid = true;


                                    responseModel.travel_time_prev_dest = travelTime;
                                    responseModel.dist_prev_destination = distance;
                                    

                                }
                            }
                        }
                    }
                }
            }

            return responseModel;

        }

        public ItineraryPlannerViewModel GetItineraryDetails(int id = 0, string itinerary_guid = "", string apiKey = "")
        {
            ItineraryPlannerViewModel itinerary = new ItineraryPlannerViewModel();

            int TicketEventId = 0;
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@Id", id));
            parameterList.Add(GetParameter("@ItineraryGUID", itinerary_guid));

            using (DbDataReader dataReader = GetDataReader("GetItineraryDetail", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        itinerary.id = Convert.ToInt32(dataReader["Id"]);
                        itinerary.itinerary_guid = Convert.ToString(dataReader["ItineraryGUID"]);
                        itinerary.itinerary_name = Convert.ToString(dataReader["ItineraryName"]);
                        itinerary.itinerary_status = (Itinerary_Status)Convert.ToInt32(dataReader["ItineraryStatus"]);
                        itinerary.user_id = Convert.ToInt32(dataReader["UserId"]);
                        itinerary.concierge_id = Convert.ToInt32(dataReader["ConciergeId"]);
                        itinerary.party_adults = Convert.ToInt32(dataReader["PartyAdults"]);
                        itinerary.party_children = Convert.ToInt32(dataReader["PartyChildren"]);
                        itinerary.party_kids = Convert.ToInt32(dataReader["PartyKids"]);
                        itinerary.items_count = Convert.ToInt32(dataReader["ItemsCount"]);

                        itinerary.start_date = Convert.ToDateTime(dataReader["ItineraryStartDateTime"]).ToString();
                        itinerary.end_date = Convert.ToDateTime(dataReader["ItineraryEndDateTime"]).ToString();
                        string regions = Convert.ToString(dataReader["RegionList"]);
                        if (!string.IsNullOrWhiteSpace(regions))
                        {
                            itinerary.regions = JsonConvert.DeserializeObject<List<ItineraryPlanner_Region>>(regions);
                        }

                        TicketEventId = Convert.ToInt32(dataReader["TicketEventId"]);

                        string items = Convert.ToString(dataReader["ItemsList"]);
                        if (!string.IsNullOrWhiteSpace(items))
                        {
                            itinerary.items = JsonConvert.DeserializeObject<List<ItineraryPlannerItem>>(items);
                            itinerary.items = itinerary.items.OrderBy(i => i.item_start_date).ToList();

                            if (itinerary.items != null && itinerary.items.Count > 0)
                            {
                                itinerary.items_count = itinerary.items.Count;

                                if (!string.IsNullOrWhiteSpace(apiKey) && itinerary.items_count > 1)
                                {

                                    bool DisableTravelTimeRestrictions = false;

                                    if (TicketEventId > 0)
                                    {
                                        TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                                        DisableTravelTimeRestrictions = ticketDAL.CheckDisableTravelTimeRestrictionsByEventId(TicketEventId);
                                    }

                                    for (int i = 1; i < itinerary.items_count; i++)
                                    {
                                        string travelTime = "", distance = "";
                                        if (DisableTravelTimeRestrictions == false)
                                        {
                                            string origin = string.Format("{0},{1}", itinerary.items[i - 1].latitude, itinerary.items[i - 1].longitude);
                                            string dest = string.Format("{0},{1}", itinerary.items[i].latitude, itinerary.items[i].longitude);


                                            CalculateDistanceAndDuration(apiKey, origin, dest, Convert.ToDateTime(itinerary.items[i].item_start_date), ref travelTime, ref distance);
                                        }
                                        
                                        itinerary.items[i].travel_time = travelTime;
                                        itinerary.items[i].distance = distance;

                                    }
                                }
                            }


                        }

                    }
                }
            }

            return itinerary;

        }

        public ItineraryPlannerViewModel GetItineraryDetailsMyAccount(int id = 0, string itinerary_guid = "", string apiKey = "")
        {
            ItineraryPlannerViewModel itinerary = new ItineraryPlannerViewModel();
            int TicketEventId = 0;
            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@Id", id));
            parameterList.Add(GetParameter("@ItineraryGUID", itinerary_guid));

            using (DbDataReader dataReader = GetDataReader("GetItineraryDetailMyAccount", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        TicketEventId = Convert.ToInt32(dataReader["TicketEventId"]);
                        itinerary.id = Convert.ToInt32(dataReader["Id"]);
                        itinerary.itinerary_guid = Convert.ToString(dataReader["ItineraryGUID"]);
                        itinerary.itinerary_name = Convert.ToString(dataReader["ItineraryName"]);
                        itinerary.itinerary_status = (Itinerary_Status)Convert.ToInt32(dataReader["ItineraryStatus"]);
                        itinerary.user_id = Convert.ToInt32(dataReader["UserId"]);
                        itinerary.concierge_id = Convert.ToInt32(dataReader["ConciergeId"]);
                        itinerary.party_adults = Convert.ToInt32(dataReader["PartyAdults"]);
                        itinerary.party_children = Convert.ToInt32(dataReader["PartyChildren"]);
                        itinerary.party_kids = Convert.ToInt32(dataReader["PartyKids"]);
                        itinerary.items_count = Convert.ToInt32(dataReader["ItemsCount"]);

                        itinerary.start_date = Convert.ToDateTime(dataReader["ItineraryStartDateTime"]).ToString();
                        itinerary.end_date = Convert.ToDateTime(dataReader["ItineraryEndDateTime"]).ToString();
                        string regions = Convert.ToString(dataReader["RegionList"]);
                        if (!string.IsNullOrWhiteSpace(regions))
                        {
                            itinerary.regions = JsonConvert.DeserializeObject<List<ItineraryPlanner_Region>>(regions);
                        }

                        string items = Convert.ToString(dataReader["ItemsList"]);
                        if (!string.IsNullOrWhiteSpace(items))
                        {
                            itinerary.items = JsonConvert.DeserializeObject<List<ItineraryPlannerItem>>(items);
                            itinerary.items = itinerary.items.OrderBy(i => i.item_start_date).ToList();

                            if (itinerary.items != null && itinerary.items.Count > 0)
                            {
                                itinerary.items_count = itinerary.items.Count;

                                if (!string.IsNullOrWhiteSpace(apiKey) && itinerary.items_count > 1)
                                {
                                    bool DisableTravelTimeRestrictions = false;

                                    if (TicketEventId > 0)
                                    {
                                        TicketDAL ticketDAL = new TicketDAL(Common.Common.ConnectionString);
                                        DisableTravelTimeRestrictions = ticketDAL.CheckDisableTravelTimeRestrictionsByEventId(TicketEventId);
                                    }

                                    for (int i = 1; i < itinerary.items_count; i++)
                                    {
                                        string travelTime = "", distance = "";
                                        if (DisableTravelTimeRestrictions == false)
                                        {
                                            string origin = string.Format("{0},{1}", itinerary.items[i - 1].latitude, itinerary.items[i - 1].longitude);
                                            string dest = string.Format("{0},{1}", itinerary.items[i].latitude, itinerary.items[i].longitude);

                                            
                                            CalculateDistanceAndDuration(apiKey, origin, dest, Convert.ToDateTime(itinerary.items[i].item_start_date), ref travelTime, ref distance);
                                        }
                                        
                                        itinerary.items[i].travel_time = travelTime;
                                        itinerary.items[i].distance = distance;

                                    }
                                }
                            }


                        }

                    }
                }
            }

            return itinerary;

        }

        public List<ItineraryUserReservationsModel> GetUserReservationsForItinerary(int userId, DateTime toDate, DateTime fromDate)
        {
            List<ItineraryUserReservationsModel> data = new List<ItineraryUserReservationsModel>();

            var parameterList = new List<DbParameter>();

            parameterList.Add(GetParameter("@userId", userId));
            parameterList.Add(GetParameter("@fromDate", fromDate));
            parameterList.Add(GetParameter("@toDate", toDate));

            using (DbDataReader dataReader = GetDataReader("GetUserReservationsForItinerary", parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ItineraryUserReservationsModel itineraryUserReservation = new ItineraryUserReservationsModel();
                        itineraryUserReservation.reservation_id = Convert.ToInt32(dataReader["reservation_id"]);
                        itineraryUserReservation.member_name = Convert.ToString(dataReader["member_name"]);
                        itineraryUserReservation.event_start_date = Convert.ToDateTime(dataReader["event_start_date"]);
                        itineraryUserReservation.event_end_date = Convert.ToDateTime(dataReader["event_end_date"]);
                        itineraryUserReservation.booking_code = Convert.ToString(dataReader["booking_code"]);
                        itineraryUserReservation.member_business_phone = Convert.ToString(dataReader["member_business_phone"]);
                        itineraryUserReservation.city = Convert.ToString(dataReader["city"]);
                        itineraryUserReservation.state = Convert.ToString(dataReader["state"]);
                        itineraryUserReservation.address1 = Convert.ToString(dataReader["address1"]);

                        data.Add(itineraryUserReservation);
                    }
                }
            }

            return data;

        }

        private long ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return long.Parse((Math.Floor(diff.TotalSeconds).ToString()));
        }

        public long CalculateDistanceAndDuration(string googleAPIKey, string originLocation, string destinationLocation, DateTime arrivalTime, ref string travelTime, ref string distance)
        {
            long ret = 0;
            GoogleSigned.AssignAllServices(new GoogleSigned(googleAPIKey));
            var request = new Google.Maps.Direction.DirectionRequest();
            request.Origin = new Location(originLocation);
            request.Destination = new Location(destinationLocation);
            request.ArrivalTime = ConvertToUnixTimestamp(arrivalTime.AddMinutes(-5));
            request.Alternatives = false;
            var response = new DirectionService().GetResponse(request);
            if (response.Status == ServiceResponseStatus.Ok && response.Routes.Count() > 0)
            {
                var result = response.Routes.First();
                if (result != null && result.Legs.Count() > 0)
                {
                    ret = result.Legs[0].Duration.Value;
                    travelTime = result.Legs[0].Duration.Text;
                    distance = result.Legs[0].Distance.Text;
                }
            }
            return ret;
        }

        public List<ItineraryPlanner_Ota> GetOTAList()
        {
            List<ItineraryPlanner_Ota> otaList = new List<ItineraryPlanner_Ota>();

            using (DbDataReader dataReader = GetDataReader("GetOTAList", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ItineraryPlanner_Ota ota = new ItineraryPlanner_Ota();
                        ota.id = Convert.ToInt32(dataReader["Id"]);
                        ota.ota_account = Convert.ToString(dataReader["OtaAccount"]);
                        ota.ota_name = Convert.ToString(dataReader["OtaName"]);
                        ota.ota_type = Convert.ToInt16(dataReader["OtaType"]);

                        otaList.Add(ota);
                    }
                }
            }

            return otaList;

        }

        public List<ItineraryPlanner_BookingType> GetBookingTypeList()
        {
            List<ItineraryPlanner_BookingType> bookingTypes = new List<ItineraryPlanner_BookingType>();

            using (DbDataReader dataReader = GetDataReader("GetBookingTypeList", null, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ItineraryPlanner_BookingType bookingType = new ItineraryPlanner_BookingType();
                        bookingType.booking_type_id = Convert.ToInt32(dataReader["Id"]);
                        bookingType.booking_type_name = Convert.ToString(dataReader["BookingTypeName"]);

                        bookingTypes.Add(bookingType);
                    }
                }
            }

            return bookingTypes;

        }

    }
}
