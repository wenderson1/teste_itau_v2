using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class CompraProgramadaDbContext : DbContext
{
    public CompraProgramadaDbContext(DbContextOptions<CompraProgramadaDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ContaGrafica> ContasGraficas => Set<ContaGrafica>();
    public DbSet<Custodia> Custodias => Set<Custodia>();
    public DbSet<HistoricoAporte> HistoricoAportes => Set<HistoricoAporte>();
    public DbSet<DistribuicaoAporte> DistribuicaoAportes => Set<DistribuicaoAporte>();
    public DbSet<Cesta> Cestas => Set<Cesta>();
    public DbSet<ItemCesta> ItensCesta => Set<ItemCesta>();
    public DbSet<OrdemCompra> OrdensCompra => Set<OrdemCompra>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CPF).HasMaxLength(11).IsRequired();
            entity.HasIndex(e => e.CPF).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ValorMensal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Ativo).HasDefaultValue(true);

            entity.HasOne(e => e.ContaGrafica)
                  .WithOne(c => c.Cliente)
                  .HasForeignKey<ContaGrafica>(c => c.ClienteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ContaGrafica
        modelBuilder.Entity<ContaGrafica>(entity =>
        {
            entity.ToTable("ContasGraficas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NumeroConta).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.NumeroConta).IsUnique();
            entity.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(10);
        });

        // Custodia
        modelBuilder.Entity<Custodia>(entity =>
        {
            entity.ToTable("Custodias");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Ticker).HasMaxLength(12).IsRequired();
            entity.Property(e => e.PrecoMedio).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => new { e.ContaGraficaId, e.Ticker }).IsUnique();

            entity.HasOne(e => e.ContaGrafica)
                  .WithMany(c => c.Custodias)
                  .HasForeignKey(e => e.ContaGraficaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // HistoricoAporte
        modelBuilder.Entity<HistoricoAporte>(entity =>
        {
            entity.ToTable("HistoricoAportes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ValorAporte).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Cliente)
                  .WithMany(c => c.HistoricoAportes)
                  .HasForeignKey(e => e.ClienteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // DistribuicaoAporte
        modelBuilder.Entity<DistribuicaoAporte>(entity =>
        {
            entity.ToTable("DistribuicaoAportes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Ticker).HasMaxLength(12).IsRequired();
            entity.Property(e => e.PrecoUnitario).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ValorTotal).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.HistoricoAporte)
                  .WithMany(h => h.Distribuicoes)
                  .HasForeignKey(e => e.HistoricoAporteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Cesta
        modelBuilder.Entity<Cesta>(entity =>
        {
            entity.ToTable("Cestas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Ativa).HasDefaultValue(true);
        });

        // ItemCesta
        modelBuilder.Entity<ItemCesta>(entity =>
        {
            entity.ToTable("ItensCesta");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Ticker).HasMaxLength(12).IsRequired();
            entity.Property(e => e.Percentual).HasColumnType("decimal(5,2)");

            entity.HasOne(e => e.Cesta)
                  .WithMany(c => c.Itens)
                  .HasForeignKey(e => e.CestaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // OrdemCompra
        modelBuilder.Entity<OrdemCompra>(entity =>
        {
            entity.ToTable("OrdensCompra");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Ticker).HasMaxLength(12).IsRequired();
            entity.Property(e => e.TipoOrdem).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.PrecoUnitario).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ValorTotal).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.ContaGrafica)
                  .WithMany()
                  .HasForeignKey(e => e.ContaGraficaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
