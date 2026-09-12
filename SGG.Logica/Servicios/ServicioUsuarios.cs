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

        public Usuario? ObtenerPorId(int id)
        {
            return _usuarioRepositorio.ObtenerPorId(id);
        }

        public (bool Exitoso, string Mensaje) EditarUsuario(
            int id, string nombre, string apellido, string? direccion, string? telefono,
            string? dni, string email, int rolId, string? passwordNueva)
        {
            if (_usuarioRepositorio.ExisteEmailExcepto(email, id))
                return (false, "Ya existe otro usuario con ese email.");

            if (_usuarioRepositorio.ExisteDniExcepto(dni, id))
                return (false, "Ya existe otro usuario con ese DNI.");

            var usuario = _usuarioRepositorio.ObtenerPorId(id);
            if (usuario == null)
                return (false, "No se encontró el usuario.");

            usuario.Nombre = nombre;
            usuario.Apellido = apellido;
            usuario.Direccion = direccion;
            usuario.Telefono = telefono;
            usuario.Dni = dni;
            usuario.Email = email;
            usuario.RolId = rolId;

            if (!string.IsNullOrWhiteSpace(passwordNueva))
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordNueva);

            _usuarioRepositorio.Actualizar(usuario);
            return (true, "Usuario actualizado con éxito.");
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