using System;
using System.IO;
using System.Threading;
using Witanra.Shared;

namespace Witanra.OrganizeByDate
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

            Console.WriteLine($"Found { settings.DirectoryPairs.Count} Directory Pairs in Settings");

            foreach (var directorypair in settings.DirectoryPairs)
            {
                MoveFileByCreatedDate(settings.DateFormat, directorypair.SourceDirectory, directorypair.DestinationDirectory);
            }            

            CloseWait();
        }

        public static void MoveFileByCreatedDate(string DateFormat, string SourceDirectory, string DestinationDirectory)
        {
            Console.WriteLine($"Getting Files from {SourceDirectory}...");
            string[] files = Directory.GetFiles(SourceDirectory, "*.*", SearchOption.AllDirectories);
            Console.WriteLine($"{files.Length + 1} file{((files.Length + 1 != 1) ? "s" : "")} found");

            foreach (string file in files)
            {
                var filename = Path.GetFileName(file);
                try
                {
                    var date = new DateTime(Math.Min(File.GetCreationTime(file).Ticks, File.GetLastWriteTime(file).Ticks));
                    var newFileName = Path.Combine(DestinationDirectory, date.Date.ToString(DateFormat), filename);
                    newFileName = FileHelper.GetUniqueFilename(newFileName);                    

                    Console.WriteLine($"Moving {file} to {newFileName}");
                    (new FileInfo(newFileName)).Directory.Create();
                    File.Move(file, newFileName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to move {ex.Message}");
                }
            }
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
