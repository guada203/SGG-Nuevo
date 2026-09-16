namespace SGG.Dominio.Entidades
{
    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public override string ToString() => Nombre;

        public List<Usuario> Usuarios { get; set; } = new();
    }
}
