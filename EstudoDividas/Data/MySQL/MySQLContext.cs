using Microsoft.EntityFrameworkCore;
using EstudoDividas.Data.MySQL.Entities;

namespace EstudoDividas.Data.MySQL
{
    public class MySQLContext : DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<Friend> Friend { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<AccessLevel> AccessLevel { get; set; }
        public MySQLContext(DbContextOptions<MySQLContext> options) : base(options) { }
    }
}