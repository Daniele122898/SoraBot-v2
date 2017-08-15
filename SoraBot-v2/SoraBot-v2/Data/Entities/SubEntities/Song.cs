using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class Song
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string  Base64EncodedLink{ get; set; }

        public DateTime Added { get; set; }
        public string Name{ get; set; }
        public ulong RequestorUserId { get; set; }
    }
}