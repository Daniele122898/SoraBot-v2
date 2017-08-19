using System.Collections.Generic;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class GiphyData
    {
        public List<Data> data { get; set; }
    }

    public class Data
    {
        public Images images { get; set; }
    }

    public class Images
    {
        public Original original { get; set; }
    }

    public class Original
    {
        public string url { get; set; }
    }
}