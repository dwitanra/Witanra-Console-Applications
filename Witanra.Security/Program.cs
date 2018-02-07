using System;
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
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            _cw = new ConsoleWriter();
            Console.SetOut(_cw);

            var settings = JsonHelper.DeserializeFile<Settings>("settings.json");
            _cw.LogDirectory = settings.LogDirectory;

            Console.WriteLine($"Found { settings.Folders.Count} Folder in Settings");

            var today = DateTime.Today.ToString(settings.DateFormat);

            foreach (var folder in settings.Folders)
            {
                try
                {
                    FileHelper.MoveFileByNameAndDate(folder.Name, settings.DateFormat, folder.Dir, settings.DestinationDir, 30);

                    var Dirs = Directory.GetDirectories(Path.Combine(settings.DestinationDir, folder.Name));
                    foreach (var dir in Dirs)
                    {
                        if (dir.Contains(today))
                            break;

                        var dirName = new DirectoryInfo(dir).Name;
                        var filename = FileHelper.GetUniqueFilename(Path.Combine(Directory.GetParent(dir).FullName, folder.Name + "_" + dirName + ".mp4"));
                        VideoHelper.MakeVideoFromImages(dir, filename, settings.TempDir, true);
                    }
                    FileHelper.DeleteDirIfEmpty(folder.Dir);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"folder was not processes successfully. Exception: {ex.Message}");
                }
            }

            FileHelper.DeleteDirIfEmpty(settings.DestinationDir);

            Console.WriteLine("Application finished, will close in 30 seconds.");
            Console.WriteLine("");

            _cw.SaveToDisk();

            Thread.Sleep(30000);
        }
        static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());

            _cw.SaveToDisk();

            Environment.Exit(1);
        }
    }
}
