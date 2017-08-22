using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class Marriage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong PartnerId { get; set; }

        public DateTime Since { get; set; }
        
        public ulong UserForeignId { get; set; }
        [ForeignKey("UserForeignId")]
        public virtual User User { get; set; }
    }
}