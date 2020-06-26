using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Witanra.Shared;

namespace Witanra.ProcessManager
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

            var settings = JsonHelper.DeserializeFile<Settings>("settings.json");
            _cw.LogDirectory = settings.LogDirectory;

            while (true)
            {
                var processes = Process.GetProcesses();

                foreach (var settingProcess in settings.Processes)
                {
                    if (processes.Where(a => a.ProcessName == settingProcess.Name).FirstOrDefault() == null && settingProcess.StartIfMissing)
                    {
                        if (File.Exists(settingProcess.Path))
                        {
                            Console.WriteLine($"Process Missing {settingProcess.Name}. Starting {settingProcess.Path}");
                            Process.Start(settingProcess.Path);
                        }
                    }

                    foreach (var process in processes.Where(a => a.ProcessName == settingProcess.Name))
                        try
                        {
                            if (settingProcess.StopIfFound)
                            {
                                if ((DateTime.Now - process.StartTime).TotalSeconds > settingProcess.StopTotalSeconds)
                                {
                                    Console.WriteLine($"Process Found {settingProcess.Name}. Stopping {settingProcess.Path}");
                                    process.Kill();
                                    break;
                                }
                            }

                            var priority = (ProcessPriorityClass)Enum.Parse(typeof(ProcessPriorityClass), settingProcess.Priority);
                            if (process.PriorityClass != priority)
                            {
                                Console.WriteLine($"Changing Priority for {process.ProcessName} {process.Id} to {settingProcess.Priority}");
                                process.PriorityClass = priority;
                                if (process.PriorityClass != priority)
                                {
                                    Console.WriteLine($"Changing Priority Failed.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Process Error {process.ProcessName} Exception: {ex.Message}");
                        }
                }

                Console.WriteLine($"Waiting {settings.IntervalInSeconds} seconds...");
                Thread.Sleep(settings.IntervalInSeconds * 1000);

                if (!settings.Loop)
                {
                    Console.WriteLine($"Not Looping");
                    break;
                }
                _cw.SaveToDisk();
            }

            CloseWait(settings.IntervalInSeconds);
        }

        private static void CloseWait(int IntervalInSeconds = 30)
        {
            Console.WriteLine($"Application finished, will close in {IntervalInSeconds} seconds.");
            Console.WriteLine("");
            _cw.SaveToDisk();
            Thread.Sleep(IntervalInSeconds * 1000);
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