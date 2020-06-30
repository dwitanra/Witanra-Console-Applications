using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Witanra.WebScraper.Extentions;
using Witanra.WebScraper.Websites.www.timeanddate.com.Models;

namespace Witanra.WebScraper.Websites.www.timeanddate.com
{
    public class DateAndTime_Com_Service
    {
        private IWebDriver _webDriver;

        public DateAndTime_Com_Service(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        public Result GetWeatherForecast(string locationCode)
        {
            var result = new Result();

            _webDriver.GoToUrl($"https://www.timeanddate.com/weather/{locationCode}/hourly");
            result.webPageResults.Add(
                _webDriver.GetWebPageResult(
                    new List<IWebElement> {
                        _webDriver.FindElement(By.Id("weatherContainer"))
                    },
                    new List<IWebElement> {
                        _webDriver.FindElement(By.XPath("//table[@id='wt-hbh']/thead")),
                        _webDriver.FindElement(By.Id("header__wrapper")),
                    }
                )
            );

            DateTime date = DateTime.Today;
            var today = !DateTime.TryParse((new SelectElement(_webDriver.FindElement(By.Id("wt-hbh-select")))).SelectedOption.Text, out date);
            if (today)
            {
                date = DateTime.Today;
            }

            var TableRows = _webDriver.FindElements(By.XPath("//table[@id='wt-hbh']/tbody/tr"));
            foreach (var tablerow in TableRows)
            {
                var TableTh = tablerow.FindElement(By.TagName("th"));
                var TableTd = tablerow.FindElements(By.TagName("td"));
                var dateResult = new HourResult
                {
                    RowText_Raw = TableTd
                        .Select(a => a.GetAttribute("innerText").RemoveHTMLFormatting())
                        .Prepend(TableTh.GetAttribute("innerText").RemoveHTMLFormatting())
                        .ToList(),

                    RowHTML_Raw = TableTd
                        .Select(a => a.GetAttribute("innerHTML"))
                        .Prepend(TableTh.GetAttribute("innerText"))
                        .ToList()
                };
                var timeText = TableTh.GetAttribute("innerText");
                int index = timeText.IndexOf("\r");
                if (index > 0)
                    timeText = timeText.Substring(0, index);

                DateTime time;
                DateTime.TryParseExact(timeText, "h:mm tt", null, DateTimeStyles.None, out time);

                result.dateValues.Add(date.AddTicks(time.TimeOfDay.Ticks), dateResult);
            }

            _webDriver.GoToUrl($"https://www.timeanddate.com/weather/{locationCode}/ext");
            result.webPageResults.Add(
                _webDriver.GetWebPageResult(
                     new List<IWebElement> {
                        _webDriver.FindElement(By.Id("weatherContainer"))
                    },
                     new List<IWebElement> {
                        _webDriver.FindElement(By.XPath("//table[@id='wt-ext']/thead")),
                        _webDriver.FindElement(By.Id("header__wrapper")),
                    }
                )
            );

            _webDriver.GoToUrl($"https://www.timeanddate.com/weather/{locationCode}/historic");
            result.webPageResults.Add(
                 _webDriver.GetWebPageResult(
                     new List<IWebElement> {
                        _webDriver.FindElement(By.Id("weatherContainer"))
                     },
                     new List<IWebElement> {
                        _webDriver.FindElement(By.XPath("//table[@id='wt-his']/thead")),
                        _webDriver.FindElement(By.Id("header__wrapper")),
                     }
                 )
             );

            return result;
        }
    }
}