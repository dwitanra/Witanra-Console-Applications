﻿using System;
using System.IO;
//using System.Linq;
using System.Threading;
using Witanra.Shared;

namespace Witanra.Security
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

            Console.WriteLine($"Found { settings.Directories.Count} Directory in Settings");

            foreach (var directory in settings.Directories)
            {
                try
                {
                    FileHelper.MoveFileByNameAndDate(directory.Name, settings.DateFormats, directory.Directory, settings.DestinationDirectory, 30, false);

                    var Dirs = Directory.GetDirectories(Path.Combine(settings.DestinationDirectory, directory.Name));
                    foreach (var dir in Dirs)
                    {
                        var IsToday = false;
                        foreach (string DateFormat in settings.DateFormats)
                        {
                            if (dir.Contains(DateTime.Today.ToString(DateFormat)))
                            {                                
                                IsToday = true;
                                break;
                            }
                        }
                        if (IsToday)
                        {
                            Console.WriteLine($"Not Making video file for {dir} because it is today.");
                           break;
                        }

                        var dirName = new DirectoryInfo(dir).Name;
                        var filename = Path.Combine(settings.DestinationDirectory, directory.Name + "_" + dirName + ".mp4");
                        VideoHelper.MakeVideoFromImages(dir, filename, settings.TempDirectory, true);
                    }
                    FileHelper.DeleteDirIfEmpty(directory.Directory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Directory was not processes successfully. Exception: {ex.Message}");
                }
            }

            FileHelper.DeleteDirIfEmpty(settings.DestinationDirectory);

            CloseWait();
        }

        static void CloseWait()
        {
            Console.WriteLine("Application finished, will close in 30 seconds.");
            Console.WriteLine("");
            _cw.SaveToDisk();
            Thread.Sleep(30000);
        }

        static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Exception:" + e.ExceptionObject.ToString());
            CloseWait();
            Environment.Exit(1);
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            _cw.SaveToDisk();
        }
    }
}
