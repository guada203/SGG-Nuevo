namespace SGG.Dominio.Entidades
{
    public class Ejercicio
    {
        public int Id { get; set; }
        public int RutinaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string GrupoMuscular { get; set; } = string.Empty;
        public int Series { get; set; }
        public int Repeticiones { get; set; }
        public int DescansoSegundos { get; set; }
    }
}