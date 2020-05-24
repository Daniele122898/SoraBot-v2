using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot.Data.Models.SoraDb
{
    public enum RequestState
    {
        Pending,
        Accepted,
        Rejected
    }
    
    public class WaifuRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }
        [Required]
        public RequestState RequestState { get; set; }
        [Required]
        public DateTime RequestTime { get; set; }

        public DateTime? ProcessedTime { get; set; }
        
        [Required]
        public string Name { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        public WaifuRarity Rarity { get; set; }

        [Required]
        public ulong UserId { get; set; }

        public virtual User User { get; set; }
    }
}