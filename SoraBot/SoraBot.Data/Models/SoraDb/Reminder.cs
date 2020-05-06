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
        public DateTime DueDateUTC { get; set; }

        public Reminder(ulong userId, string msg, DateTime dueDateUtc)
        {
            this.UserId = userId;
            this.Message = msg;
            this.DueDateUTC = dueDateUtc;
        }

        public Reminder(ulong userId, string msg, TimeSpan dueIn)
        {
            this.UserId = userId;
            this.Message = msg;
            this.DueDateUTC = DateTime.UtcNow.Add(dueIn);
        }

        public virtual User User { get; set; }
    }
}