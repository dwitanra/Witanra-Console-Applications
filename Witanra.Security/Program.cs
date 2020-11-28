using System;
using System.IO;
using System.Threading;
using Witanra.Shared;

namespace Witanra.Security
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

            var Camera_Dirs = Directory.GetDirectories(settings.SourceDataDirectory);

            Console.WriteLine($"Found { Camera_Dirs.Length} Directory in {settings.SourceDataDirectory}");

            foreach (var Camera_Dir in Camera_Dirs)
            {
                var Day_Dirs = Directory.GetDirectories(Camera_Dir);
                foreach (var day_dir in Day_Dirs)
                {
                    try
                    {
                        var IsTodayFormatMatch = false;
                        foreach (string DateFormat in settings.DateFormats)
                        {
                            if (day_dir.Contains(DateTime.Today.ToString(DateFormat)))
                            {
                                IsTodayFormatMatch = true;
                                break;
                            }
                        }
                        if (IsTodayFormatMatch && settings.ExcludeToday)
                        {
                            Console.WriteLine($"Not Making video file for {day_dir} because it contains Today.");
                            continue;
                        }

                        var filename = Path.Combine(settings.DestinationSummaryDirectory, new DirectoryInfo(Camera_Dir).Name + "_" + DateTime.Today.ToString(settings.DateFormats[0]) + ".mp4");
                        VideoHelper.MakeVideoFromImages(day_dir, filename, settings.TempDirectory, settings.DeleteImages, settings.MinImagesToMakeVideo);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Directory was not processes successfully. Exception: {ex.Message}");
                    }
                }
            }

            FileHelper.DeleteDirIfEmpty(settings.SourceDataDirectory);

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