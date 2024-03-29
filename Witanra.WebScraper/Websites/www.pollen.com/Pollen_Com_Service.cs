﻿using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using Witanra.WebScraper.Extentions;
using Witanra.WebScraper.Websites.www.pollen.com.Models;

namespace Witanra.WebScraper.Websites.www.pollen.com
{
    public class Pollen_Com_Service
    {
        private IWebDriver _webDriver;

        public Pollen_Com_Service(IWebDriver webDriver)
        {
            _webDriver = webDriver;
        }

        public Result GetForecast(string zipcode)
        {
            var result = new Result();

            _webDriver.GoToUrl($"https://www.pollen.com/forecast/current/pollen/{zipcode}");
            result.webPageResults.Add(_webDriver.GetWebPageResult(_webDriver.FindElement(By.Id("forecast-chart"))));
            Get_Allergy_Forcast_Date(result.pollenComDateResults, "yesterday", -1);
            Get_Allergy_Forcast_Date(result.pollenComDateResults, "today", 0);
            Get_Allergy_Forcast_Date(result.pollenComDateResults, "tomorrow", 1);

            _webDriver.GoToUrl($"https://www.pollen.com/forecast/historic/pollen/{zipcode}");
            result.webPageResults.Add(_webDriver.GetWebPageResult(_webDriver.FindElement(By.Id("highcharts-0"))));

            _webDriver.GoToUrl($"https://www.pollen.com/forecast/extended/pollen/{zipcode}");
            result.webPageResults.Add(_webDriver.GetWebPageResult(_webDriver.FindElement(By.Id("highcharts-0"))));
            var extendedPlot = _webDriver.FindElement(By.XPath("//*[@class='highcharts-data-labels highcharts-series-0 highcharts-tracker']"));
            var extendedDays = extendedPlot.FindElements(By.TagName("tspan"));
            var Date = DateTime.Today;
            foreach (var day in extendedDays)
            {
                if (!result.pollenComDateResults.ContainsKey(Date))
                {
                    var pollenComDateResult = new DateResult
                    {
                        level = Convert.ToDouble(day.Text.RemoveHTMLFormatting())
                    };
                    result.pollenComDateResults.Add(Date, pollenComDateResult);
                }
                Date = Date.AddDays(1);
            }

            return result;
        }

        private void Get_Allergy_Forcast_Date(Dictionary<DateTime, DateResult> PollenComDateResults, string dayKeyword, int DayOffset)
        {
            var divCurrent = _webDriver.FindElements(By.Id("today")).Where(a => a.Text.ToLowerInvariant().Contains(dayKeyword)).FirstOrDefault();
            if (divCurrent != null)
            {
                var allergens = new List<string>();
                var divAllergens = divCurrent.FindElement(By.XPath("following-sibling::*"));
                var liAllergens = divAllergens.FindElements(By.TagName("li"));
                foreach (var liAllergen in liAllergens)
                {
                    allergens.Add(
                            liAllergen.GetAttribute("innerText").RemoveHTMLFormatting()
                        );
                }

                var pollenComDateResult = new DateResult
                {
                    level = Convert.ToDouble(divCurrent.FindElement(By.ClassName("forecast-level")).Text),
                    level_description = divCurrent.FindElement(By.ClassName("forecast-level-desc")).Text,
                    allergens = allergens,
                };
                PollenComDateResults.Add(DateTime.Today.AddDays(DayOffset), pollenComDateResult);
            }
        }
    }
}