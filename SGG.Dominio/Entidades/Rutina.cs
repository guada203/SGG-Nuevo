
using System;
using System.Collections.Generic;

namespace SGG.Dominio.Entidades
{
    public class Rutina
    {
        public int Id { get; set; }

        public int SocioId { get; set; }
        public Socio? Socio { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Objetivo { get; set; } = string.Empty;
        public int DuracionSemanas { get; set; }
        public int FrecuenciaSemanal { get; set; }
        public string Nivel { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public DateTime FechaAsignacion { get; set; }

        // Ejercicios que pertenecen a esta rutina
        public List<Ejercicio> Ejercicios { get; set; } = new();
    }
}
