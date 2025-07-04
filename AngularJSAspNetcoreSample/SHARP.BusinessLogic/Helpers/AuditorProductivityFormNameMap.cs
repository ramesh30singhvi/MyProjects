using System;
using System.Collections.Generic;
using System.Text;

namespace SHARP.BusinessLogic.Helpers
{
    public static class AuditorProductivityFormNameMap
    {
        public static Dictionary<string, IList<string>> Maro = new Dictionary<string, IList<string>>
        {
            { "24/72 Hour Keyword",new List<string> {  @"\b24/72\b", @"\bHour\b",@"\bKeyword\b" } },
            { "24 Hour Admission/Readmission",new List<string> {  @"\b24\b", @"\bHour\b" ,@"\bAdmission/Readmission\b" }  },
            { "Admission", new List<string> { @"(?<!\b(?:24|48|72)\s+Hour\s+)(?<!\b7\s+Day\s+)Admission\b" } },
            { "Discharge Community", new List<string> { @"\bDischarge\b", @"\bCommunity\b" } },
            { "Discharge Death",  new List<string> { @"\bDischarge\b", @"\bDeath\b" } },
            { "Room Change", new List<string> { @"\bRoom\b", @"\bChange\b" } },
            { "Supplemental Refusal",   new List<string> { @"\bSupplemental\b", @"\bRefusal\b" } },
            { "Supplemental Change In Condition",  new List<string> { @"\bSupplemental\b", @"\bChange\b", @"\bIn\b", @"\bCondition\b" }  },
            { "Supplemental Behavior",  new List<string> {  @"\bSupplemental\b", @"\bBehavior\b" } },
            { "24 Hour Keyword",  new List<string> { @"(?<!24/72\s)24\s+Hour\s+Keyword\b" } },
            { "Supplemental Wound",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Foley Catheter",  new List<string> { @"\Foley\b", @"\bCatheter\b" } },
            { "Hemodialysis",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Laboratory/Diagnostic",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Wound",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Advance Directives",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "48 Hour Admission",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "72 Hour Admission",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "7 Day Admission (Revisit)",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Supplemental New Advanced Directive",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Supplemental Hospice",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Nebulizer Treatment",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "CPAP/Bipap/Oxygen",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "ED Visit/Hospitalization",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Admission and Readmission Tracker",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Discharged Community Tracker",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Intravenous",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Supplemental New Antipsychotic Medication",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Tracheostomy",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Mechanical Ventilation",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "7th Day Revisit",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Hospice",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Diabetes/Insulin",  new List<string> { @"\bDiabetes\b", @"\bInsulin\b" } },
            { "Insulin",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Psychotropic",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Respiratory Audit",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Elopement And Wandering",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Anticoagulant (Coumadin)",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Weight Loss",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "Tube Feeding",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },

            { "Discharged AMA",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "24 Keyword AI V1",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },
            { "24 Keyword AI V2",  new List<string> {  @"\bSupplemental\b", @"\bWound\b" } },

        };
    }
}
