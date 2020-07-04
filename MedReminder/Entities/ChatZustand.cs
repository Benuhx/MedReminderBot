using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedReminder.Entities
{
    [Table("chat_zustand")]
    public partial class ChatZustand
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("chat_id")]
        public long ChatId { get; set; }
        [Column("zustand")]
        public int Zustand { get; set; }

        public virtual Benutzer Benutzer { get; set; }
    }
}
