using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public static class DbContextSeeder
{
    public static async Task SeedContaMasterAsync(CompraProgramadaDbContext context)
    {
        // Verifica se já existe conta master
        var contaMasterExiste = await context.ContasGraficas
            .AnyAsync(c => c.Tipo == TipoConta.Master);

        if (!contaMasterExiste)
        {
            var contaMaster = new ContaGrafica
            {
                NumeroConta = "MASTER-001",
                Tipo = TipoConta.Master,
                DataCriacao = DateTime.UtcNow,
                ClienteId = null // Conta master não tem cliente
            };

            context.ContasGraficas.Add(contaMaster);
            await context.SaveChangesAsync();
        }
    }
}
