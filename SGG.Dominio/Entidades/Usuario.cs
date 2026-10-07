using System;
using System.Collections.Generic;
using System.Text;

namespace SGG.Dominio.Entidades
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Apellido { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Dni { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public int RolId { get; set; }
        public Rol? Rol { get; set; }

        public bool Activo { get; set; } = true;
    }
}
