using System;
using System.Diagnostics;
using System.IO;

namespace Witanra.Shared
{
    public class FileHelper
    {
        public static void MoveFileByNameAndDate(string Name, string DateFormat, string SourceFolder, string DestinationFolder, int NumberOfDaysToLookBack)
        {
            Console.WriteLine($"Getting Files from {SourceFolder}");
            string[] files = Directory.GetFiles(SourceFolder, "*.*", SearchOption.AllDirectories);

            for (int i = 0; i <= NumberOfDaysToLookBack; i++)
            {
                //var FileForDateFound = false;
                var Date = DateTime.Today.AddDays(-1 * i).ToString(DateFormat);
                foreach (string file in files)
                {
                    if (file.Contains(Date))
                    {
                        //FileForDateFound = true;
                        var filename = Path.GetFileName(file);
                        //Console.WriteLine($"Moving {filename}");
                        try
                        {
                            var newFileName = Path.Combine(DestinationFolder, Name, Date, filename);
                            (new FileInfo(newFileName)).Directory.Create();
                            if (File.Exists(newFileName))
                            {
                                File.Delete(newFileName);
                            }
                            File.Move(file, newFileName);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unable to move {ex.Message}");
                        }
                    }
                }
                //if (!FileForDateFound)
                //{
                //    Console.WriteLine($"No files for {Date} found. Not processing more dates");
                //    break;
                //}
            }
        }

        internal static void DeleteDirSafe(string Dir)
        {

            try
            {
                Directory.Delete(Dir, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to delete Directory {Dir}. Exception: {ex.Message}");
            }
        }

        public static string GetUniqueFilename(string filename)
        {
            int count = 1;

            string fileNameOnly = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename);
            string path = Path.GetDirectoryName(filename);
            string newFullPath = filename;

            while (File.Exists(newFullPath))
            {
                string tempFileName = $"{fileNameOnly}({ count++})";
                newFullPath = Path.Combine(path, tempFileName + extension);
            }
            return newFullPath;
        }

        public static void DeleteDirIfEmpty(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                DeleteDirIfEmpty(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
            }
        }

        public static void LaunchCommandLineApp(string dir, string exe, string argument)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = exe;
            startInfo.Arguments = argument;
            startInfo.WorkingDirectory = dir;
            try
            {
                Process exeProcess = Process.Start(startInfo);

                exeProcess.WaitForExit();
                exeProcess.Close();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
            finally
            {

            }
        }

        public static string BytesToString(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            return String.Format("{0:0.##} {1} ({2} bytes)", len, sizes[order], bytes);
        }
    }
}
