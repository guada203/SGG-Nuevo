using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        public DbSet<Ejercicio> Ejercicios { get; set; }

        private const string CadenaConexionPorDefecto =
            @"Server=.\SQLEXPRESS;Database=SGG;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=30;";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ObtenerCadenaConexion());
        }

        // Lee ConnectionStrings:SGG de appsettings.json (plantilla del repo) y, si existe,
        // de appsettings.local.json (archivo local de cada desarrolladora, fuera de git).
        // Si no hay archivos de configuración, cae a la instancia local por defecto de SQL Express.
        private static string ObtenerCadenaConexion()
        {
            var configuracion = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false)
                .Build();

            var cadena = configuracion.GetConnectionString("SGG");
            return string.IsNullOrWhiteSpace(cadena) ? CadenaConexionPorDefecto : cadena;
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

            // Membresias.TipoActividad se guarda como texto ("Musculacion"/"Funcional"/"Combinado")
            // en la base, no como int. Sin esta conversión EF intenta castear el string a int y falla.
            modelBuilder.Entity<Membresia>()
                .Property(m => m.TipoActividad)
                .HasConversion<string>();

            // Las fechas de la cuota del socio se guardan como DATE en la base.
            modelBuilder.Entity<Socio>()
                .Property(s => s.FechaAlta)
                .HasColumnType("date");

            modelBuilder.Entity<Socio>()
                .Property(s => s.FechaVencimiento)
                .HasColumnType("date");


            // Relación: una rutina contiene varios ejercicios.
            // Cada ejercicio pertenece a una rutina mediante RutinaId.
            modelBuilder.Entity<Rutina>()
                .HasMany(r => r.Ejercicios)
                .WithOne(e => e.Rutina)
                .HasForeignKey(e => e.RutinaId);

            // FechaAsignacion se almacena como DATE en SQL Server.
            modelBuilder.Entity<Rutina>()
                .Property(r => r.FechaAsignacion)
                .HasColumnType("date");

        }
    }
}