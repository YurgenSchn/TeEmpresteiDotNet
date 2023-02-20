using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EstudoDividas.Data.MySQL.Entities
{
    [Table("payment")]
    public class Payment
    {
        [Key]
        [Column("id")]
        public int id { get; set; }

        [Column("value")]
        public float value { get; set; }

        [Column("description")]
        public string description { get; set; }

        [Column("sender")]
        public string sender { get; set; }     // user.id_publico

        [Column("receiver")]
        public string receiver { get; set; }   // user.id_publico

        [Column("sent_date")]
        public string sent_date { get; set; }

        [Column("confirmed")]
        public bool confirmed { get; set; }

        [Column("confirmed_date")]
        public string? confirmed_date { get; set; }

    }
}
