using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot.Data.Models.SoraDb
{
    public class Clan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public ulong OwnerId { get; set; }

        public string AvatarUrl { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public int Level { get; set; }

        public virtual ICollection<ClanMember> Members { get; set; }
        public virtual ICollection<ClanInvite> Invites { get; set; }
        public virtual User Owner { get; set; }
    }
}