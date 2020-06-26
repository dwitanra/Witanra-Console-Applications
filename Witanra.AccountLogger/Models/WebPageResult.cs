using System;
using System.Collections.Generic;
using System.IO;
using Witanra.AccountLogger.Extentions;

namespace Witanra.AccountLogger.Models
{
    public class WebPageResult
    {
        public DateTime dateTime;
        public string URL;
        public string HTML;
        public byte[] Full_Screenshot;
        public List<byte[]> Focus_Screenshots;

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

            if (Full_Screenshot != null)
            {
                File.WriteAllBytes($"{path}Full.png", Full_Screenshot);
            }
            var i = 1;
            foreach (var Screenshot_Focused in Focus_Screenshots)
            {
                File.WriteAllBytes($"{path}Focused_{i}.png", Screenshot_Focused);
                i++;
            }
        }
    }
}