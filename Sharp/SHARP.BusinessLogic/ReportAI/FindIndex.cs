using System;

using System.Collections.Generic;

using System.Linq;

using System.Text.RegularExpressions;

namespace SHARP.BusinessLogic.ReportAI
{

    public class FindIndex
    {

        static Dictionary<string, string> lemmatizationDict = new Dictionary<string, string>()
        {

            {"refused", "refuse"},

            {"refusing", "refuse"},

            {"refusal", "refuse"},

            {"denied", "denie"},

            {"denies", "denie"},

            {"denial", "denie"},

            {"crying", "cry"},

            {"cries", "cry"},

            {"cried", "cry"}  ,

            {"smoke", "smoking" },

            {"cry","crying" },

            {"swallow", "swallowing" },// "ALLERGY/ ALLERGIC",

            {"allergy","allergic" },//SHORTNESS OF BREATH/ SOB

            {"short of breath", "sob" },//"SWELL/ SWOLLEN",

            {"smell","swollen" }, //  "UNRESPONSIVE/ NON RESPONSIVE",

            {"unresponsive", "non responsive" },//"ANXIOUS/ ANXIETY",

            {"anxious", "anxiety"}

            // Add more entries as needed  

        };

        static string Lemmatize(string word)
        {
            if (lemmatizationDict.ContainsKey(word))
                return lemmatizationDict[word];

            return word;
        }

        public static int Find(string cleanedText, string index)
        {

            int check1 = cleanedText.IndexOf(index.ToLower());

            if (check1 == -1)
                return 0;


            index = Lemmatize(index);

            string[] lemmatizedSentence = cleanedText.Split(' ');

            for (int i = 0; i < lemmatizedSentence.Length; i++)
                lemmatizedSentence[i] = Lemmatize(lemmatizedSentence[i]);


            string lemmatizedSentenceStr = string.Join(" ", lemmatizedSentence);

            int pi = check1;

            int poi = check1 + index.Length;

            string pre = "";

            string post = "";

            try
            {

                pre = lemmatizedSentenceStr[pi - 1].ToString();

                post = lemmatizedSentenceStr[poi].ToString();

                if (!char.IsLetter(pre[0]) && !char.IsLetter(post[0]))
                    return 1;


            }
            catch (IndexOutOfRangeException)
            {
                return 0;
            }


            return 0;

        }

        static Dictionary<string, List<string>> constraints = new Dictionary<string, List<string>>()
        {

            {"pain", new List<string>{"no", "denial", "denies"}},

            {"cough", new List<string>{"not new"}}

        };

        public static List<string> GetConstraints(string searchWord)
        {

            if (constraints.ContainsKey(searchWord))
            {
                return constraints[searchWord];
            }

            return new List<string>();
        }

        public static bool CheckAdjacent(string text, string targetWord, List<string> checkWords)
        {

            string pattern = @"(?:[a-zA-Z'-]+[^a-zA-Z'-]+){0,5}" + targetWord + @"(?:[^a-zA-Z'-]+[a-zA-Z'-]+){0,5}";

            Match match = Regex.Match(text, pattern);

            if (match.Success)
            {

                string[] wordsBefore = match.Groups[0].Value.Split(' ').Reverse().Take(5).Reverse().ToArray();

                string[] wordsAfter = match.Groups[0].Value.Split(' ').Take(5).ToArray();

                foreach (string checkWord in checkWords)
                {

                    if (Array.IndexOf(wordsBefore, checkWord) != -1 || Array.IndexOf(wordsAfter, checkWord) != -1)
                        return true;
                }

            }

            return false;
        }

        public static List<string> GetTypes(string searchWord)
        {

            Dictionary<string, List<string>> types = new Dictionary<string, List<string>>()
            {

                {"pain", new List<string>{"skilled nursing charting"}}

            };

            if (types.ContainsKey(searchWord))
            {
                return types[searchWord];
            }

            return new List<string>();

        }

        public static bool CheckTypes(string text, List<string> checkTypes)
        {

            foreach (string checkType in checkTypes)
            {

                string pattern = @"type: " + checkType;

                Match match = Regex.Match(text, pattern);

                if (match.Success)
                    return true;
            }

            return false;
        }

        public static bool CheckNumAdj(string text, string targetWord)
        {

            string pattern = @"(\d*)(\s)(" + targetWord + @")";

            MatchCollection matches = Regex.Matches(text, pattern);

            bool findFlag = false;

            if (matches.Count > 0)
            {

                foreach (Match match in matches)
                {

                    string num = match.Groups[1].Value;

                    if (num.Length > 0 && int.TryParse(num, out int result))
                    {

                        findFlag = true;

                        break;

                    }
                }
            }

            return findFlag;

        }

        public static bool ContainsAny(string text, List<string> elements)
        {
            foreach (string element in elements)
            {
                if (text.Contains(element))

                    return true;
            }

            return false;
        }

        public static bool GetMedicalDesc(string text)
        {
            bool flag = false;

            if (!text.Contains("note"))
                return flag;


            List<string> medSet = new List<string> { "tablet", "mg", "ml" };


            try
            {

                int index = text.IndexOf("note");

                int endIndex = text.IndexOf('\n', index);

                if (endIndex == -1)

                {

                    endIndex = text.Length;

                }

                string content = text.Substring(index, endIndex - index);

                bool result = ContainsAny(content, medSet);

                if (result)

                {

                    flag = false;

                }

            }
            catch
            {

            }
            medSet.Clear();
            return flag;

        }

    }

}