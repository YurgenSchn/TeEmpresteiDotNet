﻿using EstudoDividas.Constants;
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
        public LoginResponseContract login(LoginRequestContract request)
        {
            // checar se já existe ao menos 1 usuário com este email, depois checar
            var user = _context.User.FromSql($"SELECT * FROM user WHERE email = {request.email} AND password = {ToSHA384(request.password)};").ToList().FirstOrDefault();
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
            var roleName = _context.AccessLevel.Where(x => x.id.Equals(user.id_access_level)).FirstOrDefault();
            if (roleName == null)
            {
                return new()
                {
                    status = "invalid_role",
                    message = "Usuário Corrompido.",
                    //empty fields
                };
            }

            // testar se o token pode ser criado usando o role obtido
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
                token = token
            };

        }

        public RegisterUserResponseContract registerUser(RegisterUserRequestContract request)
        {
            // checar se já existe ao menos 1 usuário com este email
            var user = _context.User.FromSql($"SELECT * FROM user WHERE email = {request.email};").ToList().FirstOrDefault();
            if (user != null)
                return new()
                {
                    status = "existing_email",
                    message = "Usuário já cadastrado.",
                };

            // Criar novo usuário

            // Primeiro, gerar os ids publico e privado
            var ids = GenerateUserIds();

            // Depois, buscar o ID da ROLE
            var idAccessLevel = _context.AccessLevel.Where(x => x.role.Equals(Roles.usuario)).FirstOrDefault();

            if (idAccessLevel == null)
                return new()
                {
                    status = "internal_role_error",
                    message = "Internal error.",
                };

            User newUser = new()
            {
                id_private = ids[0],
                id_public = ids[1],
                email = request.email,
                name = request.name,         // TODO - Hash das credenciais de login
                password = ToSHA384(request.password), //        antes de enviar ao banco de dados
                id_access_level = idAccessLevel.id
            };

            // Adicionar registro ao banco de dados
            _context.User.Add(newUser);
            _context.SaveChanges();

            // Retornar reponse ao controller
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

            var isValidRole = Roles.roles.Where(e => e.Equals(role)).Any();

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
            string full_length_64 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
            return full_length_64.Substring(0, Math.Min(length, full_length_64.Length));
            // Cada caractere é 6 bits de dados.
            // 6   * 64  = 384 bits
            // 384 / 8   = 48  bytes
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