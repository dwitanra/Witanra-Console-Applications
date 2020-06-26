using OpenQA.Selenium.Chrome;
using System.IO;
using System.Reflection;
using Witanra.AccountLogger.Services;

namespace Witanra.AccountLogger
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("headless");//Comment if we want to see the window.
            options.AddArgument("window-size=1920,5080");
            var driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), options);

            var pollenService = new pollenComService(driver);
            var pollenResult = pollenService.getForecast("76012");
            foreach (var webpage in pollenResult.webPageResults)
            {
                webpage.SaveToFolder("C:\\temp\\");
            }

            var weatherService = new timeanddateComService(driver);
            var weatherResult = weatherService.getWeatherForecast("@z-us-76012");
            foreach (var webpage in weatherResult.webPageResults)
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
        }
    }
}