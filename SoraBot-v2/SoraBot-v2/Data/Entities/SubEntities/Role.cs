using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong RoleId { get; set; }
        public int Cost { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool CanExpire { get; set; }
        
        public ulong GuildForeignId { get; set; }
        [ForeignKey("GuildForeignId")]
        public virtual Guild Guild{ get; set; }
    }
}