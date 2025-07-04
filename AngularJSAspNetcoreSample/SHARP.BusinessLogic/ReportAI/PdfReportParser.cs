
using ClosedXML.Excel;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SHARP.BusinessLogic.DTO.Report;
using SHARP.BusinessLogic.ReportAI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace SHARP.BusinessLogic.ReportAI
{
    public class PdfReportParser
    {

        private readonly List<string> accidentKeyword = new List<string> { "FLOOR" };

        private List<string> emergency = new List<string> { "General Documentation" };

        private List<string> historicInfoType = new List<string> { "Care Plan Note", "General Progress Note" };

        private readonly List<string> keywordList = new List<string>();

        private readonly List<string> malfunction = new List<string> { "REFUSE" };

        private readonly List<string> medicalEventType = new List<string>

            {"General Documentation", "Nursing Clinical Evaluation"};

        private readonly List<string> medicalList = new List<string>
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

        private readonly List<string> taskInfo = new List<string>

        {

            "General Documentation", "Nursing Clinical Evaluation", "eMar - Medication Administration Note",

            "LN: Skin Monitoring"

        };
        private readonly List<string> excludeType = new List<string>
        {
           "emar-medicationadministrationnote","orders-administrationnote","ordersadministrationnote"
        };
        private readonly Dictionary<string, List<string>> typeMap = new Dictionary<string, List<string>>();

        private readonly Dictionary<int, List<List<string>>> typeMap2 = new Dictionary<int, List<List<string>>>();

        public PdfReportParser()
        {

            keywordList.AddRange(medicalList);

            keywordList.AddRange(accidentKeyword);

            keywordList.AddRange(malfunction);

            typeMap.Add("refuse", taskInfo);

            typeMap.Add("skin tear", medicalEventType);

            typeMap.Add("agitated", medicalEventType);

            typeMap.Add("seizure", medicalEventType);

            typeMap.Add("floor", medicalEventType);

            typeMap2.Add(0, new List<List<string>> { accidentKeyword, taskInfo });

            typeMap2.Add(1, new List<List<string>> { medicalList, medicalEventType });

        }

        internal void Clear()
        {
            medicalList?.Clear();
            accidentKeyword?.Clear();
            malfunction?.Clear();
            taskInfo?.Clear();
            medicalEventType?.Clear();
            historicInfoType?.Clear();
            emergency?.Clear();

            keywordList?.Clear();
            typeMap?.Clear();
            foreach (var item in typeMap2)
            {
                item.Value?.Clear();
            };

            typeMap2?.Clear();


        }
        //extract pages  .every report can take different number of pages .
        public IList<ReportPages> ExtractReports(byte[] bytes)
        {
            var allReports = new List<ReportPages>();

            using (var stream = new MemoryStream(bytes))
            {

                using (var reader = new PdfReader(stream))
                {

                    var pdfDocument = new PdfDocument(reader);

                    var currentReport = new ReportPages();

                    var pageCounter = 0;

                    for (var pageNum = 1; pageNum <= pdfDocument.GetNumberOfPages(); pageNum++)
                    {

                        pageCounter++;

                        pdfDocument.GetPage(pageNum);

                        var text = PdfTextExtractor.GetTextFromPage(pdfDocument.GetPage(pageNum)).Trim();

                        var pageMatch = Regex.Match(text, @"Page\s+(\d+)\s+of\s+(\d+)");

                        if (pageMatch.Success)
                        {

                            var currentPage = int.Parse(pageMatch.Groups[1].Value);

                            var totalPages = int.Parse(pageMatch.Groups[2].Value);

                            currentReport.CurrentPage = currentPage;

                            currentReport.TotalPages = totalPages;

                            if (currentReport.Pages == null)
                                currentReport.Pages = new List<string>();

                            currentReport.Pages =

                                ((List<string>)currentReport.Pages).Concat(new List<string> { text }).ToList();

                            if (currentPage == totalPages)
                            {

                                currentReport.TotalPages = pageCounter;

                                allReports.Add(currentReport);
                                // init report for new 
                                currentReport = new ReportPages();

                            }

                        }

                    }

                    pdfDocument.Close();

                }

            }
            return allReports;

        }

        public PCCReport ProcessReport(ReportPages report)
        {

            if (report.TotalPages == 0)
                return null;

            if (report.Pages == null)
                return null;

            if (!report.Pages.Any())
                return null;

            string firstPageText = report.Pages.FirstOrDefault();
            int headerEndIndex = (int)(firstPageText?.IndexOf("\nEffective Date:"));
            string lastName = string.Empty;
            string firstName = string.Empty;
            string idNumber = string.Empty;
            if (headerEndIndex == -1)
            {
                Debug.WriteLine("eff date not find ");
                return null;
            }
            // Check if the header is found  
            string pattern = @"\s*(\w+),\s*([\w\s]+)\s*\((\w+)\)";
            // Extract the patient's name, ID number, and other information from the header  
            var idMatch = Regex.Match(firstPageText, pattern);
            if (!idMatch.Success)
            {
                return null;
            }
            firstName = idMatch.Groups[2].Value;
            lastName = idMatch.Groups[1].Value;
            idNumber = idMatch.Groups[3].Value;

            var combinedText = string.Concat(report.Pages.Select(page => page.ToString().Substring(headerEndIndex)));

            // Split the combined text into individual notes  

            var notesPattern = new Regex(@"Effective Date: (\d{2}/\d{2}/\d{4} \d{2}:\d{2})");

            var notes = notesPattern.Split(combinedText);

            if (notes.Length == 0)
            {
                Debug.WriteLine("notes " + combinedText);
                return null;
            }
            notes = notes.Skip(1).ToArray();
            var labeledNotes = new Dictionary<int, Dictionary<string, object>>();

            for (var i = 0; i < notes.Length; i += 2)
            {

                var noteText = notes[i + 1].Trim();

                var dateTime = notes[i];

                // Initialize dictionary structure  

                var noteDict = new Dictionary<string, object>();

                noteDict.Add("Date and Time", dateTime);

                noteDict.Add("Full Text", noteText);

                // Extract and assign 'Type'  

                var typeMatch = Regex.Match(noteText, @"Type: ([^\n]+)");
                noteDict.Add("Type", typeMatch.Success ? typeMatch.Groups[1].Value : "");

                // Remove 'Type' and 'Author' from the text for 'Cleaned Text'  

                var cleanedText = Regex.Replace(noteText, @"Type: [^\n]+\n?", "");

                cleanedText = Regex.Replace(cleanedText, @"Author: [^\n]+\n?", "");
                if (string.IsNullOrEmpty(cleanedText))
                {
                    Debug.WriteLine("Cleaned " + noteText);
                    return null;
                }
                noteDict.Add("Cleaned Text", cleanedText);


                // Extract remaining sections  

                foreach (var section in new[] { "Note Text :", "Comments:", "Additional Notes:", "Author:" })
                {
                    try
                    {
                        var match = Regex.Match(noteText, section + @"(.+?)(?=\n[A-Z]\w+ ?:|$)", RegexOptions.Singleline);

                        if (match.Success)
                            noteDict.Add(section.TrimEnd(':'), match.Groups[1].Value.Trim());
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("sections  " + noteText);
                    }
                }

                labeledNotes.Add(i / 2 + 1, noteDict);

            }

            // Return the extracted information as a dictionary  
            if (string.IsNullOrEmpty(lastName) && string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(idNumber))
                return null;

            return new PCCReport()
            {
                Header = firstPageText,
                CcombinedText = combinedText,
                Notes = labeledNotes,
                ID = idNumber,
                Name = $"{firstName.Trim()} {lastName.Trim()}",
                FirstName = firstName,
                LastName = lastName
            };
        }

        public static string ExtractSectionWithKeyword(string paragraph, string keyword)
        {

            string[] sections = paragraph.Split('\n');

            for (int i = 0; i < sections.Length; i++)
            {

                if (sections[i].ToLower().Contains(keyword.ToLower()))
                {
                    // Return the paragraph containing the keyword  

                    string keywordParagraph = sections[i].Trim();

                    // Return the previous paragraph if exists  

                    string previousParagraph = i > 0 ? sections[i - 1].Trim() : "";

                    // Return the next paragraph if exists  

                    string nextParagraph = i < sections.Length - 1 ? sections[i + 1].Trim() : "";

                    return previousParagraph + " " + keywordParagraph + " " + nextParagraph;

                }

            }
            sections.ToList().Clear();
            // If the keyword is not found, return the original paragraph  
            return paragraph;

        }

        public static string FormatReportNotes(string fullText)
        {
            string[] textSet = fullText.Split('\n');

            textSet = textSet.Take(textSet.Length - 1).ToArray();

            string formattedText = string.Join(" ", textSet);

            return formattedText;
        }
        public static string TextPreprocess(string text, string typeTxt)
        {
            string processedText = " " + typeTxt + ":" + text;

            return processedText.ToLower();
        }

        public static FormattedReport ReportReturn(string keyword, PCCReport report, string type, string fullText, string dateTime, string cleanedText)
        {
            return new FormattedReport()
            {
                ID = report.ID,
                Name = report.Name,
                FirstName = report.FirstName,
                LastName = report.LastName,
                UUID = Guid.NewGuid().ToString(),
                Type = type,
                Date = dateTime,
                Keyword = keyword,
                PdfNotes = FormatReportNotes(fullText),
                CleanedText = cleanedText

            };

        }

        public string GetReportDate(string header)
        {
            var match = Regex.Match(header, @"(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s+\d{1,2},\s+\d{4}");

            if (match.Success)
            {
                return match.Value;
            }

            return string.Empty;
        }

        public string GetReportTime(string header)
        {
            var match = Regex.Match(header, @"Time: (\d{2}:\d{2}:\d{2}) [A-Z]{2}");

            if (match.Success)
            {
                return match.Value;
            }

            return string.Empty;
        }

        public void keywordIndexAppend(Dictionary<string, List<FormattedReport>> keywordIndex, string eachKey, string index, List<PCCReport> processed_reports)
        {

            try
            {
                int recordCounter = 0;
                foreach (var report in processed_reports) // reports each patient  
                {
                    var objectReports = report;
                    if (report.Notes.Any())
                    {
                        var notes = report.Notes;

                        if (notes is Dictionary<int, Dictionary<string, object>> notesDic)
                        {

                            foreach (var note in notesDic) // each record of a patients report  
                            {

                                var values = note.Value;

                                if (!values.ContainsKey("Cleaned Text"))
                                    continue;

                                string cleaned_text = values["Cleaned Text"].ToString().ToLower();

                                string date_time = values["Date and Time"].ToString();

                                index = index.Trim();

                                int ind_flag = FindIndex.Find(cleaned_text, index);

                                if (ind_flag != 0)
                                {

                                    bool note_med = FindIndex.GetMedicalDesc(cleaned_text);

                                    cleaned_text = ExtractSectionWithKeyword(cleaned_text, eachKey);

                                    cleaned_text = TextPreprocess(cleaned_text, values["Type"].ToString());

                                    List<string> keyword = FindIndex.GetConstraints(eachKey);

                                    List<string> types = FindIndex.GetTypes(eachKey);

                                    bool type_flag = FindIndex.CheckTypes(cleaned_text, types);

                                    bool type_med_flag = FindIndex.CheckTypes(cleaned_text, new List<string> { "EMAR - Administration Note", "eMar - Medication Administration" });

                                    bool constr_flag = FindIndex.CheckAdjacent(cleaned_text, eachKey, keyword);

                                    bool denial_flag = FindIndex.CheckAdjacent(cleaned_text, eachKey, new List<string> { "denies" });

                                    bool check_adj_num = FindIndex.CheckNumAdj(cleaned_text, eachKey);

                                    //Console.WriteLine(cleaned_text);  

                                    if (denial_flag == false && constr_flag == false)
                                    {

                                        if (note_med == false && check_adj_num == false)
                                        {

                                            if (type_med_flag == false && type_flag == false && recordCounter < 5000)
                                            {

                                                FormattedReport reportFormated = ReportReturn(eachKey, report, values["Type"].ToString(), values["Full Text"].ToString(), date_time, cleaned_text);

                                                if (keywordIndex.ContainsKey(eachKey.ToLower()))
                                                {
                                                    var listValues = keywordIndex[eachKey.ToLower()];
                                                    if( !listValues.Any( x => x.ID == reportFormated.ID) )
                                                        keywordIndex[eachKey.ToLower()].Add(reportFormated);
                                                }
                                                else
                                                {

                                                    var list = new List<FormattedReport>
                                                        {
                                                            reportFormated
                                                        };

                                                    keywordIndex.Add(eachKey.ToLower(), list);

                                                }

                                                recordCounter = recordCounter + 1;

                                                //    recArray.Add(each_key.ToLower());

                                            }

                                        }

                                    }

                                }

                            }

                        }

                    }


                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in keyword indexing : {e}");

            }

        }

        public Dictionary<string, List<FormattedReport>> BuildWordIndex(List<PCCReport> processedReports, List<string> searchWords)
        {
            Dictionary<string, List<FormattedReport>> keywordIndex = new Dictionary<string, List<FormattedReport>>();
            try
            {

                foreach (string searchWord in searchWords)
                {

                    string eachKey = searchWord.ToLower();

                    if (eachKey.Contains("/"))
                    {

                        eachKey = eachKey.Replace(" ", "");

                        string[] eachKeys = eachKey.Split('/');

                        string index = eachKeys[0];

                        foreach (string each in eachKeys)
                            keywordIndexAppend(keywordIndex, each, index, processedReports);

                    }
                    else
                    {

                        keywordIndexAppend(keywordIndex, eachKey, eachKey, processedReports);

                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in word index building function : {e.Message}");
            }

            return keywordIndex;
        }

        internal Tuple<AuditAIPatientPdfNotesDto,string,string> ProcessReportV2(ReportPages report)
        {
            if (report.TotalPages == 0)
                return null;

            if (report.Pages == null)
                return null;

            if (!report.Pages.Any())
                return null;

            string firstPageText = report.Pages.FirstOrDefault();
            int headerEndIndex = (int)(firstPageText?.IndexOf("\nEffective Date:"));
            string lastName = string.Empty;
            string firstName = string.Empty;
            string idNumber = string.Empty;
            if (headerEndIndex == -1)
            {
                Debug.WriteLine("eff date not find ");
                return null;
            }
            // Check if the header is found  
            string pattern = @"\s*(\w+),\s*([\w\s]+)\s*\((\w+)\)";
            // Extract the patient's name, ID number, and other information from the header  
            var idMatch = Regex.Match(firstPageText, pattern);
            if (!idMatch.Success)
            {
                return null;
            }
            firstName = idMatch.Groups[2].Value;
            lastName = idMatch.Groups[1].Value;
            idNumber = idMatch.Groups[3].Value;

            var combinedText = string.Concat(report.Pages.Select(page => page.ToString().Substring(headerEndIndex)));

            // Split the combined text into individual notes  

            var notesPattern = new Regex(@"Effective Date: (\d{2}/\d{2}/\d{4} \d{2}:\d{2})");

            var notes = notesPattern.Split(combinedText);

            if (notes.Length == 0)
            {
                Debug.WriteLine("notes " + combinedText);
                return null;
            }
            notes = notes.Skip(1).ToArray();
          //  var labeledNotes = new Dictionary<int, Dictionary<string, object>>();
            var dateTime = notes[0];
       
            // Return the extracted information as a dictionary  
            if (string.IsNullOrEmpty(lastName) && string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(idNumber))
                return null;


            var patientInfoDto = new AuditAIPatientPdfNotesDto()
            {
                PatientId = idNumber,
                DateTime = dateTime,
                PatientName = $"{firstName}{lastName}",
                PdfNotes = CompressNotes(combinedText),
  
            };

            return new Tuple<AuditAIPatientPdfNotesDto, string, string>(patientInfoDto, GetReportDate(firstPageText), GetReportTime(firstPageText));

        }
        private byte[] CompressNotes(string notes)
        {

            var bytes = Encoding.UTF8.GetBytes(notes);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }

                return mso.ToArray();
            }
        }
    }
 }   
