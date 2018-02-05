using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Data.Entities
{
    public class Clan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public string Name { get; set; }
        public virtual List<User> Members { get; set; }
        public ulong OwnerId { get; set; }
        public bool HasImage { get; set; }
        public string AvatarUrl { get; set; }
        public string Message { get; set; }
        public DateTime Created { get; set; }
    }
}