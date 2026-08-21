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

        // Relación con Membresía (RF-06)
        public int MembresiaId { get; set; }
    }
}
