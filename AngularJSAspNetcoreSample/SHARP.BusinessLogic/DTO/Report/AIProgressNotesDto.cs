using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SHARP.BusinessLogic.DTO.Report
{
    public class AIProgressNotesDto : PCCNotesDto
    {
        public string Keyword { get; set; }

        public string Summary { get; set; }

        public bool Accept { get; set; }
        public static AIProgressNotesDto FromJson(JObject jObject)
        {
            var inst = new AIProgressNotesDto();
            var notes = PCCNotesDto.FromJson(jObject);
            inst.PatientName = notes.PatientName;
            inst.PatientId = notes.PatientId;
            inst.PatientNotes = notes.PatientNotes;
            inst.Date = notes.Date;
            inst.Time = notes.Time;
            inst.DateTimeNotes = notes.DateTimeNotes;
            inst.FacilityId = notes.FacilityId;
            inst.FacilityName = notes.FacilityName;
            inst.ReportId = notes.ReportId;

            if (jObject.ContainsKey("keyword"))
                inst.Keyword = (string)jObject["keyword"];
            if (jObject.ContainsKey("summary"))
                inst.Summary = (string)jObject["summary"];

            return inst;
        }
    }
}
