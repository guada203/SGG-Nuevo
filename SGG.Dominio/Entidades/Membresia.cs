using System;
using System.Collections.Generic;
using System.Text;

namespace SGG.Dominio.Entidades
{
    public class Membresia
    {
        public int Id { get; set; }
        public TipoActividad TipoActividad { get; set; }
        public decimal Precio { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public bool Vigente { get; set; } = true;
    }

    public enum TipoActividad
    {
        Musculacion,
        Funcional,
        Combinado
    }
}

