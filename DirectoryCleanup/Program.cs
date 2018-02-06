using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Witanra.Shared;

namespace Witanra.DirectoryCleanup
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"{System.AppDomain.CurrentDomain.FriendlyName} {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
            Console.WriteLine("==============================");

            long orginalDirSize = 0;
            long currentDirSize = 0;
            long filesDeleted = 0;
            long bytesDeleted = 0;

            try
            {
                var settings = JsonHelper.DeserializeFile<Settings>("settings.json");
                if (settings.TargetDirSize == default(long))
                    settings.TargetDirSize = long.MaxValue;

                Console.WriteLine($"Finding files in {settings.Directory}...");
                var allFiles = Directory.GetFiles(settings.Directory, "*.*", SearchOption.AllDirectories)
                    .Select(f => new FileInfo(f));

                orginalDirSize = allFiles.Sum(f => f.Length);
                currentDirSize = orginalDirSize;
                Console.WriteLine($"Found {allFiles.Count()} files. Total size is {BytesToString(orginalDirSize)} .");

                Console.WriteLine($"Finding files older than {settings.MinDaysOld} days...");
                var files = allFiles
                    .Where(f => f.CreationTime <= DateTime.Today.AddDays(-1 * settings.MinDaysOld))
                    .Where(f => f.LastWriteTime <= DateTime.Today.AddDays(-1 * settings.MinDaysOld))
                    .OrderByDescending(f => f.CreationTime)
                    .ThenByDescending(f => f.LastWriteTime).ToList();
                Console.WriteLine($"Found {files.Count()} that are older than {settings.MinDaysOld} days.");

                for (int i = files.Count() - 1; i >= 0; i--)
                {
                    if (currentDirSize <= settings.TargetDirSize)
                    {
                        Console.WriteLine($"STOPPED SINCE Directory size ({BytesToString(currentDirSize)}) is now less than target size {BytesToString(settings.TargetDirSize)}");

                        break;
                    }
                    else
                    {
                        try
                        {
                            if (settings.DoDelete)
                            {
                                Console.WriteLine($"Deleting {files[i].FullName}");
                                File.Delete(files[i].FullName);
                            }
                            else
                            {
                                Console.WriteLine($"Would have Deleted {files[i].FullName}");
                            }
                            currentDirSize -= files[i].Length;
                            filesDeleted++;
                            bytesDeleted += files[i].Length;

                            files.RemoveAt(i);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Unable to delete {files[i].FullName}. Exception: {e.Message}");
                        }
                    }
                }

                Console.WriteLine($"Original Directory Size: {BytesToString(orginalDirSize)}");

                if (settings.DoDelete)
                {
                    Console.WriteLine($"Current Directory Size: {BytesToString(currentDirSize)}");
                    Console.WriteLine($"{filesDeleted} files deleted. {BytesToString(bytesDeleted)} deleted.");
                }
                else
                {
                    Console.WriteLine($"Projected Directory Size: {BytesToString(currentDirSize)}");
                    Console.WriteLine($"{filesDeleted} files would have been deleted. {BytesToString(bytesDeleted)} would have been deleted.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
            }

            Console.WriteLine("Application finished, will close in 30 seconds.");
            Thread.Sleep(30000);
        }

        private static string BytesToString(long bytes)
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
