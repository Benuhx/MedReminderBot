using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedReminder.Entities
{
    [Table("erinnerung_gesendet")]
    public partial class ErinnerungGesendet
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("erinnerung_id")]
        public int ErinnerungId { get; set; }
        [Column("gesendet_um")]
        public DateTime GesendetUm { get; set; }

        [ForeignKey(nameof(ErinnerungId))]
        [InverseProperty("ErinnerungGesendet")]
        public virtual Erinnerung Erinnerung { get; set; }
    }
}
