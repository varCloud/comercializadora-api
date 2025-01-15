using comercializadora_api.Data;
using comercializadora_api.Repository;
using comercializadora_api.Services;
using comercializadora_api.UnitofWork;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//AGREGAMOS UNIT OF WORK PATTERN PARA QUE EL MANEJADOR DE INYECCION DE DEPENCIAS PUEDO USUARLO
builder.Services.AddScoped<IUnitofWork, UnitofWork>();

//builder.Services.AddScoped<RepositoryBase>();
//builder.Services.AddScoped<typeof(IRepository<>),  typeof(RepositoryBase<>)>();
//typeof(IGenericRepository<>), typeof(GenericRepository<>)
builder.Services.AddScoped<UsuariosRepository>();
builder.Services.AddScoped<UsuariosService>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build(); 

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
