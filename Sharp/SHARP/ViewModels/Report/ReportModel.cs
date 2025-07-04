using SHARP.Common.Enums;
using System.Collections.Generic;

namespace SHARP.ViewModels.Report
{
    public class ReportModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string TableauUrl { get; set; }
        public string ReportUrl { get; set; }
    }
}
