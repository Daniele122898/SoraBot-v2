using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot.Data.Models.SoraDb
{
    public class GuildUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong UserId { get; set; }
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong GuildId { get; set; }
        
        [Required]
        public uint Exp { get; set; }

        public GuildUser(ulong userId, ulong guildId, uint exp)
        {
            this.UserId = userId;
            this.GuildId = guildId;
            this.Exp = exp;
        }

        public virtual User User { get; set; }
        public virtual Guild Guild { get; set; }
    }
}