using System.Collections.Generic;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.WebApi.Dtos
{
    public class UserWaifusDto
    {
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public List<UserWaifuDto> Waifus { get; set; }
    }

    public class UserWaifuDto
    {
        public int Id { get; set; }
        public uint Count { get; set; }
        public WaifuRarity Rarity { get; set; }
        public string ImageUrl { get; set; }
        public string Name { get; set; }
    }
}