using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SGG.Datos.Contexto;
using SGG.Dominio.Entidades;

namespace SGG.Datos.Repositorios
{
    public class MembresiaRepositorio
    {
        public List<Membresia> ObtenerTodas()
        {
            using var contexto = new SggDbContext();
            return contexto.Membresias
                .OrderBy(m => m.TipoActividad)
                .ToList();
        }

        public List<Membresia> ObtenerVigentes()
        {
            using var contexto = new SggDbContext();
            return contexto.Membresias
                .Where(m => m.Vigente)
                .OrderBy(m => m.TipoActividad)
                .ToList();
        }

        public Membresia? ObtenerPorId(int id)
        {
            using var contexto = new SggDbContext();
            return contexto.Membresias.Find(id);
        }

        // Membresías de socios ACTIVOS cuyo vencimiento cae entre hoy y hoy + dias.
        // Se consulta desde Socios (que tiene la navegación a Membresia) para poder filtrar por estado del socio.
        public int ContarPorVencer(int dias)
        {
            using var contexto = new SggDbContext();
            var hoy = DateTime.Today;
            var limite = hoy.AddDays(dias);

            return contexto.Socios
                .Where(s => s.Activo
                    && s.Membresia.FechaVencimiento >= hoy
                    && s.Membresia.FechaVencimiento <= limite)
                .Count();
        }

        public void Agregar(Membresia membresia)
        {
            using var contexto = new SggDbContext();
            contexto.Membresias.Add(membresia);
            contexto.SaveChanges();
        }

        public void Actualizar(Membresia membresia)
        {
            using var contexto = new SggDbContext();
            contexto.Membresias.Update(membresia);
            contexto.SaveChanges();
        }

        public void DarDeBaja(int id)
        {
            using var contexto = new SggDbContext();
            var membresia = contexto.Membresias.Find(id);
            if (membresia != null)
            {
                membresia.Vigente = false;
                contexto.SaveChanges();
            }
        }
    }
}
