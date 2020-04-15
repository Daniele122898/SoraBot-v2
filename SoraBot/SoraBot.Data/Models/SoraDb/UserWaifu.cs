using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot.Data.Models.SoraDb
{
    public class UserWaifu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong UserId { get; set; }
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int WaifuId { get; set; }
        
        [Required]
        public uint Count { get; set; }

        public UserWaifu(ulong userId, int waifuId, uint count)
        {
            this.UserId = userId;
            this.WaifuId = waifuId;
            this.Count = count;
        }
        
        public virtual User Owner { get; set; }
        public virtual Waifu Waifu { get; set; }
    }
}