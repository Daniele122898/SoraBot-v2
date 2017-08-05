using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class Interactions
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InteractionsId { get; set; }
        
        public int Pats { get; set; }
        public int Hugs { get; set; }
        public int Kisses { get; set; }
        public int High5 { get; set; }
        public int Pokes { get; set; }
        public int Slaps { get; set; }
        public int Punches { get; set; }
        
        public ulong UserForeignId { get; set; }
        [ForeignKey("UserForeignId")]
        public virtual User User { get; set; }
    }
}
    
    