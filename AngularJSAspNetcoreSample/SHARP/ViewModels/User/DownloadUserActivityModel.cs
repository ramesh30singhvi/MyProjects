using System;
using SHARP.Common.Enums;
using SHARP.ViewModels.Common;
using System.Collections.Generic;

namespace SHARP.ViewModels.User
{
	public class DownloadUserActivityModel
	{
        public int? UserId { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }

        public string Type { get; set; }

        public UserFilterModel Filters { get; set; }
    }
}

