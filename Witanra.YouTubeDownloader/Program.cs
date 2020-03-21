using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Witanra.Shared;
using Witanra.YouTubeDownloader.Models;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace Witanra.YouTubeDownloader
{
    class Program
    {
        private static ConsoleWriter _cw;
        private static YoutubeClient _youtubeClient;
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledException);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            _cw = new ConsoleWriter();
            _youtubeClient = new YoutubeClient();
            Console.SetOut(_cw);

            var settings = JsonHelper.DeserializeFile<Settings>("settings.json");
            _cw.LogDirectory = settings.LogDirectory;

            Console.WriteLine($"Found { settings.Downloads.Count} Download Items in Settings");
            foreach (var download in settings.Downloads)
            {
                foreach (var youuTubeURLQueries in download.YouTubeURLQueries)
                {
                    var query = ParseQuery(youuTubeURLQueries);
                    if (query != null)
                    {
                        try
                        {
                            Console.WriteLine($"Getting {query.Type.ToString()} {query.Value}...");
                            var executedQuery = ExecuteQueryAsync(query).GetAwaiter().GetResult();
                            Console.WriteLine($"Got {executedQuery.Query.Type.ToString()} {executedQuery.Title}");
                            Save_Videos(executedQuery, settings, download);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Item was not processed successfully. Exception: {ex.Message}");
                        }
                    }
                }
            }
        }

        private static void Save_Videos(ExecutedQuery executedQuery, Settings settings, SettingDownload settingDownload)
        {
            Directory.CreateDirectory(settingDownload.DownloadDirectory);

            if (settingDownload.CleanDownloadDirectory)
            {
                Directory.Delete(settingDownload.DownloadDirectory, true);
            }

            int videoOrder = 1;
            foreach (var video in executedQuery.Videos)
            {
                var fileName = GetFileName(executedQuery, video, settingDownload, videoOrder);
                if (!File.Exists(fileName))
                    {
                    try
                    {
                        Save_Media(video.Id, settings, settingDownload, fileName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Save_Media. Exception: {ex.Message}");
                    }
                }
                videoOrder++;

            }
        }

        private static string GetFileName(ExecutedQuery executedQuery, Video video, SettingDownload settingDownload, int videoOrder)
        {
            var result =  settingDownload.FileNameTemplate;
            result = result.Replace("{playlistName}", FileHelper.GetSafeFilename(executedQuery.Title));
            result = result.Replace("{playlistNumber}", FileHelper.GetSafeFilename(videoOrder.ToString()));
            result = result.Replace("{videoTitle}", FileHelper.GetSafeFilename(video.Title));
            result = Path.Combine(settingDownload.DownloadDirectory, result);
            
            return result;
        }

        private static void Save_Media(string videoId, Settings settings, SettingDownload settingDownload, string filename)
        {
            var cacheDir = Path.Combine(
                settings.CacheDirectory,
                videoId);
            Directory.CreateDirectory(cacheDir);

            var streamInfoSet = _youtubeClient.GetVideoMediaStreamInfosAsync(videoId).GetAwaiter().GetResult();

            AudioStreamInfo audioStreamInfo;
            VideoStreamInfo videoStreamInfo;

            SelectMediaStreamInfoSet(streamInfoSet, settings, settingDownload, out audioStreamInfo, out videoStreamInfo);

            var audiofilename = Path.Combine(
                settings.CacheDirectory,
                videoId,
                $"{audioStreamInfo.AudioEncoding.ToString()}_({audioStreamInfo.Bitrate.ToString()}).{audioStreamInfo.Container.GetFileExtension()}"
                );

            if (
                !File.Exists(audiofilename) &&
                (
                    settingDownload.MediaType == MediaType.AudioVideo ||
                    settingDownload.MediaType == MediaType.Audio
                )
               )
            {
                DownloadMediaStream(audioStreamInfo, audiofilename, settings);
            }

            var videoFilename = Path.Combine(
                settings.CacheDirectory,
                videoId,
                $"{videoStreamInfo.VideoQuality}_{videoStreamInfo.Resolution.Width}x{videoStreamInfo.Resolution.Height}@{videoStreamInfo.Framerate}_{videoStreamInfo.VideoEncoding}_({videoStreamInfo.Bitrate}).{videoStreamInfo.Container.GetFileExtension()}"
                );

            if (
                !File.Exists(videoFilename) &&
                (
                    settingDownload.MediaType == MediaType.AudioVideo ||
                    settingDownload.MediaType == MediaType.Video
                )
               )
            {
                DownloadMediaStream(videoStreamInfo, videoFilename, settings);
            }

            try
            {
                if (settingDownload.MediaType == MediaType.AudioVideo)
                {
                    var transcode = (audioStreamInfo.Container != videoStreamInfo.Container);
                    var tempFileName = Path.Combine(settings.TempDirectory, Guid.NewGuid().ToString());

                    var args = new List<string>();
                    // Set input files
                    args.Add($"-i \"{videoFilename}\"");
                    args.Add($"-i \"{audiofilename}\"");

                    // Set output format
                    args.Add($"-f {videoStreamInfo.Container.GetFileExtension()}");

                    // Skip transcoding if it's not required
                    if (!transcode)
                        args.Add("-c copy");

                    // Optimize mp4 transcoding
                    if (transcode && string.Equals(videoStreamInfo.Container.GetFileExtension(), "mp4", StringComparison.OrdinalIgnoreCase))
                        args.Add("-preset ultrafast");

                    // Set max threads
                    args.Add($"-threads {Environment.ProcessorCount}");

                    // Disable stdin so that the process will not hang waiting for user input
                    args.Add("-nostdin");

                    // Trim streams to shortest
                    args.Add("-shortest");

                    // Overwrite files
                    args.Add("-y");

                    // Set output file                   
                    args.Add($"\"{tempFileName}\"");
                    Directory.CreateDirectory(Path.GetDirectoryName(tempFileName));
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));
                    Console.WriteLine($"Saving Video to {filename}...");
                    FileHelper.LaunchCommandLineApp(cacheDir, "ffmpeg.exe", string.Join(" ", args));
                    File.Move(tempFileName, filename);
                }

                if (settingDownload.MediaType == MediaType.Audio)
                {
                    File.Move(audiofilename, filename);
                }

                if (settingDownload.MediaType == MediaType.Video)
                {
                    File.Move(videoFilename, filename);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Media Move/Transcode Exception: {ex.Message}");
            }
        }

        private static void SelectMediaStreamInfoSet(MediaStreamInfoSet streamInfoSet, Settings settings, SettingDownload settingDownload, out AudioStreamInfo audioStreamInfo, out VideoStreamInfo videoStreamInfo)
        {
            //Todo make a better selection process
            //by largest container bitrate

            audioStreamInfo = streamInfoSet.Audio
                .Where(s => s.Container == Container.Mp4)
                .OrderByDescending(s => s.Bitrate)
                .First();

            videoStreamInfo = streamInfoSet.Video
               .Where(s => s.Container == Container.Mp4)
               .OrderByDescending(s => s.VideoQuality)
               .ThenByDescending(s => s.Framerate)
               .First();

            if (settingDownload.MediaType == MediaType.Audio)
            {
                audioStreamInfo = streamInfoSet.Audio
                .OrderByDescending(s => s.Bitrate)
                .First();
            }

            if (settingDownload.MediaType == MediaType.Video)
            {
                videoStreamInfo = streamInfoSet.Video
               .OrderByDescending(s => s.VideoQuality)
               .ThenByDescending(s => s.Framerate)
               .First();
            }

        }

        private static void DownloadMediaStream(MediaStreamInfo audioStreamInfo, string filename, Settings settings)
        {
            var tempFileName = Path.Combine(settings.TempDirectory, Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path.GetDirectoryName(tempFileName));
            Directory.CreateDirectory(Path.GetDirectoryName(filename));

            Console.WriteLine($"Downloading Media to {filename}...");
            _youtubeClient.DownloadMediaStreamAsync(audioStreamInfo, tempFileName).GetAwaiter().GetResult();
            File.Move(tempFileName, filename);
        }

        private static Query ParseQuery(string query)
        {
            query = query.Trim();

            // Playlist ID
            if (YoutubeClient.ValidatePlaylistId(query))
            {
                return new Query(QueryType.Playlist, query);
            }

            // Playlist URL
            if (YoutubeClient.TryParsePlaylistId(query, out var playlistId))
            {
                return new Query(QueryType.Playlist, playlistId);
            }

            // Video ID
            if (YoutubeClient.ValidateVideoId(query))
            {
                return new Query(QueryType.Video, query);
            }

            // Video URL
            if (YoutubeClient.TryParseVideoId(query, out var videoId))
            {
                return new Query(QueryType.Video, videoId);
            }

            // Channel ID
            if (YoutubeClient.ValidateChannelId(query))
            {
                return new Query(QueryType.Channel, query);
            }

            // Channel URL
            if (YoutubeClient.TryParseChannelId(query, out var channelId))
            {
                return new Query(QueryType.Channel, channelId);
            }

            // User URL
            if (YoutubeClient.TryParseUsername(query, out var username))
            {
                return new Query(QueryType.User, username);
            }

            // Search
            {
                return new Query(QueryType.Search, query);
            }
        }

        private static async Task<ExecutedQuery> ExecuteQueryAsync(Query query)
        {
            // Video
            if (query.Type == QueryType.Video)
            {
                var video = await _youtubeClient.GetVideoAsync(query.Value);
                var title = video.Title;

                return new ExecutedQuery(query, title, new[] { video });
            }

            // Playlist
            if (query.Type == QueryType.Playlist)
            {
                var playlist = await _youtubeClient.GetPlaylistAsync(query.Value);
                var title = playlist.Title;

                return new ExecutedQuery(query, title, playlist.Videos);
            }

            // Channel
            if (query.Type == QueryType.Channel)
            {
                var channel = await _youtubeClient.GetChannelAsync(query.Value);
                var videos = await _youtubeClient.GetChannelUploadsAsync(query.Value);
                var title = channel.Title;

                return new ExecutedQuery(query, title, videos);
            }

            // User
            if (query.Type == QueryType.User)
            {
                var channelId = await _youtubeClient.GetChannelIdAsync(query.Value);
                var videos = await _youtubeClient.GetChannelUploadsAsync(channelId);
                var title = query.Value;

                return new ExecutedQuery(query, title, videos);
            }

            // Search
            if (query.Type == QueryType.Search)
            {
                var videos = await _youtubeClient.SearchVideosAsync(query.Value, 2);
                var title = query.Value;

                return new ExecutedQuery(query, title, videos);
            }

            throw new ArgumentException($"Could not parse query [{query}].", nameof(query));
        }

        private static void CloseWait()
        {
            Console.WriteLine("Application finished, will close in 30 seconds.");
            Console.WriteLine("");
            _cw.SaveToDisk();
            Thread.Sleep(30000);
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Exception:" + e.ExceptionObject.ToString());
            CloseWait();
            Environment.Exit(1);
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            _cw.SaveToDisk();
        }
    }
}
