using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoraBot_v2.Services;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class ModCase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CaseId{ get; set; }

        public ModService.Case Type{ get; set; }
        public int CaseNr { get; set; }
        public ulong ModId{ get; set; }
        public ulong UserId { get; set; }
        public string UserNameDisc { get; set; }
        public string Reason { get; set; }
        public ulong PunishMsgId { get; set; }
        public int WarnNr { get; set; }
        
        public ulong GuildForeignId { get; set; }
        [ForeignKey("GuildForeignId")]
        public virtual Guild Guild { get; set; }
    }
}