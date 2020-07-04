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
        [Column("telegram_chat_id")]
        public long TelegramChatId { get; set; }
        [Column("zustand")]
        public int Zustand { get; set; }

        public virtual Benutzer TelegramChat { get; set; }
    }
}
