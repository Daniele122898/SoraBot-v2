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
        
        public virtual Interactions Interactions { get; set; }
    }
}