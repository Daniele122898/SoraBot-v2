using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class Voting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VoteId{ get; set; }

        public string ShareLink { get; set; }
        [ForeignKey("ShareLink")]
        public virtual ShareCentral ShareCentral { get; set; }
        
        public ulong VoterId{ get; set; }
        [ForeignKey("VoterId")]
        public virtual User User { get; set; }

        public bool UpOrDown { get; set; }//false/0 down true/1 up
    }
}