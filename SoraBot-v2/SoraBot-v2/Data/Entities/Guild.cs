using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Data.Entities
{
    public class Guild
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong GuildId { get; set; }
        
        public string Prefix { get; set; }
        public bool RestrictTags { get; set; }
        public bool IsDjRestricted { get; set; }
        
        public virtual List<Tags> Tags { get; set; }
    }
}