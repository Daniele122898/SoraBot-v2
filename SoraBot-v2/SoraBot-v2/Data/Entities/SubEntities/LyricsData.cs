using System.Collections.Generic;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class LyricsData
    {
        public Meta meta { get; set; }
        public Response response { get; set; }
    }

    public class Meta
    {
        public int status { get; set; }
    }

    public class Response
    {
        public List<Hits> hits { get; set; }
    }

    public class Hits
    {
        public string type { get; set; }
        public Result result { get; set; }
    }

    public class Result
    {
        public string full_title { get; set; }
        public string song_art_image_thumbnail_url { get; set; }
        public int id { get; set; }
        public string lyrics_state { get; set; }
        public string url { get; set; }
    }
}