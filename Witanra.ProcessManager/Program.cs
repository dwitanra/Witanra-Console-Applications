using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Witanra.Shared;

namespace Witanra.ProcessManager
{
    class Program
    {
        private static ConsoleWriter _cw;

        static void Main(string[] args)
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
                    try
                    {
                        var process = processes.Where(a => a.ProcessName == settingProcess.Name).FirstOrDefault();
                        //TODO do to all with this process name!
                        //try catch needs to be within a single process
                        if (process == null && settingProcess.StartIfMissing)
                        {
                            Console.WriteLine($"Process Missing {settingProcess.Name}. Starting {settingProcess.Path}");
                            if (File.Exists(settingProcess.Path))
                            {
                                process = Process.Start(settingProcess.Path);
                            }
                        }
                        if (process != null)
                        {
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
                        else
                        {
                            Console.WriteLine($"Process not found.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Process Error Exception: {ex.Message}");
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

            CloseWait();
        }

        private static void CloseWait()
        {
            Console.WriteLine("Application finished, will close in 5 seconds.");
            Console.WriteLine("");
            _cw.SaveToDisk();
            Thread.Sleep(5000);
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
