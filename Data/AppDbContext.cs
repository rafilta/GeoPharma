using GeoPharma.Models;
using Microsoft.EntityFrameworkCore;

namespace GeoPharma.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; } = default!;

        public DbSet<Lead> Leads { get; set; } = default!;

        public DbSet<PossivelLead> PossiveisLeads { get; set; } = default!;

        public DbSet<Regiao> Regioes { get; set; } = default!;

        public DbSet<Usuario> Usuarios { get; set; } = default!;

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>()
                .ToTable("Clientes");

            modelBuilder.Entity<Lead>()
                .ToTable("Leads");

            modelBuilder.Entity<PossivelLead>()
                .ToTable("PossiveisLeads");

            modelBuilder.Entity<Regiao>()
                .ToTable("Regioes");

            modelBuilder.Entity<Usuario>()
                .ToTable("Usuarios");

            /*
             * Um CNPJ só pode existir uma vez
             * entre os possíveis leads.
             */
            modelBuilder.Entity<PossivelLead>()
                .HasIndex(p => p.Cnpj)
                .IsUnique();
        }
    }
}