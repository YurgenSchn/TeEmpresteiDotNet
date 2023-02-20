using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using EstudoDividas.Constants;

namespace EstudoDividas.Data.MySQL.Entities
{
    [Table("access_level")]
    public class AccessLevel
    {

        [Key]
        [Column("id")]
        public int id { get; set; }

        [Column("role")]
        public string role { get; set; }
    }
}
