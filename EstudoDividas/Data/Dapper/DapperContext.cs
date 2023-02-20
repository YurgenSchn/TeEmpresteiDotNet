using Dapper;
using EstudoDividas.Data.MySQL.Entities;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Security;

namespace EstudoDividas.Data.Dapper
{
    public class DapperContext
    {
        private readonly MySqlConnection _con;

        public DapperContext()
        {
            _con = new MySqlConnection("server=localhost;port=3306;user=root;password=1234;database=estudodivida;");
        }

        public async Task<int> selectUser(int id)
        {
            string query = $"SELECT id FROM user WHERE id = @idParam;";
            return await _con.QueryFirstOrDefaultAsync<int>(query, new{idParam = id});
        }
    }
}
