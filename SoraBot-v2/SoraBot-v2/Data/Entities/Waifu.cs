using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public enum WaifuRarity
    {
        Common, Uncommon, Rare, Epic, UltimateWaifu
    }
    
    public class Waifu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }

        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public WaifuRarity Rarity { get; set; }
    }
}