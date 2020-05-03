using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot.Data.Models.SoraDb
{
    public class StarboardMessage
    {
        [Key] 
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong MessageId { get; set; }

        [Required]
        public ulong PostedMsgId { get; set; }

        // While this is not necessary for the ACTUAL usage
        // it is however needed to make sure it gets removed on cascade
        [Required]
        public ulong GuildId { get; set; }

        public StarboardMessage(ulong messageId, ulong postedMsgId, ulong guildId)
        {
            this.MessageId = messageId;
            this.PostedMsgId = postedMsgId;
            this.GuildId = guildId;
        }

        public virtual Guild Guild { get; set; }
    }
}