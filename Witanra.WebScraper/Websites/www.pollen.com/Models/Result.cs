using System;
using System.Collections.Generic;
using Witanra.WebScraper.Models;

namespace Witanra.WebScraper.Websites.www.pollen.com.Models
{
    public class Result
    {
        public DateTime captureDateTime;
        public List<WebpageResult> webPageResults;
        public Dictionary<DateTime, DateResult> pollenComDateResults;

        public Result()
        {
            captureDateTime = DateTime.Now;
            webPageResults = new List<WebpageResult>();
            pollenComDateResults = new Dictionary<DateTime, DateResult>();
        }
    }
}