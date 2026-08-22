using Microsoft.EntityFrameworkCore;
using GeoPharma.Models;

namespace GeoPharma.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Estabelecimento> Estabelecimentos { get; set; } = default!;
        public DbSet<Lead> Leads { get; set; } = default!;
        public DbSet<Regiao> Regioes { get; set; } = default!;
        public DbSet<Usuario> Usuarios { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeamento explícito para as tabelas do banco de dados
            modelBuilder.Entity<Estabelecimento>().ToTable("Estabelecimentos");
            modelBuilder.Entity<Lead>().ToTable("Leads");
            modelBuilder.Entity<Regiao>().ToTable("Regioes");
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
        }
    }
}