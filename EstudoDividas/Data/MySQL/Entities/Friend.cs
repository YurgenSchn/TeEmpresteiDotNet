using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EstudoDividas.Data.MySQL.Entities
{
    [Table("friend")]
    public class Friend
    {
        [Key]
        [Column("id")]
        public int id { get; set; }

        [Column("sender")]
        public string sender { get; set; }

        [Column("receiver")]
        public string receiver { get; set; }

        [Column("confirmed")]
        public bool confirmed { get; set; }

        [Column("confirmed_date")]
        public string confirmed_date { get; set; } // CONVERSION MIGHT BE NECESSARY

    }
}
