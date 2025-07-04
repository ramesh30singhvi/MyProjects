namespace SHARP.ViewModels.Report
{
    public class PdfReportAnalysisModel
    {
        public int NumberKeywordsFound { get; set; }
        public string Error { get; set; }

        public string Date { get; set; }
        public string Time { get; set; }

        public string BuildIndexJson { get; set; }

        public string Keywords { get; set; }
    }
}
