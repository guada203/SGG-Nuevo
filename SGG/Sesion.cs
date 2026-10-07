namespace SGG
{
    public static class Sesion
    {
        public static int? UsuarioId { get; set; }
        public static string Nombre { get; set; } = string.Empty;
        public static string Apellido { get; set; } = string.Empty;
        public static string Email { get; set; } = string.Empty;
        public static string Rol { get; set; } = string.Empty;

        public static string NombreVisible =>
            string.IsNullOrWhiteSpace(Apellido) ? Nombre : $"{Nombre} {Apellido}";

        public static bool Activa => UsuarioId.HasValue;

        public static void Iniciar(Dominio.Entidades.Usuario usuario)
        {
            UsuarioId = usuario.Id;
            Nombre = usuario.Nombre;
            Apellido = usuario.Apellido ?? string.Empty;
            Email = usuario.Email;
            Rol = usuario.Rol?.Nombre ?? string.Empty;
        }

        public static void Limpiar()
        {
            UsuarioId = null;
            Nombre = string.Empty;
            Apellido = string.Empty;
            Email = string.Empty;
            Rol = string.Empty;
        }
    }
}