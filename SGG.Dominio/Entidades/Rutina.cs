using System;

namespace SGG.Dominio.Entidades
{
    public class Rutina
    {
        public int Id { get; set; }
        public int SocioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Objetivo { get; set; } = string.Empty;
        public int DuracionSemanas { get; set; }
        public int FrecuenciaSemanal { get; set; }
        public string Nivel { get; set; } = string.Empty;       // "Principiante", "Intermedio", "Avanzado"
        public string Estado { get; set; } = string.Empty;      // "Activa", "Pendiente", "Completada"
        public string Notas { get; set; } = string.Empty;
        public DateTime FechaAsignacion { get; set; }
    }
}
