using System.Collections.Generic;
using System.Linq;
using SGG.Datos.Contexto;
using SGG.Dominio.Entidades;

namespace SGG.Datos.Repositorios
{
    public class RolRepositorio
    {
        public List<Rol> ObtenerTodos()
        {
            using var contexto = new SggDbContext();
            return contexto.Roles.OrderBy(r => r.Nombre).ToList();
        }

        public Rol? ObtenerPorNombre(string nombre)
        {
            using var contexto = new SggDbContext();
            return contexto.Roles.FirstOrDefault(r => r.Nombre == nombre);
        }
    }
}
