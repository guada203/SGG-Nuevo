using System.Linq;
using SGG.Datos.Contexto;
using SGG.Dominio.Entidades;

namespace SGG.Datos.Repositorios
{
    public class UsuarioRepositorio
    {
        public Usuario? ObtenerPorEmail(string email)
        {
            using var contexto = new SggDbContext();
            return contexto.Usuarios
                .FirstOrDefault(u => u.Email == email && u.Activo);
        }
    }
}