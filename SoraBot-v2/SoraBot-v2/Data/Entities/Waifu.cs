using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot_v2.Data.Entities.SubEntities
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

        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public WaifuRarity Rarity { get; set; }
    }
}