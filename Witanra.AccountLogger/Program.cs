using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Witanra.Shared;
using Witanra.WebScraper.Websites.www.timeanddate.com;
using Witanra.WebScraper.Websites.www.pollen.com;
using Witanra.WebScraper.Websites.waterbilling.arlingtontx.gov;

namespace Witanra.AccountLogger
{
    internal class Program
    {
        private static ConsoleWriter _cw;

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            _cw = new ConsoleWriter();
            Console.SetOut(_cw);

            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("headless");//Comment if we want to see the window.
            options.AddArgument("window-size=1920,5080");
            var driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), options);

            //var pollenService = new Pollen_Com_Service(driver);
            //var pollenResult = pollenService.GetForecast("76012");
            //foreach (var webpage in pollenResult.webPageResults)
            //{
            //    webpage.SaveToFolder("C:\\temp\\");
            //}

            var weatherService = new DateAndTime_Com_Service(driver);
            var weatherResult = weatherService.GetWeatherForecast("@z-us-76012");
            foreach (var webpage in weatherResult.webPageResults)
            {
                webpage.SaveToFolder("C:\\temp\\");
            }

            var waterService = new WaterBilling_ArlingtonTX_Gov_Service(driver, "den85nis", "arWit411!");
            var waterResult = waterService.GetResult();
            foreach (var webpage in waterResult.WebpageResults)
            {
                webpage.SaveToFolder("C:\\temp\\");
            }

            driver.Close();
            driver.Quit();

            //Pollen Count
            //Weather (temp, humidity, wind, visibility, precip)
            //Internet Usage
            //Electricity Usage
            //Water Usage

            //Should I open the windows
            //Weekly update on credit card, balances, when due, when payment
            //Weekly update on utilities (internet, electricity, water)
            //School closures
            //Should I wash my car
            //Should I mow the lawn (best time)
            //Is there an eclipse and can I see it (cloud cover)
            //Is a store having a sale? Disney
            //It an item on sale? (Grocery list, item you want to buy)
            //Should I open the window?
            //reddit /r/popular /r/news /r/worldnews
            //imgur popular
            //popular torrents
            //Tell me if indonesia is in the news (keyword flag)

            //Travel warnings

            CloseWait();
        }

        private static void CloseWait()
        {
            Console.WriteLine("Application finished, will close in 30 seconds.");
            Console.WriteLine("");
            _cw.SaveToDisk();
            Thread.Sleep(30000);
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Exception:" + e.ExceptionObject.ToString());
            CloseWait();
            Environment.Exit(1);
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            _cw.SaveToDisk();
        }
    }
}