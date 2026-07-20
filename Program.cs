using System.Text;
using System.Text.Json;
using comercializadora_api.Infraestructura.BackgroundTasks;
using comercializadora_api.Repositories.Auth;
using comercializadora_api.Repositories.Base;
using comercializadora_api.Pagination;
using comercializadora_api.Repositories.Bitacoras;
using comercializadora_api.Repositories.Clientes;
using comercializadora_api.Repositories.Compras;
using comercializadora_api.Repositories.ConsumoMpl;
using comercializadora_api.Repositories.Dashboard;
using comercializadora_api.Repositories.Estaciones;
using comercializadora_api.Repositories.Facturas;
using comercializadora_api.Repositories.InventariosFisicos;
using comercializadora_api.Repositories.LimitesInventario;
using comercializadora_api.Repositories.LineasProducto;
using comercializadora_api.Repositories.Productos;
using comercializadora_api.Repositories.ProduccionAgranel;
using comercializadora_api.Repositories.ProduccionLiquidos;
using comercializadora_api.Repositories.ProduccionTrapeadores;
using comercializadora_api.Repositories.Proveedores;
using comercializadora_api.Repositories.RelacionLiquidos;
using comercializadora_api.Repositories.RelacionTrapeadores;
using comercializadora_api.Repositories.ReportesCompras;
using comercializadora_api.Repositories.ReportesDevolucion;
using comercializadora_api.Repositories.ReportesInventario;
using comercializadora_api.Repositories.ReportesMerma;
using comercializadora_api.Repositories.ReportesVentas;
using comercializadora_api.Repositories.TiposCliente;
using comercializadora_api.Repositories.Ubicaciones;
using comercializadora_api.Repositories.Usuarios;
using comercializadora_api.Security;
using comercializadora_api.Services.Auth;
using comercializadora_api.Services.Bitacoras;
using comercializadora_api.Services.Clientes;
using comercializadora_api.Services.CodigosBarras;
using comercializadora_api.Services.Compras;
using comercializadora_api.Services.ConsumoMpl;
using comercializadora_api.Services.Dashboard;
using comercializadora_api.Services.Email;
using comercializadora_api.Services.Estaciones;
using comercializadora_api.Services.Exportacion;
using comercializadora_api.Services.Facturas;
using comercializadora_api.Services.FacturacionPac;
using comercializadora_api.Services.InventariosFisicos;
using comercializadora_api.Services.LimitesInventario;
using comercializadora_api.Services.LineasProducto;
using comercializadora_api.Services.Productos;
using comercializadora_api.Services.ProduccionAgranel;
using comercializadora_api.Services.ProduccionLiquidos;
using comercializadora_api.Services.ProduccionTrapeadores;
using comercializadora_api.Services.Proveedores;
using comercializadora_api.Services.RelacionLiquidos;
using comercializadora_api.Services.RelacionTrapeadores;
using comercializadora_api.Services.ReportesCompras;
using comercializadora_api.Services.ReportesDevolucion;
using comercializadora_api.Services.ReportesInventario;
using comercializadora_api.Services.ReportesMerma;
using comercializadora_api.Services.ReportesVentas;
using comercializadora_api.Services.TiposCliente;
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
builder.Services.AddScoped<IProduccionAgranelRepository, ProduccionAgranelRepository>();
builder.Services.AddScoped<IProduccionAgranelService, ProduccionAgranelService>();
builder.Services.AddScoped<IProduccionLiquidosRepository, ProduccionLiquidosRepository>();
builder.Services.AddScoped<IProduccionLiquidosService, ProduccionLiquidosService>();
builder.Services.AddScoped<IProduccionTrapeadoresRepository, ProduccionTrapeadoresRepository>();
builder.Services.AddScoped<IProduccionTrapeadoresService, ProduccionTrapeadoresService>();
builder.Services.AddScoped<ILimitesInventarioRepository, LimitesInventarioRepository>();
builder.Services.AddScoped<ILimitesInventarioService, LimitesInventarioService>();
builder.Services.AddScoped<IInventarioFisicoRepository, InventarioFisicoRepository>();
builder.Services.AddScoped<IInventarioFisicoService, InventarioFisicoService>();
builder.Services.AddScoped<IComprasRepository, ComprasRepository>();
builder.Services.AddScoped<IComprasService, ComprasService>();
builder.Services.AddScoped<IUbicacionesRepository, UbicacionesRepository>();
builder.Services.AddScoped<IUbicacionesService, UbicacionesService>();
builder.Services.AddScoped<IRelacionLiquidosRepository, RelacionLiquidosRepository>();
builder.Services.AddScoped<IRelacionLiquidosService, RelacionLiquidosService>();
builder.Services.AddScoped<IRelacionTrapeadoresRepository, RelacionTrapeadoresRepository>();
builder.Services.AddScoped<IRelacionTrapeadoresService, RelacionTrapeadoresService>();
builder.Services.AddScoped<IClientesRepository, ClientesRepository>();
builder.Services.AddScoped<IClientesService, ClientesService>();
builder.Services.AddScoped<ITiposClienteRepository, TiposClienteRepository>();
builder.Services.AddScoped<ITiposClienteService, TiposClienteService>();
builder.Services.AddScoped<IConsumoMplRepository, ConsumoMplRepository>();
builder.Services.AddScoped<IConsumoMplService, ConsumoMplService>();
builder.Services.AddScoped<IBitacorasRepository, BitacorasRepository>();
builder.Services.AddScoped<IBitacorasService, BitacorasService>();
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();
builder.Services.AddScoped<IFacturacionService, FacturacionService>();
builder.Services.AddScoped<IInventarioReporteRepository, InventarioReporteRepository>();
builder.Services.AddScoped<IInventarioReporteService, InventarioReporteService>();
builder.Services.AddScoped<IVentaReporteRepository, VentaReporteRepository>();
builder.Services.AddScoped<IVentaReporteService, VentaReporteService>();
builder.Services.AddScoped<IMermaReporteRepository, MermaReporteRepository>();
builder.Services.AddScoped<IMermaReporteService, MermaReporteService>();
builder.Services.AddScoped<IDevolucionReporteRepository, DevolucionReporteRepository>();
builder.Services.AddScoped<IDevolucionReporteService, DevolucionReporteService>();
builder.Services.AddScoped<IComprasReporteRepository, ComprasReporteRepository>();
builder.Services.AddScoped<IComprasReporteService, ComprasReporteService>();
builder.Services.AddSingleton<IUbicacionesPdfService, UbicacionesPdfService>();
builder.Services.AddSingleton<ICodigosBarrasPdfService, CodigosBarrasPdfService>();

