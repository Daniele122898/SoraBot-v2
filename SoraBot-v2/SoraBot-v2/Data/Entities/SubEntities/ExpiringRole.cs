using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class ExpiringRole
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime ExpiresAt { get; set; }
        
        public ulong UserForeignId { get; set; }
        [ForeignKey("UserForeignId")]
        public virtual User User { get; set; }
        
        public ulong RoleForeignId { get; set; }
        [ForeignKey("RoleForeignId")]
        public virtual Role Role { get; set; }
        
        public ulong GuildForeignId { get; set; }
        [ForeignKey("GuildForeignId")]
        public virtual Guild Guild{ get; set; }
    }
}