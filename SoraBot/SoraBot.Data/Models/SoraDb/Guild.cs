using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot.Data.Models.SoraDb
{
    public class Guild
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }

        [Required] public string Prefix { get; set; }
        
        public ulong? StarboardChannelId { get; set; }
        public uint StarboardThreshold { get; set; } = 1;

        public Guild(ulong id, string prefix = "$")
        {
            this.Id = id;
            if (string.IsNullOrWhiteSpace(prefix)) 
                throw new ArgumentNullException(nameof(prefix));
            this.Prefix = prefix;
        }
    }
}