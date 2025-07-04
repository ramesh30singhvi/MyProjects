using System;
using System.Text.RegularExpressions;

namespace SHARP.BusinessLogic.Helpers
{
	public class HtmlToPlainTextConverter
	{
        public static string ConvertHtmlToPlainTextWithRegex(string html)
        {
            string plainText = Regex.Replace(html, "<.*?>", "");
            return plainText;
        }
    }
}

