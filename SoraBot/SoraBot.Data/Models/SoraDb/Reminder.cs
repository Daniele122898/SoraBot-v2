using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoraBot.Data.Models.SoraDb
{
    public class Reminder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public uint Id { get; set; }

        [Required]
        public ulong UserId { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public DateTime DueDateUtc { get; set; }

        public Reminder(ulong userId, string message, DateTime dueDateUtc)
        {
            this.UserId = userId;
            this.Message = message;
            this.DueDateUtc = dueDateUtc;
        }

        public Reminder(ulong userId, string message, TimeSpan dueIn)
        {
            this.UserId = userId;
            this.Message = message;
            this.DueDateUtc = DateTime.UtcNow.Add(dueIn);
        }

        public virtual User User { get; set; }
    }
}