using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedReminder.Entities
{
    [Table("erinnerung")]
    public partial class Erinnerung
    {
        public Erinnerung()
        {
            ErinnerungGesendet = new HashSet<ErinnerungGesendet>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("benutzer_id")]
        public int BenutzerId { get; set; }
        [Column("uhrzeit_utc")]
        public DateTime UhrzeitUtc { get; set; }
        [Column("gueltig_ab_datim")]
        public DateTime GueltigAbDatim { get; set; }
        [Column("zusaetzliche_erinnerung")]
        public DateTime? ZusaetzlicheErinnerung { get; set; }

        [ForeignKey(nameof(BenutzerId))]
        [InverseProperty("Erinnerung")]
        public virtual Benutzer Benutzer { get; set; }
        [InverseProperty("Erinnerung")]
        public virtual ICollection<ErinnerungGesendet> ErinnerungGesendet { get; set; }
    }
}
