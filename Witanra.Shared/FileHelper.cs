using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Witanra.Shared
{
    public class FileHelper
    {

        public static void MoveFileByNameAndDate(string Name, List<string> DateFormats, string SourceDirectory, string DestinationDirectory, int NumberOfDaysToLookBack)
        {
            Console.WriteLine($"Getting Files from {SourceDirectory}...");
            var info = new DirectoryInfo(SourceDirectory);
            var files = info.GetFiles("*.*", SearchOption.AllDirectories).OrderBy(p => p.CreationTime).ToArray();          
            Console.WriteLine($"{files.Length + 1} file{((files.Length + 1 != 1) ? "s" : "")} found");

            for (int i = 0; i <= NumberOfDaysToLookBack; i++)
            {
                foreach (string DateFormat in DateFormats)
                {
                    //var FileForDateFound = false;
                    var Date = DateTime.Today.AddDays(-1 * i).ToString(DateFormat);
                    foreach (var file in files)
                    {
                        if (file.FullName.Contains(Date))
                        {
                            //FileForDateFound = true;
                            var filename = file.CreationTime.ToString("yyyyMMdd-HHmmssffff") + "_" + Path.GetFileName(file.FullName);

                            try
                            {
                                var newFileName = Path.Combine(DestinationDirectory, Name, Date, filename);
                                Directory.CreateDirectory(Path.GetDirectoryName(newFileName));
                                newFileName = GetUniqueFilename(newFileName);

                                Console.WriteLine($"Moving {file.FullName} to {newFileName}");
                                File.Move(file.FullName, newFileName);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Unable to move {ex.Message}");
                            }
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

        public static void DeleteDirSafe(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Console.WriteLine($"Deleting {directory} since it was empty.");
                    Directory.Delete(directory, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to delete Directory {directory}. Exception: {ex.Message}");
            }
        }

        public static string GetUniqueFilename(string filename)
        {
            int count = 1;

            string fileNameOnly = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename);
            string path = Path.GetDirectoryName(filename);
            string newFileName = filename;

            while (File.Exists(newFileName))
            {
                string tempFileName = $"{fileNameOnly}({ count++})";
                newFileName = Path.Combine(path, tempFileName + extension);
            }
            if (filename != newFileName)
            {
                Console.WriteLine($"{filename} was already present. Filename is now {newFileName}");
            }

            return newFileName;
        }

        public static void DeleteDirIfEmpty(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                DeleteDirIfEmpty(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Console.WriteLine($"Deleting {directory} since it was empty.");
                    Directory.Delete(directory, false);
                }
            }
        }

        public static void LaunchCommandLineApp(string dir, string exe, string argument)
        {
            Console.WriteLine($"Launching {dir} {exe} {argument}...");
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

            Console.WriteLine($"Exited {exe}");
        }

        public static string BytesToString(long bytes, bool showBytes = false)
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
            if (showBytes)
                return String.Format("{0:0.##} {1} ({2} bytes)", len, sizes[order], bytes);
            else
                return String.Format("{0:0.##} {1}", len, sizes[order]);
        }
    }
}
