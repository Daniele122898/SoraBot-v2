using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot.Data.Models.SoraDb
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }
        [Required]
        public uint Coins { get; set; } = 0;
        [Required]
        public DateTime LastDaily { get; set; } = DateTime.UnixEpoch;
    }
}