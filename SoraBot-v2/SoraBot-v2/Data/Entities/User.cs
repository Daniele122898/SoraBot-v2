﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Dynamic;
using SoraBot_v2.Data.Entities.SubEntities;


namespace SoraBot_v2.Data.Entities
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong UserId { get; set; }
        
        // User and EP
        public float Exp { get; set; }
        public DateTime CanGainAgain { get; set; }
        public bool Notified { get; set; }
        public bool HasBg { get; set; }
        public DateTime UpdateBgAgain { get; set; }
        public DateTime ShowProfileCardAgain { get; set; }
        public int Money { get; set; }
        public DateTime LastDailyClaim { get; set; }
        public int FavoriteWaifu { get; set; }
        
        
        public virtual Interactions Interactions { get; set; }
        public virtual Afk Afk { get; set; }
        public virtual List<Reminders> Reminders { get; set; }
        public virtual List<Marriage> Marriages { get; set; }
        public virtual List<ShareCentral> ShareCentrals { get; set; }
        public virtual List<Voting> Votings { get; set; }
        public virtual List<UserWaifu> UserWaifus { get; set; }
        // Clan
        public string ClanName { get; set; }
        public bool ClanStaff { get; set; }
        public DateTime JoinedClan { get; set; }
    }
}