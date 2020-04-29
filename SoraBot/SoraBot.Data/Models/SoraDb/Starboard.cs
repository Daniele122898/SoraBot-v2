using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot.Data.Models.SoraDb
{
    public class Starboard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong GuildId { get; set; }

        [Required]
        public ulong StarboardChannelId { get; set; }
        [Required]
        public uint StarboardThreshold { get; set; }

        public virtual Guild Guild { get; set; }

        public Starboard(ulong guildId, ulong starboardChannelId, uint starboardThreshold = 1)
        {
            this.GuildId = guildId;
            this.StarboardChannelId = starboardChannelId;
            this.StarboardThreshold = starboardThreshold;
        }

    }
}