using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Witanra.WebScraper.Extentions;

namespace Witanra.WebScraper.Models
{
    public class WebpageResult
    {
        public DateTime dateTime;
        public string URL;
        public string HTML;
        public Dictionary<string, byte[]> Screenshots;

        public void GetScreenshotFocused(IWebDriver driver, string iFrame_XPath, string Focus_XPath)
        {
        }

        public void SaveToFolder(string SaveDirectory = "", bool PrePendDate = false, bool PrePendTime = false)
        {
            var path = Path.Combine(SaveDirectory, $"{URL.GetSafeFilename()}\\");
            if (PrePendDate)
            {
                path = path + dateTime.ToString("yyyyMMdd-");
            }
            if (PrePendTime)
            {
                path = path + dateTime.ToString("HHmmssffff-");
            }

            Directory.CreateDirectory(path);

            File.WriteAllText($"{path}index.html", HTML);

            foreach (var Screenshot in Screenshots)
            {
                var filename = Screenshot.Key;

                File.WriteAllBytes($"{path}{filename}.png", Screenshot.Value);
            }
        }
    }
}