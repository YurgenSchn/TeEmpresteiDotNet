using EstudoDividas.Constants;
using EstudoDividas.Contracts;
using EstudoDividas.Data.MySQL;
using EstudoDividas.Data.MySQL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EstudoDividas.Services
{
    public class AuthenticationServices
    {

        // Construtor
        MySQLContext _context;
        public AuthenticationServices(MySQLContext context)
        {
            _context = context;
        }


        // Funções principais
        public async Task<LoginResponseContract> login(LoginRequestContract request)
        {
            // checar se já existe ao menos 1 usuário com este username, depois checar
            var user = await _context.User.Where(u => u.username.Equals(request.username) && u.password.Equals(ToSHA384(request.password))).FirstOrDefaultAsync();
            if (user == null)
            {
                return new()
                {
                    status = "invalid",
                    message = "Credenciais Inválidas.",
                    //empty fields
                };
            }

            // obter nome do accesslevel (role)
            var roleName = await _context.AccessLevel.Where(u => u.id.Equals(user.id_access_level)).FirstOrDefaultAsync();
            if (roleName == null)
            {
                return new()
                {
                    status = "invalid_role",
                    message = "Usuário Corrompido. Access Level inválido.",
                    //empty fields
                };
            }

            // CRIAR O TOKEN COM A ROLE DO BANCO DE DADOS
            string token = CreateToken(roleName.role);

            if (string.IsNullOrEmpty(token))
            {
                return new()
                {
                    status = "invalid_role",
                    message = "Usuário Corrompido.",
                    //empty fields
                };
            }

            // Retornar usuário
            return new()
            {
                status = "ok",
                message = "Logado com sucesso!",
                idPublic = user.id_public,
                idPrivate = user.id_private,
                name = user.name,
                username = user.username,
                token = token
            };

        }

        public async Task<RegisterUserResponseContract> registerUser(RegisterUserRequestContract request)
        {
            // CHAMADAS ASSÍNCRONAS
            // a. checar se já existe ao menos 1 usuário com este email
            // b. checar se já existe ao menos 1 usuário com este username
            // b. checar se já a role está cadastrada no banco de dados
            var taskEmailIsUsed   = _context.User.Where(u => u.email.Equals(request.email)).AnyAsync();
            var taskUsernameIsUsed = _context.User.Where(u => u.email.Equals(request.email)).AnyAsync();
            var taskIdAccessLevel = _context.AccessLevel.Where(a => a.role.Equals(Roles.usuario)).FirstOrDefaultAsync();


            // RETORNO ASSÍNCRONO
            var emailIsUsed    = await taskEmailIsUsed;
            var usernameIsUsed = await taskUsernameIsUsed;
            var idAccessLevel  = await taskIdAccessLevel;

            // VALIDAÇÕES DE EMAIL E ROLE NO BANCO
            if (emailIsUsed)
                return new()
                {
                    status = "existing_email",
                    message = "Email já cadastrado.",
                };

            if (usernameIsUsed)
                return new()
                {
                    status = "existing_username",
                    message = "Apelido já cadastrado."
                };

            if (idAccessLevel == null)
                return new()
                {
                    status = "internal_role_error",
                    message = "Internal error - registro de access_level 'usuario' ausente na base de dados.",
                };


            // CRIAR NOVO USUÁRIO
            var ids = GenerateUserIds();

            User newUser = new()
            {
                id_private = ids[0],
                id_public = ids[1],
                email = request.email,
                name = request.name,         
                username = request.username,
                password = ToSHA384(request.password),
                id_access_level = idAccessLevel.id
            };

            // ADICIONAR AO BANCO - Não precisa de await depois de salvar
            _context.User.Add(newUser);
            _context.SaveChangesAsync();  

            return new()
            {
                status = "ok",
                message = "Usuário cadastrado com sucesso!",
            };
        }



        // Funções utilitárias
        private string[] GenerateUserIds()
        {
            // Essa função retorna dois IDs, privado e publico
            // [private, public]

            string idPrivate = "";
            bool idPrivateIsValid = false;

            string idPublic = "";
            bool idPublicIsValid = false;

            // Os loops internos são para gerarem ID's até que sejam únicos (que não existam no banco).
            while (!idPrivateIsValid)
            {
                idPrivate = GenerateRandomString(16);
                var existent = _context.User.Where(row => row.id_private.Equals(idPrivate)).FirstOrDefault();
                idPrivateIsValid = existent == null;
            }

            while (!idPublicIsValid)
            {
                idPublic = GenerateRandomString(16);
                var existent = _context.User.Where(row => row.id_public.Equals(idPublic)).FirstOrDefault();
                idPublicIsValid = (existent == null) && (idPublic != idPrivate);
            }

            // Retorna em um array
            string[] ids = { idPrivate, idPublic };
            return ids;

        }

        public static string CreateToken(string role)
        {
            // CRIAR LISTA DE CLAIMS
            // Claims são "afirmativas" sobre o token
            // Só adicionaremos uma claim: o ROLE dele, nivel de acesso

            var isValidRole = Roles.roles.Where(r => r.Equals(role)).Any();

            if (!isValidRole)
            {
                return "";
            }

            List<Claim> claims = new();
            claims.Add(new Claim(ClaimTypes.Role, role));


            // Objeto com funções de manipulação de token
            JwtSecurityTokenHandler tokenHandler = new();

            // Montando o token
            var key = Encoding.UTF8.GetBytes(Key.token_secret);
            SecurityTokenDescriptor description = new()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            // Retornar o token
            var token = tokenHandler.CreateToken(description);
            return tokenHandler.WriteToken(token);

        }

        public static string GenerateRandomString(int length) //max length = 64
        {
            // Cada caracter é 6 bits
            int necessaryBytes  = length * 6;
            // Fazendo a conversão para bytes (8 bits), mas sem descartar casa decimal.
            necessaryBytes = (int)(Math.Ceiling(necessaryBytes / 8f)); 

            string randomString = Convert.ToBase64String(RandomNumberGenerator.GetBytes(necessaryBytes));
            return randomString.Substring(0, Math.Min(length, randomString.Length));
        }

        public static string ToSHA384(string value)
        {
            string salted = value + Key.salt;
            HMACSHA384 hmac = new HMACSHA384(Encoding.ASCII.GetBytes(Key.hash_secret));
            var bytes = Encoding.ASCII.GetBytes(salted);
            var hash = hmac.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
            // referencia: https://stackoverflow.com/questions/65751399/unable-to-generate-the-correct-signature-using-hmacsha256
        }
    }
}
