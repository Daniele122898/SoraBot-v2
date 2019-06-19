using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot_v2.Data.Entities.SubEntities
{
    public class RequestLog
    {
    
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public bool Accepted { get; set; }
        public ulong UserId { get; set; }
        public string WaifuName { get; set; }
        public DateTime ProcessedTime { get; set; }
    }
}