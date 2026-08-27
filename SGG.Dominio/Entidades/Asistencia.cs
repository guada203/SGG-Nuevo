using System;
using System.Collections.Generic;
using System.Text;

namespace SGG.Dominio.Entidades
{
    public class Asistencia
    {
        public int Id { get; set; }
        public int SocioId { get; set; }
        public Socio? Socio { get; set; }

        public DateTime FechaHoraIngreso { get; set; }
    }
}
