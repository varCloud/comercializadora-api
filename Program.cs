using System.Text;
using System.Text.Json;
using comercializadora_api.Repositories.Auth;
using comercializadora_api.Repositories.Base;
using comercializadora_api.Pagination;
using comercializadora_api.Repositories.Compras;
using comercializadora_api.Repositories.Dashboard;
using comercializadora_api.Repositories.Estaciones;
using comercializadora_api.Repositories.LimitesInventario;
using comercializadora_api.Repositories.LineasProducto;
using comercializadora_api.Repositories.Productos;
using comercializadora_api.Repositories.Proveedores;
using comercializadora_api.Repositories.RelacionLiquidos;
using comercializadora_api.Repositories.Ubicaciones;
using comercializadora_api.Repositories.Usuarios;
using comercializadora_api.Security;
using comercializadora_api.Services.Auth;
using comercializadora_api.Services.CodigosBarras;
using comercializadora_api.Services.Compras;
using comercializadora_api.Services.Dashboard;
using comercializadora_api.Services.Estaciones;
using comercializadora_api.Services.LimitesInventario;
using comercializadora_api.Services.LineasProducto;
using comercializadora_api.Services.Productos;
using comercializadora_api.Services.Proveedores;
using comercializadora_api.Services.RelacionLiquidos;
using comercializadora_api.Services.Ubicaciones;
using comercializadora_api.Services.Usuarios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// Licencia Community de QuestPDF (gratuita para empresas < 1M USD/año). Debe fijarse al arrancar.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "FrontPolicy";

// Add services to the container.
// Todas las respuestas JSON en camelCase, para que el front no tenga que mapear nombres.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Capa de datos: fábrica de conexiones SQL Server (Repository + Dapper + Stored Procedures).
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

// Repositorios y servicios de dominio.
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuariosRepository, UsuariosRepository>();
builder.Services.AddScoped<IUsuariosService, UsuariosService>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IEstacionesRepository, EstacionesRepository>();
builder.Services.AddScoped<IEstacionesService, EstacionesService>();
builder.Services.AddScoped<IProveedoresRepository, ProveedoresRepository>();
builder.Services.AddScoped<IProveedoresService, ProveedoresService>();
builder.Services.AddScoped<IProductosRepository, ProductosRepository>();
builder.Services.AddScoped<IProductosService, ProductosService>();
builder.Services.AddScoped<ILineasProductoRepository, LineasProductoRepository>();
builder.Services.AddScoped<ILineasProductoService, LineasProductoService>();
builder.Services.AddScoped<ILimitesInventarioRepository, LimitesInventarioRepository>();
builder.Services.AddScoped<ILimitesInventarioService, LimitesInventarioService>();
builder.Services.AddScoped<IComprasRepository, ComprasRepository>();
builder.Services.AddScoped<IComprasService, ComprasService>();
builder.Services.AddScoped<IUbicacionesRepository, UbicacionesRepository>();
builder.Services.AddScoped<IUbicacionesService, UbicacionesService>();
builder.Services.AddScoped<IRelacionLiquidosRepository, RelacionLiquidosRepository>();
builder.Services.AddScoped<IRelacionLiquidosService, RelacionLiquidosService>();
builder.Services.AddSingleton<IUbicacionesPdfService, UbicacionesPdfService>();
builder.Services.AddSingleton<ICodigosBarrasPdfService, CodigosBarrasPdfService>();

// Armado de respuestas paginadas (data/links/meta) a partir de la URL de la request.
builder.Services.AddSingleton<IPaginationBuilder, PaginationBuilder>();

// Seguridad / JWT.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? new JwtSettings();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key.Length > 0 ? jwtSettings.Key : new string('0', 32))),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// CORS para el front Angular. En desarrollo se acepta cualquier origen (el puerto de
// `ng serve` cambia). No se usan credenciales: el JWT viaja en el header Authorization.
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
