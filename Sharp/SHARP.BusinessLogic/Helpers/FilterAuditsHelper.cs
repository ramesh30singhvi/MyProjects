using System;
using System.Collections.Generic;
using System.Text.Json;
using iText.Html2pdf.Attach.Impl.Layout.Form.Element;
using Newtonsoft.Json;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.DAL.Models;

namespace SHARP.BusinessLogic.Helpers
{
    public class SimpleValue
    {
        public int id { get; set; }
        public String value { get; set; }
    }
	public static class FilterAuditsHelper
	{
		public static int GetMajorInjuryCount(List<Audit> audits, int year, int month) {

            int Total = 0;

            List<Audit> _audits = GetFilteredByMonth(audits, year, month);

            foreach (var audit in _audits)
            {
                
                foreach (var auditFieldValue in audit.AuditFieldValues)
                {
                    if (auditFieldValue.FormField.LabelName.Contains("injury", StringComparison.CurrentCultureIgnoreCase) && auditFieldValue.FormField.LabelName.Contains("type", StringComparison.CurrentCultureIgnoreCase) && auditFieldValue.Value.Contains("major", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Total++;
                    }
                }
            }

            return Total;
		}

        public static int GetByInHouseAcquired(List<Audit> audits, int year, int month)
        {

            int Total = 0;

            List<Audit> _audits = GetFilteredByMonth(audits, year, month);

            foreach (var audit in _audits)
            {

                foreach (var auditFieldValue in audit.AuditFieldValues)
                {
                    if (auditFieldValue.FormField.LabelName.Contains("origin", StringComparison.CurrentCultureIgnoreCase) && auditFieldValue.FormField.LabelName.Contains("wound", StringComparison.CurrentCultureIgnoreCase) && auditFieldValue.Value.Contains("facility", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Total++;
                    }
                }
            }

            return Total;
        }

        

        public static int GetSentToHospitalCount(List<Audit> audits, int year, int month)
        {

            int Total = 0;

            List<Audit> _audits = GetFilteredByMonth(audits, year, month);

            foreach (var audit in _audits)
            {
                foreach (var auditFieldValue in audit.AuditFieldValues)
                {
                    if (auditFieldValue.FormField.LabelName.Contains("hospital", StringComparison.CurrentCultureIgnoreCase) && auditFieldValue.Value.Contains("Yes", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Total++;
                    }
                }
            }

            return Total;
        }

        public static int GetByReHospitalization(List<Audit> audits, int year, int month)
        {

            int Total = 0;

            List<Audit> _audits = GetFilteredByMonth(audits, year, month);

            foreach (var audit in _audits)
            {
                foreach (var auditFieldValue in audit.AuditFieldValues)
                {
                    if (auditFieldValue.FormField.LabelName.Contains("rehospitalized", StringComparison.CurrentCultureIgnoreCase) && auditFieldValue.Value.Contains("Yes", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Total++;
                    }
                }
            }

            return Total;
        }

        public static int GetByDayCount(List<Audit> audits, int year, int month, DayOfWeek dayOfWeek)
        {

            int Total = 0;

            List<Audit> _audits = GetFilteredByMonth(audits, year, month);

            foreach (var audit in _audits)
            {
                foreach (var auditFieldValue in audit.AuditFieldValues)
                {
                    if (auditFieldValue.FormField.LabelName.Contains("Day", StringComparison.CurrentCultureIgnoreCase))
                    {
                        SimpleValue JSONValue = JsonConvert.DeserializeObject<SimpleValue>(auditFieldValue.Value);
                        if (JSONValue.value == dayOfWeek.ToString())
                        {
                            Total++;
                        }
                        
                    }
                }
            }

            return Total;
        }

        public static List<Audit> GetFilteredByMonth(List<Audit> audits, int year, int month)
        {
            DateTime from = new DateTime(year, month, 1);
            DateTime until = from.AddMonths(1);

            List<Audit> filteredAudits = audits.FindAll(audit =>
                audit.IncidentDateFrom >= from &&
                audit.IncidentDateFrom < until
            );

            return filteredAudits;
        }

        public static List<WoundReportDto.ByMonth.ByType> GetByTypes(List<Audit> audits, int year, int month, FormField[] fields)
        {
            List<Audit> _audits = GetFilteredByMonth(audits, year, month);

            List<WoundReportDto.ByMonth.ByType> byTypes = new List<WoundReportDto.ByMonth.ByType>();


            foreach (var field in fields)
            {
                if (field.LabelName.Contains("type", StringComparison.CurrentCultureIgnoreCase))
                {
                    foreach (var item in field.Items)
                    {
                        WoundReportDto.ByMonth.ByType byType = new WoundReportDto.ByMonth.ByType();
                        byType.Name = item.Value;
                        byType.Count = 0;
                        byTypes.Add(byType);
                    }
                }
                
            }


            foreach (var audit in _audits)
            {

                foreach (var auditFieldValue in audit.AuditFieldValues)
                {

                    if (auditFieldValue.FormField.LabelName.Contains("type", StringComparison.CurrentCultureIgnoreCase))
                    {
                       
                        if (auditFieldValue.FormField.FieldType.Name.Contains("Multiselect"))
                        {
                            List<SimpleValue> JSONValues = JsonConvert.DeserializeObject<List<SimpleValue>>(auditFieldValue.Value);

                            foreach (var JSONValue in JSONValues)
                            {
                                var existingType = byTypes.Find(type => type.Name == JSONValue.value);
                                if (existingType == null)
                                {
                                    WoundReportDto.ByMonth.ByType byType = new WoundReportDto.ByMonth.ByType();
                                    byType.Name = JSONValue.value;
                                    byType.Count = 1;
                                    byTypes.Add(byType);
                                }
                                else
                                {
                                    existingType.Count++;
                                }
                            }
                            
                        }
                        else if (auditFieldValue.FormField.FieldType.Name.Contains("Single Select"))
                        {
                            SimpleValue JSONValue = JsonConvert.DeserializeObject<SimpleValue>(auditFieldValue.Value);

                            var existingType = byTypes.Find(type => type.Name == JSONValue.value);
                            if (existingType == null)
                            {
                                WoundReportDto.ByMonth.ByType byType = new WoundReportDto.ByMonth.ByType();
                                byType.Name = JSONValue.value;
                                byType.Count = 1;
                                byTypes.Add(byType);
                            }
                            else
                            {
                                existingType.Count++;
                            }
                        }
                        else
                        {
                            var existingType = byTypes.Find(type => type.Name == auditFieldValue.Value);
                            if (existingType == null)
                            {
                                WoundReportDto.ByMonth.ByType byType = new WoundReportDto.ByMonth.ByType();
                                byType.Name = auditFieldValue.Value;
                                byType.Count = 1;
                                byTypes.Add(byType);
                            }
                            else
                            {
                                existingType.Count++;
                            }
                        }
                    }
                    
                }

            }
            return byTypes;
        }

        public static List<ByShift> GetByShift(List<Audit> audits) {
            List<ByShift> shifts = new List<ByShift>();

            foreach (var audit in audits)
            {
                ByShift shift = new ByShift();

                foreach (var auditFieldValue in audit.AuditFieldValues)
                {
                    SimpleValue JSONValue = JsonConvert.DeserializeObject<SimpleValue>(auditFieldValue.Value);

                    if (auditFieldValue.FormField.LabelName.Contains("Shift", StringComparison.CurrentCultureIgnoreCase))
                    {
                        foreach (var item in auditFieldValue.FormField.Items)
                        {
                            if (shifts.Find(place => place.Name == item.Value) == null)
                            {
                                ByShift byShift = new ByShift();
                                byShift.Name = item.Value;
                                shifts.Add(byShift);
                            }
                        }

                        var existingShift = shifts.Find(shift => shift.Name == JSONValue.value);
                        if (existingShift == null)
                        {
                            shift.Name = JSONValue.value;
                            shifts.Add(shift);
                        }
                        else
                        {
                            shift = existingShift;
                        }
                    }
                }

                foreach (var auditFieldValue in audit.AuditFieldValues)
                {
                    SimpleValue JSONValue = JsonConvert.DeserializeObject<SimpleValue>(auditFieldValue.Value);

                    if (auditFieldValue.FormField.LabelName.Contains("Time", StringComparison.CurrentCultureIgnoreCase))
                    {
                        foreach (var item in auditFieldValue.FormField.Items)
                        {
                            if (shift.ByTime.Find(place => place.Name == item.Value) == null)
                            {
                                ByTime byPlace = new ByTime();
                                byPlace.Name = item.Value;
                                byPlace.Count = 0;
                                shift.ByTime.Add(byPlace);
                            }
                        }

                        ByTime existingTime = shift.ByTime.Find(time => time.Name == JSONValue.value);
                        if (existingTime == null)
                        {
                            ByTime byTime = new ByTime();
                            byTime.Count = 1;
                            byTime.Name = JSONValue.value;
                            shift.ByTime.Add(byTime);
                        }
                        else
                        {
                            existingTime.Count++;
                        }
                        
                    }
                }

            }

            return shifts;
            
        }

        public static List<ByPlace> GeyByPlace(List<Audit> audits)
        {
            List<ByPlace> places = new List<ByPlace>();

            foreach (var audit in audits)
            {

                foreach (var auditFieldValue in audit.AuditFieldValues)
                {
                    SimpleValue JSONValue = JsonConvert.DeserializeObject<SimpleValue>(auditFieldValue.Value);

                    if (auditFieldValue.FormField.LabelName.Contains("Place", StringComparison.CurrentCultureIgnoreCase))
                    {
                        foreach (var item in auditFieldValue.FormField.Items)
                        {
                            if (places.Find(place => place.Name == item.Value) == null)
                            {
                                ByPlace byPlace = new ByPlace();
                                byPlace.Name = item.Value;
                                byPlace.Count = 0;
                                places.Add(byPlace);
                            }
                        }

                        var existingPlace = places.Find(place => place.Name == JSONValue.value);
                        if (existingPlace == null)
                        {
                            ByPlace byPlace = new ByPlace();
                            byPlace.Name = JSONValue.value;
                            byPlace.Count = 1;
                            places.Add(byPlace);
                        }
                        else
                        {
                            existingPlace.Count++;
                        }
                    }
                }

            }

            return places;

        }

        public static List<ByActivity> GetByActivity(List<Audit> audits)
        {
            List<ByActivity> activities = new List<ByActivity>();

            foreach (var audit in audits)
            {
                
                foreach (var auditFieldValue in audit.AuditFieldValues)
                {
                    SimpleValue JSONValue = JsonConvert.DeserializeObject<SimpleValue>(auditFieldValue.Value);

                    if (auditFieldValue.FormField.LabelName.Contains("Activity", StringComparison.CurrentCultureIgnoreCase))
                    {
                        foreach (var item in auditFieldValue.FormField.Items)
                        {
                            if (activities.Find(place => place.Name == item.Value) == null)
                            {
                                ByActivity byPlace = new ByActivity();
                                byPlace.Name = item.Value;
                                byPlace.Count = 0;
                                activities.Add(byPlace);
                            }
                        }

                        var existingActivity = activities.Find(place => place.Name == JSONValue.value);
                        if (existingActivity == null)
                        {
                            ByActivity byPlace = new ByActivity();
                            byPlace.Name = JSONValue.value;
                            byPlace.Count = 1;
                            activities.Add(byPlace);
                        }
                        else
                        {
                            existingActivity.Count++;
                        }
                    }
                }

            }

            return activities;

        }
    }

}