// Facturación: cancelación CFDI ante el PAC (SOAP crudo, XML-DSig con el CSD) + consulta de
// estatus ante el SAT. Ver comercializadora-api/.claude/memory (módulo Facturación Ventas).
builder.Services.Configure<FacturacionArchivosOptions>(builder.Configuration.GetSection(FacturacionArchivosOptions.SectionName));
builder.Services.Configure<FacturacionPacOptions>(builder.Configuration.GetSection(FacturacionPacOptions.SectionName));
builder.Services.Configure<CfdiSignatureOptions>(builder.Configuration.GetSection(CfdiSignatureOptions.SectionName));
builder.Services.AddSingleton<ICfdiXmlSignerService, CfdiXmlSignerService>();
builder.Services.AddHttpClient<IFacturacionPacClient, FacturacionPacClient>();

// Exportación transversal a CSV: descarga inmediata (<= umbral) o generación en segundo
// plano + envío por correo (> umbral). Ver comercializadora-api/.claude/arquitectura/exportacion-reportes.md.
builder.Services.Configure<ExportacionOptions>(builder.Configuration.GetSection(ExportacionOptions.SectionName));
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.AddSingleton<IBackgroundTaskQueue>(_ => new BackgroundTaskQueue(capacidad: 50));
builder.Services.AddHostedService<ExportacionQueuedHostedService>();
builder.Services.AddSingleton<ICsvGeneratorService, CsvGeneratorService>();
builder.Services.AddSingleton<IEmailService, SmtpEmailService>();
builder.Services.AddSingleton<IExportacionService, ExportacionService>();

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
