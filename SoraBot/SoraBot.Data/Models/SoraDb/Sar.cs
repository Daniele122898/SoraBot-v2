using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot.Data.Models.SoraDb
{
    public class Sar
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong RoleId { get; set; }

        [Required]
        public ulong GuildId { get; set; }

        public Sar(ulong roleId, ulong guildId)
        {
            this.RoleId = roleId;
            this.GuildId = guildId;
        }

        public virtual Guild Guild { get; set; }
    }
}