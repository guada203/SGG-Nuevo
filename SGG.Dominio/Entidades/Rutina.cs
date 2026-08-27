using System;
using System.Collections.Generic;
using System.Text;

namespace SGG.Dominio.Entidades
{
    public class Rutina
    {
        public int Id { get; set; }
        public int SocioId { get; set; }
        public Socio? Socio { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaAsignacion { get; set; }
    }
}
