using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot.Data.Models.SoraDb
{
    /// <summary>
    /// It is of uttmost importance to use the lower user ID as parter1
    /// </summary>
    public class Marriage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Partner1 { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Partner2 { get; set; }
        
        [Required]
        public DateTime PartnerSince { get; set; } = DateTime.UtcNow;

        public virtual User Partner1User { get; set; }
        public virtual User Partner2User { get; set; }
    }
}