using GeoPharma.Enums;
using GeoPharma.Models;
using Microsoft.EntityFrameworkCore;

namespace GeoPharma.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Regiao> Regioes => Set<Regiao>();
    public DbSet<Estabelecimento> Estabelecimentos => Set<Estabelecimento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Usuário Administrador Principal
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Nome = "Administrador",
                Email = "admin@admin.com",
                SenhaHash = "Senha123!",
                Tipo = TipoUsuario.Admin,
                CriadoEm = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}