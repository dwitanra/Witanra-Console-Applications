using Google.Apis.Upload;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Witanra.Shared;

namespace Witanra.YouTubeUploader
{
    class Program
    {
        private static ConsoleWriter _cw;

        private static long _totalBytes;
        private static DateTime _startTime;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            _cw = new ConsoleWriter();
            Console.SetOut(_cw);

            var settings = JsonHelper.DeserializeFile<Settings>("settings.json");
            _cw.LogDirectory = settings.LogDirectory;

            try
            {
                Console.WriteLine("Getting Video list...");
                var YouTubeVideos = YouTube.GetMyUploadsAsync("snippet").Result;
                Console.WriteLine($"Found {YouTubeVideos.Count} Video{((YouTubeVideos.Count != 1) ? "s" : "")}.");
                //foreach (var playlistItem in YouTubeVideos)
                //{
                //    Console.WriteLine("{0} ({1})", playlistItem.Snippet.Title, playlistItem.Snippet.ResourceId.VideoId);
                //}

                Console.WriteLine("Getting Playlist list...");
                var YouTubePlaylist = YouTube.GetPlaylistsAsync("snippet").Result;
                Console.WriteLine($"Found {YouTubePlaylist.Count} Playlist{((YouTubePlaylist.Count != 1) ? "s" : "")}");

                Console.WriteLine($"Getting Files in {settings.Directory}...");
                DirectoryInfo dir = new DirectoryInfo(settings.Directory);
                List<FileInfo> filesInDir = new List<FileInfo>();

                foreach (var filetype in settings.FileTypes)
                {
                    filesInDir.AddRange(dir.GetFiles($"*{filetype}", SearchOption.AllDirectories));
                }

                if (settings.FileNameIgnore?.Count > 0)
                {
                    foreach (var item in filesInDir.ToList())
                    {
                        foreach (var ignore in settings.FileNameIgnore)
                        {
                            if (item.FullName.ToLower().Contains(ignore))
                            {
                                filesInDir.Remove(item);
                                break;
                            }

                        }
                    }
                }

                filesInDir = filesInDir.OrderByDescending(p => p.CreationTime).ToList();
                Console.WriteLine($"Found {filesInDir.Count} File{((filesInDir.Count != 1) ? "s" : "")} " +
                    $" matching these extensions: {String.Join(", ", settings.FileTypes.ToArray())}" +
                    $" that doesn't contain: {String.Join(", ", settings.FileNameIgnore?.ToArray())}");

                Console.WriteLine($"Loading and Generating File Cache...");
                var fileCache = LoadFileCacheList(settings.CacheFile);
                foreach (var f in filesInDir)
                {
                    var fileDetail = FindFileDetail(fileCache, f.FullName);

                    if (fileDetail == null)
                    {
                        Console.WriteLine($"{f.FullName} is new!");
                        fileDetail = new FileDetail(f, GetMD5(f.FullName));
                        fileCache.Add(fileDetail);
                    }
                    else
                    {
                        if (fileDetail.IsMatch(f))
                        {
                            //Console.WriteLine($"{f.FullName} hasn't changed");
                        }
                        else
                        {
                            Console.WriteLine($"{f.FullName} has changed!");
                            fileCache.Remove(fileDetail);
                            fileDetail = new FileDetail(f, GetMD5(f.FullName));
                            fileCache.Add(fileDetail);
                        }
                    }
                }
                SaveFileCacheList(fileCache, settings.CacheFile);
                Console.WriteLine($"File Cache has {fileCache.Count} File{((fileCache.Count != 1) ? "s" : "")}. Saved to {settings.CacheFile}");

                Console.WriteLine("Figuring out what video files need to be uploaded...");
                var filesToUpload = new List<FileDetail>();
                long sizeToUpload = 0;
                foreach (var f in fileCache)
                {
                    var doUpload = true;
                    if (File.Exists(f.Filename))
                    {
                        foreach (var upload in YouTubeVideos)
                        {
                            if (upload.Snippet.Description.Contains(f.MD5))
                            {
                                doUpload = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        doUpload = false;
                    }

                    if (doUpload)
                    {
                        if (filesToUpload.Count + 1 > settings.UploadLimitCount)
                        {
                            Console.WriteLine($"Upload limit count reached. {settings.UploadLimitCount}");
                            break;
                        }

                        if (sizeToUpload + f.Size > settings.UploadLimitSize)
                        {
                            Console.WriteLine($"Upload limit size would be exceeded. {FileHelper.BytesToString(sizeToUpload + f.Size)} greater than {FileHelper.BytesToString(settings.UploadLimitSize)}");
                            break;
                        }

                        filesToUpload.Add(f);
                        sizeToUpload += f.Size;
                    }
                }
                Console.WriteLine($"Found {filesToUpload.Count} Video File{((filesToUpload.Count != 1) ? "s" : "")} that need to be uploaded.");


                int i = 0;
                foreach (var f in filesToUpload)
                {
                    i++;

                    try
                    {
                        Console.WriteLine($"Uploading {i} of {filesToUpload.Count} : {f.Filename}...");

                        //Console.WriteLine(ReplaceVariables(settings.Title, f, settings.Program_Guid));
                        //Console.WriteLine(ReplaceVariables(settings.Description, f, settings.Program_Guid));
                        //Console.WriteLine(ReplaceVariables(settings.Tags, f, settings.Program_Guid));

                        _totalBytes = f.Size;
                        _startTime = DateTime.Now;

                        Task<string> t = Task<string>.Run(() => YouTube.AddVideoAsync(
                            ReplaceVariables(settings.Title, f, settings.Program_Guid),
                            ReplaceVariables(settings.Description, f, settings.Program_Guid),
                            ReplaceVariables(settings.Tags, f, settings.Program_Guid),
                            settings.Category,
                            YouTube.PrivacyStatus_Private,
                            f.Filename,
                            ProgressChanged,
                            ResponseReceived
                            ).Result
                        );
                        t.Wait();
                        Console.WriteLine($"Uploaded video {t.Result}");

                        var playlistId = String.Empty;
                        var playlistTitle = ReplaceVariables(settings.PlaylistTitle, f, settings.Program_Guid);
                        Console.WriteLine($"Finding playlist {playlistTitle}...");

                        foreach (var playlist in YouTubePlaylist)
                        {
                            if (playlist.Snippet.Title == playlistTitle)
                            {
                                playlistId = playlist.Id;
                                break;
                            }
                        }

                        if (playlistId == String.Empty)
                        {
                            playlistId = YouTube.AddPlaylistAsync(playlistTitle, ReplaceVariables(settings.PlaylistDescription, f, settings.Program_Guid), settings.PrivacyStatus).Result;
                            YouTubePlaylist = YouTube.GetPlaylistsAsync("snippet").Result;
                        }

                        YouTube.AddVideoToPlaylist(playlistId, t.Result);
                        Console.WriteLine($"Video {t.Result} added to playlist {playlistId}...");
                    }

                    catch (AggregateException ex)
                    {
                        PrintAggregateException(ex);
                    }
                }
            }
            catch (AggregateException ex)
            {
                PrintAggregateException(ex);
            }

            CloseWait();
        }

        private static void PrintAggregateException(AggregateException ex)
        {
            foreach (var e in ex.InnerExceptions)
            {
                Console.WriteLine("Error: " + e.Message);
            }
        }

        private static List<string> ReplaceVariables(List<string> StringVariables, FileDetail filedetail, Guid guid)
        {
            for (int i = 0; i < StringVariables.Count; i++)
            {
                StringVariables[i] = ReplaceVariables(StringVariables[i], filedetail, guid);

            }
            return StringVariables;
        }

        private static string ReplaceVariables(string StringVariable, FileDetail filedetail, Guid Program_Guid)
        {
            StringVariable = StringVariable.Replace("{program_name}", System.AppDomain.CurrentDomain.FriendlyName);
            StringVariable = StringVariable.Replace("{program_version}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            StringVariable = StringVariable.Replace("{program_guid}", Program_Guid.ToString());

            StringVariable = StringVariable.Replace("{computer_name}", Environment.MachineName);

            StringVariable = StringVariable.Replace("{file}", Path.GetFileName(filedetail.Filename));
            StringVariable = StringVariable.Replace("{filepath}", filedetail.Filename);
            StringVariable = StringVariable.Replace("{filesize}", filedetail.Size.ToString());
            StringVariable = StringVariable.Replace("{fileDateCreated}", filedetail.Created.ToString());
            StringVariable = StringVariable.Replace("{fileDateCreated_year}", filedetail.Created.ToString("yyyy"));
            StringVariable = StringVariable.Replace("{fileDateCreated_month}", filedetail.Created.ToString("MM"));
            StringVariable = StringVariable.Replace("{fileDateCreated_day}", filedetail.Created.ToString("dd"));
            StringVariable = StringVariable.Replace("{fileDateModified}", filedetail.Modified.ToString());
            StringVariable = StringVariable.Replace("{fileDateModified_year}", filedetail.Modified.ToString("yyyy"));
            StringVariable = StringVariable.Replace("{fileDateModified_month}", filedetail.Modified.ToString("MM"));
            StringVariable = StringVariable.Replace("{fileDateModified_day}", filedetail.Modified.ToString("dd"));
            StringVariable = StringVariable.Replace("{uploaded_date}", DateTime.Now.ToShortDateString());
            StringVariable = StringVariable.Replace("{uploaded_time}", DateTime.Now.ToShortTimeString());
            StringVariable = StringVariable.Replace("{uploaded_date_year}", DateTime.Now.ToString("yyyy"));
            StringVariable = StringVariable.Replace("{uploaded_date_month}", DateTime.Now.ToString("MM"));
            StringVariable = StringVariable.Replace("{uploaded_date_day}", DateTime.Now.ToString("dd"));
            StringVariable = StringVariable.Replace("{MD5}", filedetail.MD5);

            return StringVariable;
        }

        private static List<FileDetail> LoadFileCacheList(string filename)
        {
            var result = new List<FileDetail>();

            try
            {
                result = JsonHelper.DeserializeFile<List<FileDetail>>(filename);
            }
            catch (Exception e)
            {
                Console.WriteLine("CacheList not valid, Regenerating...");
                Console.WriteLine($"{e.Message}");
                if (File.Exists(filename))
                    File.Delete(filename);
                result = new List<FileDetail>();
            }

            return result;
        }

        private static void SaveFileCacheList(List<FileDetail> list, string filename)
        {
            try
            {
                JsonHelper.SerializeFile(list, filename);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unable to save CacheList: {e.Message}");
            }
        }

        private static FileDetail FindFileDetail(List<FileDetail> fileCache, string filename)
        {
            FileDetail result = null;
            foreach (var f in fileCache)
            {
                if (f.Filename == filename)
                {
                    result = f;
                    break;
                }
            }
            return result;
        }

        private static string GetMD5(string filename)
        {
            Console.WriteLine($"Generating MD5 for {filename}...");
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private static void ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case UploadStatus.Uploading:
                    Console.WriteLine($"{Math.Round((double)progress.BytesSent / _totalBytes * 100, 2)}% uploaded " +
                        $"({FileHelper.BytesToString(progress.BytesSent)}/{FileHelper.BytesToString(_totalBytes)}) at " +
                        $"{ FileHelper.BytesToString(Convert.ToInt64(progress.BytesSent / (DateTime.Now - _startTime).TotalSeconds)) }/s.");
                    break;

                case UploadStatus.Failed:
                    Console.WriteLine($"An error prevented the upload from completing.\n{progress.Exception}");
                    Thread.Sleep(10000);
                    break;
            }
        }

        private static void ResponseReceived(Video video)
        {
            Console.WriteLine($"Video id '{video.Id}' was successfully uploaded.");

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
            CloseWait();
            Environment.Exit(1);
        }

        static void OnProcessExit(object sender, EventArgs e)
        {
            _cw.SaveToDisk();
        }
    }
}
