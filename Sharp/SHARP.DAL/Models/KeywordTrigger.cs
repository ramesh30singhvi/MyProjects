using Newtonsoft.Json;
using SHARP.Common.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.DAL.Models
{
    public class KeywordTrigger
    {
        public KeywordTrigger() {

        }
        public KeywordTrigger(int keywordFormId, int keywordId, int formTriggerId)
        {
            KeywordFormId = keywordFormId;
            KeywordId = keywordId;
            FormTriggerId = formTriggerId;
        }

        public int Id { get; set; }

        public int KeywordFormId { get; set; }

        public int KeywordId { get; set; }

        public int FormTriggerId { get; set; }

        public TableColumn Keyword { get; set; }

        public Form FormTriggeredByKeyword {get;set;}

        public KeywordTrigger Clone()
        {
            var keywordTrigger = new KeywordTrigger(KeywordFormId, KeywordId, FormTriggerId);

            return keywordTrigger;
        }
    }
}
