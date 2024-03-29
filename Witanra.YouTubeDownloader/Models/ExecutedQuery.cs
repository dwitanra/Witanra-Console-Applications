﻿using System.Collections.Generic;
using YoutubeExplode.Videos;

namespace Witanra.YouTubeDownloader.Models
{
    public class ExecutedQuery
    {
        public Query Query { get; }

        public string Title { get; }

        public IReadOnlyList<Video> Videos { get; }

        public ExecutedQuery(Query query, string title, IReadOnlyList<Video> videos)
        {
            Query = query;
            Title = title;
            Videos = videos;
        }
    }
}