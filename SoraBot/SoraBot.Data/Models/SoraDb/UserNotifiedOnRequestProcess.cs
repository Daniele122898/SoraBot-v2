using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot.Data.Models.SoraDb
{
    public class UserNotifiedOnRequestProcess
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong UserId { get; set; }

        public UserNotifiedOnRequestProcess(ulong userId)
        {
            this.UserId = userId;
        }
    }
}