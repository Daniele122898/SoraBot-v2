using System.Collections.Generic;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.WebApiModels
{
    public class UserWaifusAPI
    {
        public bool Success { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public List<UserWaifuAPI> Waifus { get; set; } = new List<UserWaifuAPI>();
    }

    public class UserWaifuAPI
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public string Rarity { get; set; }
        public WaifuRarity SortRarity { get; set; }
        public string ImageUrl { get; set; }
        public string Name { get; set; }
    }

    public class AllWaifus
    {
        public List<Waifu> Waifus { get; set; } = new List<Waifu>();
    }
}