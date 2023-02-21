using EstudoDividas.Constants;
using EstudoDividas.Data.Dapper;
using EstudoDividas.Data.MySQL;
using EstudoDividas.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();           // Documentação automática
builder.Services.AddAuthentication(x =>     // Autenticação com token
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>

    // Parametros para validar tokens
    // Deve seguir a mesma criptografia do token = Simetrico, com base na chave secreta comum.

    x.TokenValidationParameters = new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key.token_secret)),
        ValidateAudience = false,
        ValidateIssuer = false
    }
) ;

// Adicionar serviços individuais
builder.Services.AddScoped<AuthenticationServices>();
builder.Services.AddScoped<FriendServices>();
builder.Services.AddScoped<PaymentServices>();
//builder.Services.AddScoped<DapperContext>();

// Adicionar a conexão MYSQL - Definido na pasta Data
builder.Services.AddDbContext<MySQLContext>(options =>
    options.UseMySQL("server=localhost;port=3306;user=root;password=1234;database=estudodivida;")
) ;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html"); ;

app.Run();
