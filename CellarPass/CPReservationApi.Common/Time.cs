using System;
using System.Collections.Generic;
using System.Text;
using static CPReservationApi.Common.Common;

namespace CPReservationApi.Common
{
    
    public class Time
    {

        public enum DayOfWeek
        {
            Sunday = 0,
            Monday = 1,
            Tuesday = 2,
            Wednesday = 3,
            Thursday = 4,
            Friday = 5,
            Saturday = 6,
            NA = 7
        }

        public static DateTime ToTimeZoneTime(DateTime utcDateTime, TimeZone TimeZone = TimeZone.PacificTimeZone)
        {

            if (utcDateTime == null)
            {
                utcDateTime = System.DateTime.UtcNow;
            }

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(GetTimeZoneId(TimeZone));
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, tzi);

        }

        public static DateTime ToUniversalTime(DateTime dateTime, TimeZone TimeZone = TimeZone.PacificTimeZone)
        {

            if (dateTime == null)
            {
                dateTime = System.DateTime.UtcNow;
            }

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(GetTimeZoneId(TimeZone));
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, tzi);

        }

        public static string GetTimeZoneId(TimeZone timeZone)
        {

            string value = "";
            if (TimeZoneIds.TryGetValue(Convert.ToInt32(timeZone), value))
            {
                return value;
            }
            else
            {
                return "Pacific Standard Time";
            }

        }

        public static string GetTimeZoneAbbreviation(TimeZone timeZone)
        {

            string value = "";

            try
            {
                TimeZoneAbbr abbr = Convert.ToInt32(timeZone);
                value = abbr.ToString;

            }
            catch (Exception ex)
            {
            }

            return value;

        }

        public static string GetOffset(TimeZone timeZone)
        {

            string strOffset = "";

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(CPReservation.BLL.Times.GetTimeZoneId(timeZone));
            TimeSpan offset = tzi.GetUtcOffset(System.DateTime.UtcNow);

            strOffset = offset.ToString;

            return strOffset;

        }

        public static int GetOffsetMinutes(TimeZone timeZone)
        {

            int minutes = 0;

            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(CPReservation.BLL.Times.GetTimeZoneId(timeZone));
            TimeSpan offset = tzi.GetUtcOffset(System.DateTime.UtcNow);

            minutes = offset.TotalMinutes;

            return minutes;

        }

        private static readonly Dictionary<int, string> TimeZoneIds = new Dictionary<int, string> {
        {
            0,
            "Pacific Standard Time"
        },
        {
            1,
            "US Mountain Standard Time"
        },
        {
            2,
            "Eastern Standard Time"
        },
        {
            3,
            "Central Standard Time"
        },
        {
            4,
            "Mountain Standard Time"
        },
        {
            5,
            "Pacific Standard Time"
        },
        {
            6,
            "Hawaiian Standard Time"
        },
        {
            7,
            "Alaskan Standard Time"
        },
        {
            8,
            "UTC-11"
        },
        {
            9,
            "Pacific Standard Time (Mexico)"
        },
        {
            10,
            "Dateline Standard Time"
        },
        {
            11,
            "Mountain Standard Time (Mexico)"
        },
        {
            12,
            "Central America Standard Time"
        },
        {
            13,
            "Central Standard Time (Mexico)"
        },
        {
            14,
            "Canada Central Standard Time"
        },
        {
            15,
            "SA Pacific Standard Time"
        },
        {
            16,
            "Eastern Standard Time (Mexico)"
        },
        {
            17,
            "US Eastern Standard Time"
        },
        {
            18,
            "Venezuela Standard Time"
        },
        {
            19,
            "Paraguay Standard Time"
        },
        {
            20,
            "Atlantic Standard Time"
        },
        {
            21,
            "Central Brazilian Standard Time"
        },
        {
            22,
            "SA Western Standard Time"
        },
        {
            23,
            "Newfoundland Standard Time"
        },
        {
            24,
            "E. South America Standard Time"
        },
        {
            25,
            "Argentina Standard Time"
        },
        {
            26,
            "SA Eastern Standard Time"
        },
        {
            27,
            "Greenland Standard Time"
        },
        {
            28,
            "Montevideo Standard Time"
        },
        {
            29,
            "Bahia Standard Time"
        },
        {
            30,
            "Pacific SA Standard Time"
        },
        {
            31,
            "UTC-02"
        },
        {
            32,
            "Mid-Atlantic Standard Time"
        },
        {
            33,
            "Azores Standard Time"
        },
        {
            34,
            "Cape Verde Standard Time"
        },
        {
            35,
            "Morocco Standard Time"
        },
        {
            36,
            "UTC"
        },
        {
            37,
            "GMT Standard Time"
        },
        {
            38,
            "Greenwich Standard Time"
        },
        {
            39,
            "W. Europe Standard Time"
        },
        {
            40,
            "Central Europe Standard Time"
        },
        {
            41,
            "Romance Standard Time"
        },
        {
            42,
            "Central European Standard Time"
        },
        {
            43,
            "W. Central Africa Standard Time"
        },
        {
            44,
            "Namibia Standard Time"
        },
        {
            45,
            "Jordan Standard Time"
        },
        {
            46,
            "GTB Standard Time"
        },
        {
            47,
            "Middle East Standard Time"
        },
        {
            48,
            "Egypt Standard Time"
        },
        {
            49,
            "Syria Standard Time"
        },
        {
            50,
            "E. Europe Standard Time"
        },
        {
            51,
            "South Africa Standard Time"
        },
        {
            52,
            "FLE Standard Time"
        },
        {
            53,
            "Turkey Standard Time"
        },
        {
            54,
            "Israel Standard Time"
        },
        {
            55,
            "Kaliningrad Standard Time"
        },
        {
            56,
            "Libya Standard Time"
        },
        {
            57,
            "Arabic Standard Time"
        },
        {
            58,
            "Arab Standard Time"
        },
        {
            59,
            "Belarus Standard Time"
        },
        {
            60,
            "Russian Standard Time"
        },
        {
            61,
            "E. Africa Standard Time"
        },
        {
            62,
            "Iran Standard Time"
        },
        {
            63,
            "Arabian Standard Time"
        },
        {
            64,
            "Azerbaijan Standard Time"
        },
        {
            65,
            "Russia Time Zone 3"
        },
        {
            66,
            "Mauritius Standard Time"
        },
        {
            67,
            "Georgian Standard Time"
        },
        {
            68,
            "Caucasus Standard Time"
        },
        {
            69,
            "Afghanistan Standard Time"
        },
        {
            70,
            "West Asia Standard Time"
        },
        {
            71,
            "Ekaterinburg Standard Time"
        },
        {
            72,
            "Pakistan Standard Time"
        },
        {
            73,
            "India Standard Time"
        },
        {
            74,
            "Sri Lanka Standard Time"
        },
        {
            75,
            "Nepal Standard Time"
        },
        {
            76,
            "Central Asia Standard Time"
        },
        {
            77,
            "Bangladesh Standard Time"
        },
        {
            78,
            "N. Central Asia Standard Time"
        },
        {
            79,
            "Myanmar Standard Time"
        },
        {
            80,
            "SE Asia Standard Time"
        },
        {
            81,
            "North Asia Standard Time"
        },
        {
            82,
            "China Standard Time"
        },
        {
            83,
            "North Asia East Standard Time"
        },
        {
            84,
            "Singapore Standard Time"
        },
        {
            85,
            "W. Australia Standard Time"
        },
        {
            86,
            "Taipei Standard Time"
        },
        {
            87,
            "Ulaanbaatar Standard Time"
        },
        {
            88,
            "Tokyo Standard Time"
        },
        {
            89,
            "Korea Standard Time"
        },
        {
            90,
            "Yakutsk Standard Time"
        },
        {
            91,
            "Cen. Australia Standard Time"
        },
        {
            92,
            "AUS Central Standard Time"
        },
        {
            93,
            "E. Australia Standard Time"
        },
        {
            94,
            "AUS Eastern Standard Time"
        },
        {
            95,
            "West Pacific Standard Time"
        },
        {
            96,
            "Tasmania Standard Time"
        },
        {
            97,
            "Magadan Standard Time"
        },
        {
            98,
            "Vladivostok Standard Time"
        },
        {
            99,
            "Russia Time Zone 10"
        },
        {
            100,
            "Central Pacific Standard Time"
        },
        {
            101,
            "Russia Time Zone 11"
        },
        {
            102,
            "New Zealand Standard Time"
        },
        {
            103,
            "UTC+12"
        },
        {
            104,
            "Fiji Standard Time"
        },
        {
            105,
            "Kamchatka Standard Time"
        },
        {
            106,
            "Tonga Standard Time"
        },
        {
            107,
            "Samoa Standard Time"
        },
        {
            108,
            "Line Islands Standard Time"
        }

    };

        //       Public Shared TimeZonesIds As Dictionary(Of String, Integer)

        //       Dim dictionary As New Dictionary(Of String, Integer)
        //       ' Add four entries.
        //dictionary.Add("Dot", 20)
        //dictionary.Add("Net", 1)
        //dictionary.Add("Perls", 10)
        //dictionary.Add("Visual", -1)
        //Public Class BusinessTime

        //    Private _Id As Integer
        //    Public Property Id() As Integer
        //        Get
        //            Return _Id
        //        End Get
        //        Set(ByVal value As Integer)
        //            _Id = value
        //        End Set
        //    End Property

        //    Private _entityId As Integer
        //    Public Property EntityId() As Integer
        //        Get
        //            Return _entityId
        //        End Get
        //        Set(ByVal value As Integer)
        //            _entityId = value
        //        End Set
        //    End Property

        //    Private _entityType As EntityType
        //    Public Property EntityType() As EntityType
        //        Get
        //            Return _entityType
        //        End Get
        //        Set(ByVal value As EntityType)
        //            _entityType = value
        //        End Set
        //    End Property

        //    Private _startTime As DateTime
        //    Public Property StartTime() As DateTime
        //        Get
        //            Return _startTime
        //        End Get
        //        Set(ByVal value As DateTime)
        //            _startTime = value
        //        End Set
        //    End Property

        //    Private _endTime As DateTime
        //    Public Property EndTime() As DateTime
        //        Get
        //            Return _endTime
        //        End Get
        //        Set(ByVal value As DateTime)
        //            _endTime = value
        //        End Set
        //    End Property

        //    Private _dayOfWeek As DayOfWeek
        //    Public Property DayOfWeek() As DayOfWeek
        //        Get
        //            Return _dayOfWeek
        //        End Get
        //        Set(ByVal value As DayOfWeek)
        //            _dayOfWeek = value
        //        End Set
        //    End Property

        //    Private _recurring As Boolean
        //    Public Property Recurring() As Boolean
        //        Get
        //            Return _recurring
        //        End Get
        //        Set(ByVal value As Boolean)
        //            _recurring = value
        //        End Set
        //    End Property

        //    Private _description As String
        //    Public Property Description() As String
        //        Get
        //            Return _description
        //        End Get
        //        Set(ByVal value As String)
        //            _description = value
        //        End Set
        //    End Property


        //End Class


        public class BusinessHours
        {

            private DateTime _startTime;
            public DateTime StartTime
            {
                get { return _startTime; }
                set { _startTime = value; }
            }

            private DateTime _endTime;
            public DateTime EndTime
            {
                get { return _endTime; }
                set { _endTime = value; }
            }

            private DayOfWeek _dayOfWeek;
            public DayOfWeek DayOfWeek
            {
                get { return _dayOfWeek; }
                set { _dayOfWeek = value; }
            }

            private bool _closed;
            public bool Closed
            {
                get { return _closed; }
                set { _closed = value; }
            }

            private System.DateTime _dayDate;
            public System.DateTime DayDate
            {
                get { return _dayDate; }
                set { _dayDate = value; }
            }

        }

        public static bool SaveBusinessHours(int entityId, int entityType, BusinessHours hours)
        {

            CPReservationDataContext db = new CPReservationDataContext();


            try
            {
                bool createNew = false;

                dynamic dbHours = (from t in db.BusinessTimeswhere t.Entity_Id == entityId & t.Entity_Type == entityType & t.DayOfWeek == hours.DayOfWeek & t.Recurring == true).SingleOrDefault;

                if (dbHours == null)
                {
                    createNew = true;
                    dbHours = new Data.BusinessTime();
                }

                if (!hours.Closed)
                {
                    dbHours.Entity_Id = entityId;
                    dbHours.Entity_Type = entityType;
                    dbHours.DayOfWeek = hours.DayOfWeek;
                    dbHours.Recurring = true;
                    dbHours.Description = hours.DayOfWeek.ToString;
                    dbHours.StartTime = (hours.StartTime == null ? System.DateTime.Now : hours.StartTime);
                    dbHours.EndTime = (hours.EndTime == null ? System.DateTime.Now : hours.EndTime);
                }

                //if existing we update else we create
                if (createNew & !hours.Closed)
                {
                    db.BusinessTimes.InsertOnSubmit(dbHours);
                }
                else
                {
                    //If Closed Delete 
                    if (!createNew & hours.Closed == true)
                    {
                        db.BusinessTimes.DeleteOnSubmit(dbHours);
                    }
                }

                db.SubmitChanges();

            }
            catch (Exception ex)
            {
                return false;
            }

            return true;

        }

        public static List<BusinessHours> LoadBusinessHours(int entityId, int entityType)
        {


            List<BusinessHours> listHours = new List<BusinessHours>();

            CPReservation.Data.CPReservationDataContext db = new CPReservation.Data.CPReservationDataContext();

            dynamic dbHours = (from t in db.BusinessTimeswhere t.Entity_Id == entityId & entityType == entityType);


            if ((dbHours != null))
            {
                foreach (void time_loopVariable in dbHours)
                {
                    time = time_loopVariable;
                    BusinessHours hours = new BusinessHours();
                    hours.StartTime = time.StartTime;
                    hours.EndTime = time.EndTime;
                    hours.DayOfWeek = time.DayOfWeek;
                    hours.Closed = false;

                    listHours.Add(hours);
                }

            }

            return listHours;

        }

        public static List<BusinessHours> LoadBusinessHours_7Day(int entityId, int entityType, int dayOffset)
        {

            List<BusinessHours> listHours = new List<BusinessHours>();

            CPReservation.Data.CPReservationDataContext db = new CPReservation.Data.CPReservationDataContext();

            dynamic dbHours = (from t in db.BusinessTimeswhere t.Entity_Id == entityId & entityType == entityType);


            if ((dbHours != null))
            {
                dbHours.ToList();


                for (x = 0; x <= 6; x++)
                {
                    System.DateTime day = System.DateTime.Today.AddDays(dayOffset).Date.AddDays(x);

                    BusinessHours hours = new BusinessHours();
                    hours.DayOfWeek = day.DayOfWeek;
                    hours.DayDate = day;
                    hours.StartTime = dbHours.Where(f => f.DayOfWeek == day.DayOfWeek & f.Recurring == true).Select(f => f.StartTime).FirstOrDefault;
                    hours.EndTime = dbHours.Where(f => f.DayOfWeek == day.DayOfWeek & f.Recurring == true).Select(f => f.EndTime).FirstOrDefault;
                    if (hours.StartTime == null | hours.EndTime == null)
                    {
                        hours.Closed = true;
                    }
                    listHours.Add(hours);

                    //Get Holidays to overide open/closed hours if needed
                    List<Holidays.BusinessHoliday> holidayList = Holidays.GetBusinessHolidayDates(entityId, entityType, day.Year).ToList;

                    if ((holidayList != null))
                    {
                        //Go through hours for the week and see if any holidays fall on those dates and mark them closed.
                        foreach (Times.BusinessHours bh in listHours)
                        {
                            dynamic match = holidayList.Where(f => f.HolidayDate.ToShortDateString == bh.DayDate.ToShortDateString).Select(f => f.Closed).FirstOrDefault;

                            if (match)
                            {
                                bh.Closed = true;
                            }
                        }
                    }

                }

            }

            return listHours;

        }

    }
}
