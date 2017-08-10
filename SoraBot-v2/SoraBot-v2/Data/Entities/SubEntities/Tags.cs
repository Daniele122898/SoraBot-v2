using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class Tags
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TagId { get; set; }

        public string Name { get; set; }
        public string Value { get; set; }
        public ulong CreatorId { get; set; }
        public bool PictureAttachment { get; set; }
        public string AttachmentString { get; set; }
        public bool ForceEmbed { get; set; }
        
        public ulong GuildForeignId { get; set; }
        [ForeignKey("GuildForeignId")]
        public virtual Guild Guild{ get; set; }
        
    }
}