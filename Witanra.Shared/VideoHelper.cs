﻿using System;
using System.IO;
using System.Linq;

namespace Witanra.Shared
{
    public class VideoHelper
    {
        public static void MakeVideoFromImages(string SourceDirWithImages, string DestinationFile, string TempDir, bool DeleteSourceImages, int MinImagesToMakeVideo, bool OrderByCreationDate = true)
        {
            try
            {
                var files = Directory.GetFiles(SourceDirWithImages, "*.jpg", SearchOption.AllDirectories);

                if (OrderByCreationDate)
                {
                    files = files.OrderBy(f => new FileInfo(f).CreationTime).ToArray();
                }

                if (files.Count() > MinImagesToMakeVideo)
                {
                    Console.WriteLine($"Processing {files.Count()} jpgs files from {SourceDirWithImages}...");
                    TempDir = Path.Combine(TempDir, Path.GetFileNameWithoutExtension(DestinationFile));
                    DestinationFile = FileHelper.GetUniqueFilename(DestinationFile);

                    FileHelper.DeleteDirSafe(TempDir);
                    Directory.CreateDirectory(TempDir);
                    var i = 1;
                    foreach (var file in files)
                    {
                        string oldFile = file;
                        string newFile = TempDir + "\\" + Convert.ToString(i).PadLeft(6, '0') + ".jpg";
                        //Console.WriteLine("Copying file from " + oldFile + " to " + newFile);
                        if (new FileInfo(oldFile).Length > 0)
                        {
                            if (DeleteSourceImages)
                            {
                                File.Move(oldFile, newFile);
                            }
                            else
                            {
                                File.Copy(oldFile, newFile);
                            }
                        }
                        i++;
                    }

                    Console.WriteLine($"Saving Video to {DestinationFile}...");
                    FileHelper.LaunchCommandLineApp(TempDir, "ffmpeg.exe", "-y -framerate 5 -i %06d.jpg -c:v libx264 -r 30 -pix_fmt yuv420p -preset veryfast \"" + DestinationFile + "\"");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to make video. Exception: {ex.Message}");
            }

            FileHelper.DeleteDirSafe(TempDir);
        }
    }
}