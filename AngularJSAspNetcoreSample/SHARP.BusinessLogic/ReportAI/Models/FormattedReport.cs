using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.ReportAI.Models
{
    public class FormattedReport 
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Type { get; set; }

        public string Date { get; set; }

        public string Keyword { get; set; }

        public string PdfNotes { get; set; }

        public string CleanedText { get; set; }

        public string UUID { get; set; }

        
        

        public JObject ToJObject()
        {
            JObject json = new JObject();
            json.Add("ID", ID);
            json.Add("Name", Name);
            json.Add("First Name", FirstName);
            json.Add("Last Name", LastName);
            json.Add("CleanedText", CleanedText);
            json.Add("keyword", Keyword);
            json.Add("Type", Type);
            json.Add("Date", Date);
            json.Add("UUID", UUID);
            json.Add("PdfText", PdfNotes);
            return json;
        }
    }
}
