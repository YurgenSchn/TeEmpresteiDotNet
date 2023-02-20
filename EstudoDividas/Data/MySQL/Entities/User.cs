using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EstudoDividas.Data.MySQL.Entities
{
    [Table("user")]
    public class User
    {
        [Key]
        [Column("id")]
        public int id { get; set; }

        [Column("id_public")]
        public string id_public { get; set; }

        [Column("id_private")]
        public string id_private { get; set; }

        [Column("name")]
        public string name { get; set; }
        [Column("email")]
        public string email { get; set; }
        [Column("password")]
        public string password { get; set; }

        [Column("id_access_level")]
        public int id_access_level { get; set; }
    }
}

