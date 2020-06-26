using System;
using System.Collections.Generic;

namespace Witanra.AccountLogger.Models.timeanddate.com
{
    public class TimeAndDateComResult
    {
        public DateTime captureDateTime;
        public List<WebPageResult> webPageResults;
        public Dictionary<DateTime, TimeAndDateComDateResult> pollenComDateResults;

        public TimeAndDateComResult()
        {
            captureDateTime = DateTime.Now;
            webPageResults = new List<WebPageResult>();
            pollenComDateResults = new Dictionary<DateTime, TimeAndDateComDateResult>();
        }
    }
}