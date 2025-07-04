using System;
using System.Collections.Generic;
using SHARP.Common.Enums;


namespace SHARP.BusinessLogic.DTO.UserActivity
{
	public class DownloadUserActivityDto
	{
        public int? UserId { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
}

