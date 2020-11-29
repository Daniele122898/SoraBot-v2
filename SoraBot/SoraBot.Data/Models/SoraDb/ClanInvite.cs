using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot.Data.Models.SoraDb
{
    public class ClanInvite
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ClanId { get; set; }
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong UserId { get; set; }
        
        public virtual Clan Clan { get; set; }
        public virtual User User { get; set; }

    }
}