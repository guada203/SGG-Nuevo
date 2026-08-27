using SGG.Datos.Repositorios;
using SGG.Dominio.Entidades;

namespace SGG.Logica.Servicios
{
    public class ServicioAutenticacion
    {
        private readonly UsuarioRepositorio _usuarioRepositorio = new();

        public ResultadoLogin ValidarCredenciales(string email, string password, string rolEsperado)
        {
            var usuario = _usuarioRepositorio.ObtenerPorEmail(email);

            if (usuario == null)
                return new ResultadoLogin(false, "Usuario o contraseña incorrectos.");

            bool passwordValida = BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);
            if (!passwordValida)
                return new ResultadoLogin(false, "Usuario o contraseña incorrectos.");

            if (usuario.Rol != rolEsperado)
                return new ResultadoLogin(false, $"Este usuario no tiene permisos de {rolEsperado}.");

            return new ResultadoLogin(true, "Login exitoso.", usuario);
        }
    }

    public class ResultadoLogin
    {
        public bool Exitoso { get; }
        public string Mensaje { get; }
        public Usuario? Usuario { get; }

        public ResultadoLogin(bool exitoso, string mensaje, Usuario? usuario = null)
        {
            Exitoso = exitoso;
            Mensaje = mensaje;
            Usuario = usuario;
        }
    }
}