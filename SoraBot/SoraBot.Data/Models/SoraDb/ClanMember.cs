using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot.Data.Models.SoraDb
{
    public class ClanMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong UserId { get; set; }
        
        [Required]
        public int ClanId { get; set; }
        
        public virtual Clan Clan { get; set; }
        public virtual User User { get; set; }
    }
}