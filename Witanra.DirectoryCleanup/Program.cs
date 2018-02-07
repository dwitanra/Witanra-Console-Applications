﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using Witanra.Shared;

namespace Witanra.DirectoryCleanup
{
    class Program
    {
        private static ConsoleWriter _cw;

        static void Main(string[] args)
        {
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            _cw = new ConsoleWriter();
            Console.SetOut(_cw);

            long orginalDirSize = 0;
            long currentDirSize = 0;
            long filesDeleted = 0;
            long bytesDeleted = 0;

            var settings = JsonHelper.DeserializeFile<Settings>("settings.json");
            _cw.LogDirectory = settings.LogDirectory;

            foreach (var dir in settings.Directories)
            {
                if (dir.TargetDirSize == default(long))
                    dir.TargetDirSize = long.MaxValue; //incase is 0

                Console.WriteLine($"Finding files in {dir.Directory}...");
                var allFiles = Directory.GetFiles(dir.Directory, "*.*", SearchOption.AllDirectories)
                    .Select(f => new FileInfo(f));

                orginalDirSize = allFiles.Sum(f => f.Length);
                currentDirSize = orginalDirSize;
                Console.WriteLine($"Found {allFiles.Count()} files. Total size is {FileHelper.BytesToString(orginalDirSize)} .");

                Console.WriteLine($"Finding files older than {dir.MinDaysOld} days...");
                var files = allFiles
                    .Where(f => f.CreationTime <= DateTime.Today.AddDays(-1 * dir.MinDaysOld))
                    .Where(f => f.LastWriteTime <= DateTime.Today.AddDays(-1 * dir.MinDaysOld))
                    .OrderByDescending(f => f.CreationTime)
                    .ThenByDescending(f => f.LastWriteTime).ToList();
                Console.WriteLine($"Found {files.Count()} that are older than {dir.MinDaysOld} days.");

                for (int i = files.Count() - 1; i >= 0; i--)
                {
                    if (currentDirSize <= dir.TargetDirSize)
                    {
                        Console.WriteLine($"STOPPED SINCE Directory size ({FileHelper.BytesToString(currentDirSize)}) is now less than target size {FileHelper.BytesToString(dir.TargetDirSize)}");

                        break;
                    }
                    else
                    {
                        try
                        {
                            if (dir.DoDelete)
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

                Console.WriteLine($"Original Directory Size: {FileHelper.BytesToString(orginalDirSize)}");

                if (dir.DoDelete)
                {
                    Console.WriteLine($"Current Directory Size: {FileHelper.BytesToString(currentDirSize)}");
                    Console.WriteLine($"{filesDeleted} files deleted. {FileHelper.BytesToString(bytesDeleted)} deleted.");
                }
                else
                {
                    Console.WriteLine($"Projected Directory Size: {FileHelper.BytesToString(currentDirSize)}");
                    Console.WriteLine($"{filesDeleted} files would have been deleted. {FileHelper.BytesToString(bytesDeleted)} would have been deleted.");
                }
                Console.WriteLine("");
            }

            Console.WriteLine("Application finished, will close in 30 seconds.");
            Console.WriteLine("");
            _cw.SaveToDisk();
            Thread.Sleep(30000);
        }  

        static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());

            Console.WriteLine("Application finished, will close in 30 seconds.");
            Console.WriteLine("");
            _cw.SaveToDisk();
            Thread.Sleep(30000);

            Environment.Exit(1);
        }       
    }
}