using System;
using System.Collections.Generic;
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
        public int? FavoriteWaifuId { get; set; }
        public uint Exp { get; set; } = 0;
        public bool HasCustomProfileBg { get; set; } = false;
        
        public virtual ICollection<UserWaifu> UserWaifus { get; set; }
        public virtual ICollection<GuildUser> GuildUsers { get; set; }
        public virtual ICollection<Reminder> Reminders { get; set; }
        public virtual Waifu FavoriteWaifu { get; set; }

    }
}