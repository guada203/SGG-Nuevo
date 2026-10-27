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
                .Include(s => s.Entrenador)
                .ToList();
        }

        public Socio? ObtenerPorId(int id)
        {
            using var contexto = new SggDbContext();
            return contexto.Socios
                .Include(s => s.Membresia)
                .Include(s => s.Entrenador)
                .FirstOrDefault(s => s.Id == id);
        }

        public int ContarActivos()
        {
            using var contexto = new SggDbContext();
            return contexto.Socios.Count(s => s.Activo);
        }

        public int ContarInactivos()
        {
            using var contexto = new SggDbContext();
            return contexto.Socios.Count(s => !s.Activo);
        }

        // "Nuevos socios del mes": usa la FechaAlta propia del socio.
        public int ContarNuevosDelMes()
        {
            using var contexto = new SggDbContext();
            var ahora = DateTime.Now;
            return contexto.Socios
                .Where(s => s.FechaAlta.HasValue
                         && s.FechaAlta.Value.Month == ahora.Month
                         && s.FechaAlta.Value.Year == ahora.Year)
                .Count();
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

        // Actualiza solo los campos propios del socio. Se busca la fila en el contexto
        // actual a propósito: con Update(socio) y la navegación Membresia cargada, EF
        // marcaría también la membresía como modificada, y esa fila puede estar
        // COMPARTIDA por varios socios.
        public void Actualizar(Socio socio)
        {
            using var contexto = new SggDbContext();
            var socioDb = contexto.Socios.Find(socio.Id);
            if (socioDb == null)
                return;

            socioDb.Nombre = socio.Nombre;
            socioDb.Apellido = socio.Apellido;
            socioDb.Dni = socio.Dni;
            socioDb.FechaNacimiento = socio.FechaNacimiento;
            socioDb.Telefono = socio.Telefono;
            socioDb.Email = socio.Email;
            socioDb.Activo = socio.Activo;
            socioDb.MembresiaId = socio.MembresiaId;
            socioDb.FechaAlta = socio.FechaAlta;
            socioDb.FechaVencimiento = socio.FechaVencimiento;
            socioDb.EntrenadorId = socio.EntrenadorId;

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