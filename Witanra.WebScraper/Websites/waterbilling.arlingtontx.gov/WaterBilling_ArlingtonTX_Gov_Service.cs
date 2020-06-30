using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Threading;
using Witanra.WebScraper.Extentions;
using Witanra.WebScraper.Websites.waterbilling.arlingtontx.gov.Models;

namespace Witanra.WebScraper.Websites.waterbilling.arlingtontx.gov
{
    public class WaterBilling_ArlingtonTX_Gov_Service
    {
        private IWebDriver _webDriver;
        private string _username;
        private string _password;

        public WaterBilling_ArlingtonTX_Gov_Service(IWebDriver webdriver, string username, string password)
        {
            _webDriver = webdriver;
            _username = username;
            _password = password;
        }

        public Result GetResult()
        {
            var result = new Result();

            try
            {
                _webDriver.GoToUrl($"https://waterbilling.arlingtontx.gov/app/login.jsp");
                var usernameElement = _webDriver.FindElement(By.Id("accessCode"));
                usernameElement.Clear();
                usernameElement.SendKeys(_username);
                var passwordElement = _webDriver.FindElement(By.Id("password"));
                passwordElement.Clear();
                passwordElement.SendKeys(_password);
                var builder = new Actions(_webDriver);

                var LoginElement = _webDriver.FindElement(By.XPath("//button[@type='submit']"));
                LoginElement.Click();
                Thread.Sleep(5000);

                Navigate_Section_Add_Result(result, "//a[contains(@href,'#mainTabs-dashboard')]", "//div[@id='mainTabs-dashboard']//div[@class='panel capricorn-selected-panel']", "#home");
                result.Account_Number = _webDriver.FindElement(By.XPath("//*[@id='selectAccountRibbon']//strong")).Text.RemoveHTMLFormatting();
                result.Current_Balance_Raw = _webDriver.FindElement(By.Id("my_account_current_balance")).Text.RemoveHTMLFormatting();
                result.Current_Due_Raw = _webDriver.FindElement(By.Id("my_account_current_due_date")).Text.RemoveHTMLFormatting();

                Navigate_Section_Add_Result(result, "//a[contains(@href,'#tabs-BILLINQ')]", "//*[@id='tabs-BILLINQ']", "#billsandpayments");

                Navigate_Section_Add_Result(result, "//a[contains(@href,'#tabs-PAYHIST')]", "//*[@id='tabs-PAYHIST']", "#transactions");

                Navigate_Section_Add_Result(result, "//a[contains(@href,'#tabs-CONSUMPTION')]", "//*[@id='tabs-CONSUMPTION']", "#billedusage");

                Navigate_Section_Add_Result(result, "//a[contains(@href,'#tabs-WATSMCON')]", "//*[@id='tabs-WATSMCON']", "#remotemeterread");

                Navigate_Section_Add_Result(result, "//a[contains(@href,'#tabs-bilCons')]", "//*[@id='tabs-bilCons']", "#compare");
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
            return result;
        }

        //Below doesn't work because the iFrame X,Y is inaccurate, would be nice to get more focused on the images.
        //private void Navigate_Section_Add_Result(Result result, string Link_XPath, string iFrame_XPath, string Focus_XPath, string URL_Append)
        //{
        //    _webDriver.SwitchTo().ParentFrame();

        //    var Link = _webDriver.FindElement(By.XPath(Link_XPath));
        //    Link.Click();

        //    if (!String.IsNullOrEmpty(iFrame_XPath))
        //    {
        //        var wait = new WebDriverWait(_webDriver, TimeSpan.FromSeconds(10));
        //        wait.Until(driver => driver.FindElement(By.XPath(iFrame_XPath)));
        //    }

        //    var WebPageResult = _webDriver.GetWebPageResult(); //This causes a SwitchTo().ParentFrame
        //    WebPageResult.URL += URL_Append;

        //    var focusedScreenshot = _webDriver.GetScreenshotFocused(WebPageResult.Full_Screenshot, iFrame_XPath, Focus_XPath);

        //    WebPageResult.Focus_Screenshots.Add(focusedScreenshot);

        //    result.WebpageResults.Add(WebPageResult);
        //}

        private void Navigate_Section_Add_Result(Result result, string Link_XPath, string Focus_XPath, string URL_Append)
        {
            var Link = _webDriver.FindElement(By.XPath(Link_XPath));
            Link.Click();
            Thread.Sleep(5000);

            var WebPageResult = _webDriver.GetWebPageResult();
            WebPageResult.URL += URL_Append;

            var focusedScreenshot = _webDriver.GetScreenshotFocused(WebPageResult.Screenshots["_Full"], Focus_XPath);

            WebPageResult.Screenshots.Add("Focused_1", focusedScreenshot);

            result.WebpageResults.Add(WebPageResult);
        }

        //public byte[] GetBill(string Bill_Date)
        //{
        //    return byte[0];
        //}
    }
}