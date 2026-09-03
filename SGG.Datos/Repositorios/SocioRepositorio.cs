using System.Collections.Generic;
using System.Linq;
using SGG.Datos.Contexto;
using SGG.Dominio.Entidades;

namespace SGG.Datos.Repositorios
{
    public class SocioRepositorio
    {
        public List<Socio> ObtenerTodos()
        {
            using var contexto = new SggDbContext();
            return contexto.Socios.ToList();
        }

        public int ContarActivos()
        {
            using var contexto = new SggDbContext();
            return contexto.Socios.Count(s => s.Activo);
        }
    }
}