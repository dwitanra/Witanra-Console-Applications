using OpenQA.Selenium;
using System.Collections.Generic;
using Witanra.AccountLogger.Extentions;
using Witanra.AccountLogger.Models.timeanddate.com;

namespace Witanra.AccountLogger.Services
{
    public class timeanddateComService
    {
        private IWebDriver _webDriver;

        public timeanddateComService(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        public TimeAndDateComResult getWeatherForecast(string locationCode)
        {
            var result = new TimeAndDateComResult();

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