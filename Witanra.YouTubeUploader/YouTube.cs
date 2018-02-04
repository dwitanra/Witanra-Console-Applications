using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Witanra.YouTubeUploader
{
    public static class YouTube
    {
        public const string PrivacyStatus_Unlisted = "unlisted";
        public const string PrivacyStatus_Private = "private";
        public const string PrivacyStatus_Public = "public";

        public static async Task<string> AddVideoAsync(string Title, string Description, List<string> Tags, string CategoryID, string PrivacyStatus, string filename, Action<Google.Apis.Upload.IUploadProgress> ProgressChanged, Action<Video> ResponseReceived)
        {
            var youtubeService = await Task<string>.Run(() => GetYouTubeService().Result);

            var video = new Video();
            video.Snippet = new VideoSnippet();
            video.Snippet.Title = Title;
            video.Snippet.Description = Description;
            video.Snippet.Tags = Tags;
            video.Snippet.CategoryId = CategoryID; // See https://developers.google.com/youtube/v3/docs/videoCategories/list
            video.Status = new VideoStatus();
            video.Status.PrivacyStatus = PrivacyStatus; // "unlisted or "private" or "public"
            var filePath = filename;

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
                videosInsertRequest.ProgressChanged += ProgressChanged;
                videosInsertRequest.ResponseReceived += ResponseReceived;
                await videosInsertRequest.UploadAsync();

                return videosInsertRequest.ResponseBody?.Id;
            }
        }

        public static async Task<IList<PlaylistItem>> GetMyUploadsAsync(string part)
        {
            var youtubeService = await Task<string>.Run(() => GetYouTubeService().Result);

            var result = new List<PlaylistItem>();
            var channelsListRequest = youtubeService.Channels.List("contentDetails");
            channelsListRequest.Mine = true;

            // Retrieve the contentDetails part of the channel resource for the authenticated user's channel.
            var channelsListResponse = await channelsListRequest.ExecuteAsync();

            foreach (var channel in channelsListResponse.Items)
            {
                // From the API response, extract the playlist ID that identifies the list
                // of videos uploaded to the authenticated user's channel.
                var uploadsListId = channel.ContentDetails.RelatedPlaylists.Uploads;

                Console.WriteLine("Videos in Uploads list {0}", uploadsListId);

                var nextPageToken = "";
                while (nextPageToken != null)
                {
                    var playlistItemsListRequest = youtubeService.PlaylistItems.List(part);
                    playlistItemsListRequest.PlaylistId = uploadsListId;
                    playlistItemsListRequest.MaxResults = 50;
                    playlistItemsListRequest.PageToken = nextPageToken;

                    // Retrieve the list of videos uploaded to the authenticated user's channel.
                    var playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();

                    result.AddRange(playlistItemsListResponse.Items);

                    nextPageToken = playlistItemsListResponse.NextPageToken;
                }
            }

            return result;
        }

        public static async Task<IList<Playlist>> GetPlaylistsAsync(string part)
        {
            var youtubeService = await Task<string>.Run(() => GetYouTubeService().Result);

            var result = new List<Playlist>();
            var nextPageToken = "";
            while (nextPageToken != null)
            {
                var playlistItemsListRequest = youtubeService.Playlists.List(part);
                playlistItemsListRequest.Mine = true;
                playlistItemsListRequest.MaxResults = 50;
                playlistItemsListRequest.PageToken = nextPageToken;

                var playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();

                result.AddRange(playlistItemsListResponse.Items);

                nextPageToken = playlistItemsListResponse.NextPageToken;
            }

            return result;
        }

        public static async void AddVideoToPlaylist(string playlistId, string videoId)
        {
            if (videoId == null || playlistId == null)
                return;

            var youtubeService = await Task<string>.Run(() => GetYouTubeService().Result);  

            var newPlaylistItem = new PlaylistItem();
            newPlaylistItem.Snippet = new PlaylistItemSnippet();
            newPlaylistItem.Snippet.PlaylistId = playlistId;
            newPlaylistItem.Snippet.ResourceId = new ResourceId();
            newPlaylistItem.Snippet.ResourceId.Kind = "youtube#video";
            newPlaylistItem.Snippet.ResourceId.VideoId = videoId;
            newPlaylistItem = await youtubeService.PlaylistItems.Insert(newPlaylistItem, "snippet").ExecuteAsync();

        }

        public static async Task<string>AddPlaylistAsync(string playlist_name, string playlist_description, string PrivacyStatus)
        {
            var youtubeService = await Task<string>.Run(() => GetYouTubeService().Result);
            var newPlaylist = new Playlist();

            newPlaylist.Snippet = new PlaylistSnippet();
            newPlaylist.Snippet.Title = playlist_name;
            newPlaylist.Snippet.Description = playlist_description;
            newPlaylist.Status = new PlaylistStatus();
            newPlaylist.Status.PrivacyStatus = PrivacyStatus;
            newPlaylist = await youtubeService.Playlists.Insert(newPlaylist, "snippet,status").ExecuteAsync();
            return newPlaylist.Id;
        }

        private static async Task<YouTubeService> GetYouTubeService()
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows an application to upload files to the                   
                    new[] { YouTubeService.Scope.YoutubeUpload, YouTubeService.Scope.YoutubeReadonly, YouTubeService.Scope.Youtube },
                    "user",
                    CancellationToken.None,
                    null // to revoke token look in %appdata%/Google.Apis.Auth
                );
            }

            return new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Assembly.GetExecutingAssembly().GetName().Name
            });
        }


    }
}
