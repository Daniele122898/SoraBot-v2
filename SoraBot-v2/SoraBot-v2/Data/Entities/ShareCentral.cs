using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot_v2.Data.Entities
{
    public class ShareCentral
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ShareLink{ get; set; } //hastebin link

        public int Upvotes { get; set; }
        public int Downvotes { get; set; }
        public string Tags { get; set; }
        public string Titel { get; set; }
        public bool IsPrivate { get; set; }
        
        public ulong CreatorId { get; set; }
        [ForeignKey("CreatorId")]
        public virtual User User { get; set; }
    }
}