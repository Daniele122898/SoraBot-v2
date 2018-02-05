using System.Collections.Generic;

namespace SoraBot_v2.WebApiModels
{
    public class WebStarboard
    {
        public string StarChannelId { get; set; }
        public int StarMinimum { get; set; }
        public List<WebGuildChannel> Channels { get; set; } = new List<WebGuildChannel>();
    }

    public class WebStarboardEdit
    {
	    public string ChannelId { get; set; }
	    public string GuildId { get; set; }
	    public int StarMin { get; set; }
	    public bool Disabled { get; set; }
	    public string UserId { get; set; }
    }
}