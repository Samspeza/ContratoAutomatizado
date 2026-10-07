using Contratos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Contratos.Infrastructure.Persistence;

public sealed class ContratosDbContext : DbContext
{
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<ContratoResponsavel> ContratoResponsaveis => Set<ContratoResponsavel>();
    public DbSet<EventoHistorico> EventosHistorico => Set<EventoHistorico>();

    public ContratosDbContext(DbContextOptions<ContratosDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contrato>(entidade =>
        {
            entidade.Property(c => c.Cnpj).IsRequired().HasMaxLength(14);
            entidade.Property(c => c.RazaoSocial).IsRequired().HasMaxLength(200);
            entidade.Property(c => c.Endereco).IsRequired().HasMaxLength(300);
            entidade.Property(c => c.PercentualHonorarios).HasPrecision(5, 2);
            entidade.Property(c => c.Status).HasConversion<string>().HasMaxLength(30);
            entidade.Property(c => c.StatusEmail).HasConversion<string>().HasMaxLength(20);
            entidade.Property(c => c.StatusWhatsapp).HasConversion<string>().HasMaxLength(20);

            entidade.HasIndex(c => c.Status);
            entidade.HasIndex(c => c.Cnpj);

            entidade.HasMany(c => c.Responsaveis)
                .WithOne()
                .HasForeignKey(r => r.ContratoId)
                .OnDelete(DeleteBehavior.Cascade);

            entidade.HasMany(c => c.Historico)
                .WithOne()
                .HasForeignKey(h => h.ContratoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ContratoResponsavel>(entidade =>
        {
            entidade.Property(r => r.Nome).IsRequired().HasMaxLength(200);
            entidade.Property(r => r.Cpf).HasMaxLength(14);
        });

        modelBuilder.Entity<EventoHistorico>(entidade =>
        {
            entidade.Property(h => h.Tipo).HasConversion<string>().HasMaxLength(40);
            entidade.Property(h => h.Descricao).IsRequired().HasMaxLength(500);
            entidade.Property(h => h.Usuario).HasMaxLength(200);
        });
    }
}