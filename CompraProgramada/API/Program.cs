using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configure MySQL with EF Core (Oracle MySQL Provider)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CompraProgramadaDbContext>(options =>
    options.UseMySQL(connectionString!));

// Register Repositories
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IContaGraficaRepository, ContaGraficaRepository>();
builder.Services.AddScoped<ICustodiaRepository, CustodiaRepository>();
builder.Services.AddScoped<IHistoricoAporteRepository, HistoricoAporteRepository>();
builder.Services.AddScoped<ICestaRepository, CestaRepository>();
builder.Services.AddScoped<IOrdemCompraRepository, OrdemCompraRepository>();

// Register Services
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddSingleton(new CotacaoServiceOptions
{
    CotacoesPath = builder.Configuration["CotacoesPath"] ?? "cotacoes"
});
builder.Services.AddScoped<ICotacaoService, CotacaoService>();
builder.Services.AddScoped<ICestaService, CestaService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ICompraService, CompraService>();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CompraProgramadaDbContext>();
    await DbContextSeeder.SeedContaMasterAsync(context);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

