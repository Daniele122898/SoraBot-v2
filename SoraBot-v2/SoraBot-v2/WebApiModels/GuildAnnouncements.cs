using System.Collections.Generic;

namespace SoraBot_v2.WebApiModels
{
    public class GuildAnnouncements
    {
        public string WelcomeChannelId { get; set; }
        public string LeaveChannelId { get; set; }
        public string WelcomeMessage { get; set; }
        public string LeaveMessage { get; set; }
        public bool EmbedWelcome { get; set; }
        public bool EmbedLeave { get; set; }
        public List<WebGuildChannel> Channels { get; set; } = new List<WebGuildChannel>();
    }

    public class EditAnn
    {
        public string UserId { get; set; }
        public string GuildId { get; set; }
        public string ChannelId { get; set; }
        public string Message { get; set; }
        public bool Embed { get; set; }
        public string Type { get; set; }
    }

    public class WebGuildChannel
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }
}