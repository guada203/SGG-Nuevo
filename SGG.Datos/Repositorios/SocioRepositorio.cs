using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SGG.Datos.Contexto;
using SGG.Dominio.Entidades;

namespace SGG.Datos.Repositorios
{
    public class SocioRepositorio
    {
        public List<Socio> ObtenerTodos()
        {
            using var contexto = new SggDbContext();
            return contexto.Socios
                .Include(s => s.Membresia)
                .ToList();
        }

        public Socio? ObtenerPorId(int id)
        {
            using var contexto = new SggDbContext();
            return contexto.Socios
                .Include(s => s.Membresia)
                .FirstOrDefault(s => s.Id == id);
        }

        public int ContarActivos()
        {
            using var contexto = new SggDbContext();
            return contexto.Socios.Count(s => s.Activo);
        }

        public bool ExisteDni(string dni)
        {
            using var contexto = new SggDbContext();
            return contexto.Socios.Any(s => s.Dni == dni);
        }

        public void Agregar(Socio socio)
        {
            using var contexto = new SggDbContext();
            contexto.Socios.Add(socio);
            contexto.SaveChanges();
        }

        public void DarDeBaja(int id)
        {
            using var contexto = new SggDbContext();
            var socio = contexto.Socios.Find(id);
            if (socio != null)
            {
                socio.Activo = false;
                contexto.SaveChanges();
            }
        }

        public void Reactivar(int id)
        {
            using var contexto = new SggDbContext();
            var socio = contexto.Socios.Find(id);
            if (socio != null)
            {
                socio.Activo = true;
                contexto.SaveChanges();
            }
        }
    }
}