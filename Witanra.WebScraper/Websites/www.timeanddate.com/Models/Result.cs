using System;
using System.Collections.Generic;
using Witanra.WebScraper.Models;

namespace Witanra.WebScraper.Websites.www.timeanddate.com.Models
{
    public class Result
    {
        public DateTime captureDateTime;
        public List<WebpageResult> webPageResults;
        public Dictionary<DateTime, HourResult> dateValues;

        public Result()
        {
            captureDateTime = DateTime.Now;
            webPageResults = new List<WebpageResult>();
            dateValues = new Dictionary<DateTime, HourResult>();
        }
    }
}