﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedReminder.Entities
{
    [Table("benutzer")]
    public partial class Benutzer
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("name", TypeName = "character varying")]
        public string Name { get; set; }
        [Column("telegram_chat_id")]
        public long TelegramChatId { get; set; }

        public virtual ChatZustand ChatZustand { get; set; }
    }
}