using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot_v2.Data.Entities
{
    public class Ban
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong UserId { get; set; }

        public string Reason { get; set; }
        public DateTime BannedAt { get; set; }
    }
}