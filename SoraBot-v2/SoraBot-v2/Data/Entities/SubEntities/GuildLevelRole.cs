using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class GuildLevelRole
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public ulong RoleId { get; set; }
        public int RequiredLevel { get; set; }
        public bool Banned { get; set; }
        
        public ulong GuildId { get; set; }
        [ForeignKey("GuildId")]
        public virtual Guild Guild{ get; set; }
    }
}