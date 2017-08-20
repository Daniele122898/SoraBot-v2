using System;
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
        
        //User and EP
        public float Exp { get; set; }
        public DateTime CanGainAgain { get; set; }
        public bool Notified { get; set; }
        public bool HasBg { get; set; }
        public DateTime UpdateBgAgain { get; set; }
        public DateTime ShowProfileCardAgain { get; set; }
        
        
        public virtual Interactions Interactions { get; set; }
        public virtual Afk Afk { get; set; }
        public virtual List<Reminders> Reminders { get; set; }
    }
}