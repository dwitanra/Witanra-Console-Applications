using System;
using System.Collections.Generic;
using System.Text;

namespace Witanra.WebScraper.Websites.www.timeanddate.com.Models
{
    public class HourResult
    {
        public List<string> RowText_Raw;
        public List<string> RowHTML_Raw;

        public string Temperature
        {
            get
            {
                var result = RowText_Raw[2];
                int index = result.IndexOf(" ");
                if (index > 0)
                    result = result.Substring(0, index);
                return result;
            }
        }

        public string Weather
        {
            get
            {
                return RowText_Raw[3];
            }
        }

        public string Temperature_Feels_Like
        {
            get
            {
                var result = RowText_Raw[4];
                int index = result.IndexOf(" ");
                if (index > 0)
                    result = result.Substring(0, index);
                return result;
            }
        }

        public string Wind_Speed
        {
            get
            {
                var result = RowText_Raw[5];
                int index = result.IndexOf(" ");
                if (index > 0)
                    result = result.Substring(0, index);
                return result;
            }
        }

        public string Wind_Direction
        {
            get
            {
                return RowText_Raw[6];
            }
        }

        public string Humidity
        {
            get
            {
                return RowText_Raw[7];
            }
        }

        public string Precipitation_Chance
        {
            get
            {
                return RowText_Raw[8];
            }
        }

        public string Precipitation_Amount
        {
            get
            {
                return RowText_Raw[9];
            }
        }
    }
}