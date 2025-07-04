using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Report
{
    public class PCCNotesDto
    {
        public int PatientNotesId {get;set;}
        public string FacilityName { get; set; }
        public int FacilityId { get; set; }

        public string PatientId { get; set; }
        public string PatientName { get; set; }

        public string DateTimeNotes { get; set; }
        public string Date { get; set; }

        public string Time { get; set; }

        public string PatientNotes { get; set; }

        public int ReportId { get; set; }

        public string ToJsonString()
        {
            JObject json = new JObject();
            json["facilityName"] =  FacilityName;
            json["patientNotes"] = PatientNotes;
            json["dateTimeNotes"] =  DateTimeNotes;
            json["date"] = Date;
            json["time"] = Time;
            json["patientName"]=  PatientName;
            json["patientId"] =  PatientId;
            json["facilityId"] =  FacilityId;
            json["reportId"] = ReportId;
            return json.ToString();
        }

        protected static PCCNotesDto  FromJson(JObject jObject)
        {
            var ins = new PCCNotesDto();
            if (jObject.ContainsKey("facilityName"))
                ins.FacilityName = (string)jObject["facilityName"];
            if (jObject.ContainsKey("patientNotes"))
                ins.PatientNotes = (string)jObject["patientNotes"];
            if (jObject.ContainsKey("datetimeNotes"))
                ins.DateTimeNotes = (string)jObject["dateTimeNotes"];
            if (jObject.ContainsKey("date"))
                ins.Date = (string)jObject["date"];
            if (jObject.ContainsKey("time"))
                ins.Time = (string)jObject["time"];
            if (jObject.ContainsKey("patientName"))
                ins.PatientName = (string)jObject["patientName"];
            if (jObject.ContainsKey("patientId"))
                ins.PatientId = (string)jObject["patientId"];
            if (jObject.ContainsKey("reportId"))
                ins.ReportId = (int)jObject["reportId"];
            if (jObject.ContainsKey("facilityId"))
                ins.FacilityId = (int)jObject["facilityId"];
            return ins;
        }
    }
}
