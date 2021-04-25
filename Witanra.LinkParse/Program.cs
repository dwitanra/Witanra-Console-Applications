using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Witanra.Shared;

namespace Witanra.RegExParse
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

            var settingsFile = "settings.json";
            if (args != null && args.Length > 0)
            {
                settingsFile = args[0];
            }

            var settings = JsonHelper.DeserializeFile<Settings>(settingsFile);
            _cw.LogDirectory = settings.LogDirectory;

            var mc = Regex.Matches(File.ReadAllText(settings.InputFile), settings.RegExExpression);

            var matches = new List<string>();
            foreach (Match m in mc)
            {
                matches.Add(m.ToString());
            }

            var matchCount = matches
                .GroupBy(s => s)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .Where(c => c.Count >= settings.CountGreaterThan)
                .OrderByDescending(g => g.Count);

            File.WriteAllLines(settings.OutputFile, matchCount.Select(i => i.Name).ToList());

            Console.WriteLine($"Saved {matchCount.Count()} items to {settings.OutputFile}");

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