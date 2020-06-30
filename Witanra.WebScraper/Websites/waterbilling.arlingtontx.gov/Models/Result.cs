using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Witanra.WebScraper.Models;

namespace Witanra.WebScraper.Websites.waterbilling.arlingtontx.gov.Models
{
    public class Result
    {
        public List<WebpageResult> WebpageResults;
        public DataTable Bills;
        public DataTable Transactions;
        public DataTable BilledUsages;

        public string Account_Number;
        public string Current_Due_Raw;
        public string Current_Balance_Raw;

        public Result()
        {
            WebpageResults = new List<WebpageResult>();
        }

        public Exception Exception { get; internal set; }
    }
}