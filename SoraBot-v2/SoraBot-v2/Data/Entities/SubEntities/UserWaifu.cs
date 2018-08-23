using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class UserWaifu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }

        public int Count { get; set; }
        
        public int WaifuForeignId { get; set; }
        [ForeignKey("WaifuForeignId")]
        public virtual Waifu Waifu{ get; set; }
        
        public ulong UserForeignId { get; set; }
        [ForeignKey("UserForeignId")]
        public virtual User User{ get; set; }
    }
}