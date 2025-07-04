using Azure.AI.OpenAI;
using Azure;
using DocumentFormat.OpenXml.Math;
using OpenAI.Chat;
using SHARP.BusinessLogic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SHARP.BusinessLogic.Services
{
    public class OpenAIService : IOpenAIService
    {
        List<string> malfunction = new List<string> { "REFUSE" };
        readonly List<string> medicalList = new List<string>
        {
            "ABRASION", "ABUSE", "STEAL/ STOLE", "AGGRESSIVE", "AGITATED", "AGITATION", "ALCOHOLIC SUBSTANCE",
            "ALLEGED", "ALLERGY/ ALLERGIC", "ALTERCATION", "ANGRY", "ARGUMENT", "ASPIRATE", "ASSAULT", "ATTEMPT",
            "AWAITING MED", "BANG", "BEHAVIOR OBSERVED", "BLEEDING", "BLISTER", "BODY WEAKNESS", "BREAST", "BREATHE",
            "BRUISE", "BURN", "CHANGE IN CONDITION", "CHOKE", "COMBAT", "CONGEST", "COUGH", "COUNSEL", "COVID",
            "CRY/ CRYING", "CURSE", "CUT", "DECLINE", "DEMAND", "DIARRHEA", "DIFFICULTY OF BREATHING/ DOB",
            "DISCOLORATION", "DTI", "DYSPHAGIA", "DYSPNEA", "ELEVATED TEMPERATURE", "ELEVATOR", "ELOPE", "EMESIS",
            "EXCORIATION", "EXIT", "EXPOSE", "FAINT", "FALL", "FELL", "FEVER", "FIGHT", "FOUND", "FRACTURE",
            "GROUND", "HEADACHE", "HEMATOMA", "HIT", "HOMICIDAL", "HOSPICE", "HURT", "HYPERSEXUALITY", "HYPOXIA",
            "INGEST", "INJURY", "INTERCOURSE", "KICK", "KNIFE", "LABORED BREATHING", "LACERATION", "LAYING", "LEAVE",
            "LETHARGY/ LETHARGIC", "LYING", "LYMPHEDEMA", "MAD", "MALAISE", "MASD", "MEDICATION NOT ADMINISTERED",
            "MEDICATION NOT AVAILABLE", "MEDICATION NOT GIVEN", "NAUSEA", "ON ORDER", "OPEN AREA", "OUTBURST", "PENIS",
            "POLICE", "PULL", "PUNCH", "RALES", "RASH", "RATE", "REDIRECT", "REDNESS", "RESTLESSNESS", "RHONCHI",
            "ROUGH", "SCRATCH", "SCREAM", "SEXUAL HYPERACTIVITY", "SHEAR", "SHORTNESS OF BREATH/ SOB", "SHOUT",
            "SIT", "SKIN TEAR", "SLAM", "SLAP", "SLID", "SLIP", "SMOKE/ SMOKING", "SPIT", "STRIKE", "STUMBLE",
            "SUICIDAL", "SWALLOW/ SWALLOWING", "SWEAR", "SWELL/ SWOLLEN", "SYSTEM IDENTIFIED", "TEAR", "THREAT",
            "THREW", "THREW UP", "THROW", "TOUCH", "ULCER", "ECCHYMOSIS", "EDEMA", "NEGLECT",
            "NONCOMPLIANCE WITH EATING",
            "UNRESPONSIVE/ NON RESPONSIVE", "UNSTAGEABLE", "UPSET NEW INDWELLING CATHETER", "VAGINA", "VIOLATION",
            "VOMIT", "VULGAR", "WANDER", "WEAPON", "WEIGHT LOSS", "WHEEZE/ WHEEZING", "WHIRLED", "WORSENING",
            "WOUND", "YELL", "FECAL IMPACTION", "REFUSE", "LOWERED TO THE FLOOR", "DISLODGE", "ANXIOUS/ ANXIETY",
            "COMPLAIN OF", "PAIN", "INEFFECTIVE", "DISLODGE", "HOME", "ON HOLD", "LICE", "POSITIVE", "EXPOSE/ EXPOSURE",
            "SUSPECTED", "WEIGHT GAIN", "EASE DOWN", "UTI", "INTOXICATION", "NEW WOUND", "CLOGGED", "LEAK", "DISTENDED",
            "MEDICATION ERROR", "DYSURIA", "CONSTIPATION", "SEIZURE", "INSOMNIA", "TREMORS", "SKIN IMPAIRMENT",
            "RUPTURE", "FACIAL DROOP", "UNAROUSABLE", "RACIAL / RACIST", "HEIMLICH", "ABNORMAL", "VITAL SIGNS",
            "DUPLICATE ORDER", "DEVICE NOT AVAILABLE", "ITCH / ITCHING", "CONFUSE / CONFUSION", "NASAL CONGESTION",
            "FATIGUE", "CAUGHT BY", "CAUGHT SELF", "SITTING ON MAT", "HALLUCINATE / HALLUCINATION", "LOWER / LOWERING",
            "PHARMACY NOTIFIED", "BIT/ BITE", "CRITICAL", "HOLLERING", "HEMATURIA", "DEHISCENCE", "SEDIMENT",
            "MACERATION", "DROWSY/ DROWSINESS", "AMS", "INFILTRATE", "GASP/ GASPING", "NEW ANTIBIOTIC",
            "NEW ANTIPSYCHOTIC MEDICATION", "ROOM CHANGE", "MISSING", "CHANGE", "FULL CODE", "DNR", "DNI", "DNH"
        };

        private readonly List<string> accidentKeyword = new List<string> { "FLOOR" };

        private string GetSearchWordGroup(string searchWord)
        {
            string group = null;
            if (accidentKeyword.Contains(searchWord.ToUpper()))
            {
                group = "accident";
            }
            else if (medicalList.Contains(searchWord.ToUpper()))
            {
                group = "medical";
            }
            else if (malfunction.Contains(searchWord.ToUpper()))
            {
                group = "action";
            }
            return group;
        }
        private List<Dictionary<string, string>> WordIndexRefine(List<Dictionary<string, string>> reportList, string searchWord)
        {
            Dictionary<string, int> typeSet = new Dictionary<string, int>();
            List<object> multiType = new List<object>();
            List<Dictionary<string, string>> refinedReport = new List<Dictionary<string, string>>();
            List<Dictionary<string, string>> repeatReport = new List<Dictionary<string, string>>();

            foreach (Dictionary<string, string> report in reportList)
            {
                string types = report["Type"].ToString();
                if (typeSet.ContainsKey(types))
                {
                    typeSet[types]++;
                }
                else
                {
                    typeSet[types] = 1;
                }
            }

            foreach (KeyValuePair<string, int> typeNm in typeSet)
            {
                if (typeNm.Value > 1)
                {
                    multiType.Add(typeNm.Key);
                }
            }

            foreach (Dictionary<string, string> rpt in reportList)
            {
                if (multiType.Contains(rpt["Type"].ToString()))
                {
                    repeatReport.Add(rpt);
                }
                else
                {
                    refinedReport.Add(rpt);
                }
            }

            if (multiType.Count > 0) return repeatReport;
            else return refinedReport;
        }
        private string ListWithDictionaryToString(List<Dictionary<string, string>> reportContent)
        {
            var returnV = "";
            foreach (var item in reportContent)
            {
                var items = from kvp in item
                            select kvp.Key + "=" + kvp.Value;

                returnV += "{" + string.Join(",", items) + "}";
                ///returnV += item.ToString();
            }
            return returnV;
        }

        private  string UserMessageReturn(List<Dictionary<string, string>> reportContent, string keyWord, string keywordGrp)

        {
            if (keywordGrp == "accident")
            {
                return $"Based on the report: {ListWithDictionaryToString(reportContent)}\nGive the number of times the resident had an accident associated with {keyWord}.\nStick to single line output for each case, format:\n<serial number>: <explain incident>";
            }
            else if (keywordGrp == "action")
            {
                return $"Please analyze the provided report(s):\n{ListWithDictionaryToString(reportContent)}\nExtract details about unique instances where the patient {keyWord}. Specify relevant information surrounding the {keyWord} and summarize the information in less than 10 words. Give report in format <serial number>: <explain incident>.";
            }
            else
            {
                return $"Based on the report: {ListWithDictionaryToString(reportContent)}\nGive the total number of cases of resident associated with {keyWord} and a one line summary in max of 10 words. Stick to single line output for each case, format:\n<serial number>: <medical summary explaining {keyWord}>";
            }
        }

        private async Task<string> CheckOccurance(string keyWord, List<Dictionary<string, string>> redefineReport, string keywordGrp, string ftModel)
        {
            try
            {
                string keyFromEnvironment = Environment.GetEnvironmentVariable("23283e00781f45e0b98feccafac5c730");
                AzureOpenAIClient azureClient = new AzureOpenAIClient(new Uri("https://sharpopenaieastus.openai.azure.com/"),
                new AzureKeyCredential("23283e00781f45e0b98feccafac5c730"));

                ChatClient chatClient = azureClient.GetChatClient(ftModel);
                string userMessage = UserMessageReturn(redefineReport, keyWord, keywordGrp);
                var messages = new List<ChatMessage>()
                {
                    new SystemChatMessage($"You are an AI medical assistant which converts report infos to python dictionaries Check 'Cleaned Text' for {keyWord} related incident of a patient. Identify unique occurrences, summarize if multiple reports refer to same event. Respond just with a python dictionary of key and value as: <s.no of {keyWord}>: <incident>. Use concise medical terms, one per line."),
                    new UserChatMessage( userMessage),
                };

                var result = await chatClient.CompleteChatAsync(messages);
                string gptOutput = result.Value.Content[0].Text;

                if (gptOutput != null) return gptOutput;
                else
                {
                    return "0:None";
                }
            }
            catch (Exception e)
            {
                return "0:None";
            }
        }

        public OpenAIService()
        {

        }
        public async Task<Dictionary<string, List<Dictionary<string, object>>>> Search(Dictionary<string, List<Dictionary<string, string>>> wordIndex)
        {
            var results = new Dictionary<string, List<Dictionary<string, object>>>();

            // Fetch resident's ID's having search word contents in reports  
            var idSet = new Dictionary<string, string>();
            foreach (var report in wordIndex.Values)
            {
                foreach (var details in report)
                {
                    var id = "";
                    var name = "";

                    if (details.ContainsKey("ID"))
                    {
                        id = (string)details["ID"];
                    }
                    if (details.ContainsKey("First Name"))
                    {
                        name = (string)details["First Name"];
                    }
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(id))
                    {
                        idSet[id] = name;
                    }
                }
            }
            int count = 0;
            var selectedKeys = new[] { "ID", "Name", "Type", "Date", "Cleaned Text" };
            //var residentContent = new Dictionary<string,List<Dictionary<string,string>>>();
            var redefineReport = new List<Dictionary<string, string>>();
            foreach (var searchKey in wordIndex.Keys)
            {
                var searchWord = searchKey.ToLower();
                foreach (var idSetItem in idSet)
                {
                    var residentContent = new List<Dictionary<string, string>>(); // list of notes of a resident associated with search word  
                    var report = new Dictionary<string, string>();



                    for (int i = 0; i < wordIndex[searchWord].Count; i++)
                    {
                        var items = from kvp in wordIndex[searchWord][i]
                                    where kvp.Value == idSetItem.Key
                                    select kvp.Value;
                        if (!items.Any(x => x == idSetItem.Key))
                            continue;

                        report = wordIndex[searchWord][i];

                        var selectedReport = report.Where(r => selectedKeys.Contains(r.Key)).ToDictionary(r => r.Key, r => r.Value);
                        residentContent.Add(selectedReport);

                    }

                    if (!residentContent.Any())
                        continue;

                    if (malfunction.Contains(searchWord.ToUpper()))
                    {
                        redefineReport = residentContent;
                    }
                    else
                    {
                        redefineReport = WordIndexRefine(residentContent, searchWord);
                    }
                    var search_word_grp = GetSearchWordGroup(searchWord);


                    var reportRptCheck = await CheckOccurance(searchWord, redefineReport, search_word_grp, "Gpt35Turbo-1");


                    var responses = reportRptCheck.Split('\n');

                    foreach (var response in responses)
                    {
                        var status = response.Split(':')[0];
                        try
                        {
                            if (int.Parse(status) > 0)
                            {

                                if (!report.Keys.Any())
                                    continue;

                                if (!results.ContainsKey(searchWord))
                                {
                                    results[searchWord] = new List<Dictionary<string, object>>();
                                }
                                results[searchWord].Add(new Dictionary<string, object>
                                {
                                    { "ID", report["ID"] },
                                    { "Name", report["Name"] },
                                    { "Date", report["Date"] },
                                    { "Original", report["Cleaned Text"] },
                                    { "PdfText", report["Pdf Notes"] },
                                    { "Summary", response.Split(':')[1] },
                                    { "SearchWord", searchWord },
                                    { "Acceptable", 1 },
                                    { "UserSummary", "" }
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                    }

                }


            }
            //var returvalue = new Dictionary<string, object>()
            //{
            //    { "error", "" },
            //    { "result", results }
            //};
            return results;
        }


    }
}
