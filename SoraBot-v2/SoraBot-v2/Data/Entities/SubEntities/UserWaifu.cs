using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class UserWaifu
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int Count { get; set; }
        
        public int WaifuId { get; set; }
        
        public ulong UserForeignId { get; set; }
        [ForeignKey("UserForeignId")]
        public virtual User User{ get; set; }
    }
}