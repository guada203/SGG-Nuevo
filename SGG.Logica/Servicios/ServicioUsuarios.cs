using System.Collections.Generic;
using SGG.Datos.Repositorios;
using SGG.Dominio.Entidades;

namespace SGG.Logica.Servicios
{
    public class ServicioUsuarios
    {
        private readonly UsuarioRepositorio _usuarioRepositorio = new();

        public List<Usuario> ObtenerTodos()
        {
            return _usuarioRepositorio.ObtenerTodos();
        }

        public (bool Exitoso, string Mensaje) AltaUsuario(
            string nombre, string apellido, string? direccion, string? telefono,
            string? dni, string email, string password, int rolId)
        {
            if (_usuarioRepositorio.ExisteEmail(email))
                return (false, "Ya existe un usuario con ese email.");

            if (_usuarioRepositorio.ExisteDni(dni))
                return (false, "Ya existe un usuario con ese DNI.");

            var nuevoUsuario = new Usuario
            {
                Nombre = nombre,
                Apellido = apellido,
                Direccion = direccion,
                Telefono = telefono,
                Dni = dni,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                RolId = rolId,
                Activo = true
            };

            _usuarioRepositorio.Agregar(nuevoUsuario);
            return (true, "Usuario creado con éxito.");
        }

        public void DarDeBaja(int id)
        {
            _usuarioRepositorio.DarDeBaja(id);
        }

        public void Reactivar(int id)
        {
            _usuarioRepositorio.Reactivar(id);
        }
    }
}