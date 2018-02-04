using System;
using System.IO;
using Witanra.Shared;

namespace Witanra.Security
{
    class Program
    {
        static void Main(string[] args)
        {
            var sw = ConsoleHelper.Start("Application Start");

            var settings = JsonHelper.DeserializeFile<Settings>("settings.json");

            ConsoleHelper.WriteLine($"Found { settings.Folders.Count} Folder in Settings");

            var today = DateTime.Today.ToString(settings.DateFormat);

            foreach (NameDir m in settings.Folders)
            {
                FileHelper.MoveFileByNameAndDate(m.Name, settings.DateFormat, m.Dir, settings.DestinationDir, 30);

                var Dirs = Directory.GetDirectories(Path.Combine(settings.DestinationDir, m.Name));
                foreach (var dir in Dirs)
                {
                    if (dir.Contains(today))
                        break;

                    string[] files = Directory.GetFiles(dir, "*.jpg");
                    if (files.Length > 0)
                    {
                        try { 
                        var dirName = new DirectoryInfo(dir).Name;
                        var filename = FileHelper.GetUniqueFilename(Path.Combine(Directory.GetParent(dir).FullName, m.Name + "_" + dirName + ".mp4"));
                        VideoHelper.MakeVideoFromImages(dir, filename, settings.TempDir);

                        if (new FileInfo(filename).Length > 0)
                        {
                            foreach (var f in files)
                            {
                                File.Delete(f);
                            }
                        }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                FileHelper.DeleteDirIfEmpty(m.Dir);
            }

            FileHelper.DeleteDirIfEmpty(settings.DestinationDir);

            ConsoleHelper.Stop("Application Done", sw, 10000);
        }
    }
}
