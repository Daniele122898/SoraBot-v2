using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class GuildUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public ulong UserId { get; set; }
        public float Exp { get; set; }
        
        public ulong GuildId { get; set; }
        [ForeignKey("GuildId")]
        public virtual Guild Guild{ get; set; }
    }
}