using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot.Data.Models.SoraDb
{
    public enum WaifuRarity
    {
        Common = 0, 
        Uncommon = 1, 
        Rare = 2, 
        Epic = 3, 
        UltimateWaifu = 99, 
        Halloween = 5, 
        Christmas = 6,
        Summer = 7
    }
    
    public class Waifu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        public WaifuRarity Rarity { get; set; }

        public virtual ICollection<UserWaifu> UserWaifus { get; set; }
    }
}