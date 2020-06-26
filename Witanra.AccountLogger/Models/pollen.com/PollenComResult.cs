using System;
using System.Collections.Generic;

namespace Witanra.AccountLogger.Models.pollen.com
{
    public class PollenComResult
    {
        public DateTime captureDateTime;
        public List<WebPageResult> webPageResults;
        public Dictionary<DateTime, PollenComDateResult> pollenComDateResults;

        public PollenComResult()
        {
            captureDateTime = DateTime.Now;
            webPageResults = new List<WebPageResult>();
            pollenComDateResults = new Dictionary<DateTime, PollenComDateResult>();
        }
    }
}