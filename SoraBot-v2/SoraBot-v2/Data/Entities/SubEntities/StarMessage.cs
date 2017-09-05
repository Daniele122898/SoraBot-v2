using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class StarMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong MessageId { get; set; }

        public int StarCount { get; set; }
        public byte HitZeroCount { get; set; }
        public bool IsPosted { get; set; }
        
        public ulong GuildForeignId { get; set; }
        [ForeignKey("GuildForeignId")]
        public virtual Guild Guild{ get; set; }
    }
}