using System.ComponentModel.DataAnnotations;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Dtos
{
    public class WaifuRequestAddDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        public WaifuRarity Rarity { get; set; }
        public string UserId { get; set; }
    }
    
    public class WaifuRequestEditDto
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public WaifuRarity Rarity { get; set; }
        public string UserId { get; set; }
        public int RequestId { get; set; }
    }
}