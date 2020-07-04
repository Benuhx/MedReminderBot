using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedReminder.Entities
{
    [Table("erinnerung")]
    public partial class Erinnerung
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("benutzer_id")]
        public int BenutzerId { get; set; }
        [Column("uhrzeit_utc")]
        public DateTime UhrzeitUtc { get; set; }

        [ForeignKey(nameof(BenutzerId))]
        [InverseProperty("Erinnerung")]
        public virtual Benutzer Benutzer { get; set; }
    }
}
