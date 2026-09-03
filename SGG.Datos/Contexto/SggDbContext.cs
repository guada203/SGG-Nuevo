using Microsoft.EntityFrameworkCore;
using SGG.Dominio.Entidades;

namespace SGG.Datos.Contexto
{
    public class SggDbContext : DbContext
    {
        public DbSet<Socio> Socios { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Membresia> Membresias { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Asistencia> Asistencias { get; set; }
        public DbSet<Rutina> Rutinas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
    @"Server=DESKTOP-2GR5V5M\SQLEXPRESS;Database=SGG;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=30;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Rol>()
                .HasMany(r => r.Usuarios)
                .WithOne(u => u.Rol)
                .HasForeignKey(u => u.RolId);

            modelBuilder.Entity<Rol>()
                .HasIndex(r => r.Nombre)
                .IsUnique();
        }
    }
}