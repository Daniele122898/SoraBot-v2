using SoraBot.Data.Models.SoraDb;

namespace SoraBot.WebApi.Dtos
{
    public class WaifuDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public WaifuRarity Rarity { get; set; }
    }
}