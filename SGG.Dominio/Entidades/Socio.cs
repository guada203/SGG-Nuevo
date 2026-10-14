using System;
using System.Collections.Generic;
using System.Text;

namespace SGG.Dominio.Entidades
{
    public class Socio
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;

        // Fechas de la cuota del socio. La membresía es el catálogo de planes;
        // el vencimiento es de cada socio (varios socios pueden compartir un plan).
        public DateTime? FechaAlta { get; set; }
        public DateTime? FechaVencimiento { get; set; }

        // Relación con Membresía (RF-06)
        public int MembresiaId { get; set; }
        public Membresia? Membresia { get; set; }

        // Relaciones inversas (un socio tiene varios pagos, asistencias, rutinas)
        public List<Pago> Pagos { get; set; } = new();
        public List<Asistencia> Asistencias { get; set; } = new();
        public List<Rutina> Rutinas { get; set; } = new();
    }
}
