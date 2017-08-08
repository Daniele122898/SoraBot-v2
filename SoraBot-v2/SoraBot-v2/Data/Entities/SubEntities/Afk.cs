using System;
 using System.ComponentModel.DataAnnotations;
 using System.ComponentModel.DataAnnotations.Schema;
 namespace SoraBot_v2.Data.Entities.SubEntities
 {
     public class Afk
     {
         [Key]
         [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
         public int AfkId { get; set; }
         
         public bool IsAfk { get; set; }
         public DateTime TimeToTriggerAgain { get; set; }
         public string Message { get; set; }
         
         public ulong UserForeignId { get; set; }
         [ForeignKey("UserForeignId")]
         public virtual User User { get; set; }
     }
 }