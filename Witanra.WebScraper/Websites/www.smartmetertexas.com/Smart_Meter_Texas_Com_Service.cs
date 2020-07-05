using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Witanra.WebScraper.Extentions;
using Witanra.WebScraper.Websites.www.smartmetertexas.com.Models;

namespace Witanra.WebScraper.Websites.www.smartmetertexas.com
{
    public class Smart_Meter_Texas_Com_Service
    {
        private IWebDriver _webDriver;
        private string _username;
        private string _password;

        public Smart_Meter_Texas_Com_Service(IWebDriver webdriver, string username, string password)
        {
            _webDriver = webdriver;
            _username = username;
            _password = password;
        }

        public Result GetResult()
        {
            //ERROR smart meter texas knows that you are using selenium and blocks request
            //need to clear cookies after loading?
            var result = new Result();
            try
            {
                _webDriver.GoToUrl($"https://www.smartmetertexas.com/home");
                var usernameElement = _webDriver.FindElement(By.Id("userid"));
                usernameElement.Clear();
                usernameElement.SendKeys(_username);
                var passwordElement = _webDriver.FindElement(By.Id("password"));
                passwordElement.Clear();
                passwordElement.SendKeys(_password);

                var LoginElement = _webDriver.FindElement(By.XPath("//button[@class='btn btn-large btn-block btn-primary']"));
                LoginElement.Click();
                Thread.Sleep(5000);

                result.webPageResults.Add(_webDriver.GetWebPageResult(_webDriver.FindElement(By.XPath("//*[@class='//*[@class='interval-chart'][1]'"))));
            }
            catch
            {
            }
            return result;
        }
    }
}