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

        // Cantidad de socios por mes de ingreso (FechaAlta) para un año, zero-filled (12 meses).
        // Solo incluye socios con FechaAlta cargada.
        public List<(int Mes, int Cantidad)> ContarPorMesIngreso(int anio)
        {
            using var contexto = new SggDbContext();
            var datos = contexto.Socios
                .Where(s => s.FechaAlta.HasValue && s.FechaAlta.Value.Year == anio)
                .GroupBy(s => s.FechaAlta!.Value.Month)
                .Select(g => new { Mes = g.Key, Cantidad = g.Count() })
                .ToList();

            var resultado = new List<(int Mes, int Cantidad)>();
            for (int mes = 1; mes <= 12; mes++)
            {
                var item = datos.FirstOrDefault(d => d.Mes == mes);
                resultado.Add((mes, item?.Cantidad ?? 0));
            }
            return resultado;
        }

        // Rango de años de ingreso disponibles (min, max) según FechaAlta. Null si no hay datos.
        public (int Min, int Max)? ObtenerRangoAniosIngreso()
        {
            using var contexto = new SggDbContext();
            var fechas = contexto.Socios.Where(s => s.FechaAlta.HasValue).Select(s => s.FechaAlta!.Value.Year);
            if (!fechas.Any())
                return null;
            return (fechas.Min(), fechas.Max());
        }

        // Socios ACTIVOS cuya cuota ya venció (vencimiento estrictamente anterior a hoy).
        // El orden es ascendente por fecha: primero los más atrasados.
        public List<Socio> ObtenerVencidos()
        {
            using var contexto = new SggDbContext();
            var hoy = DateTime.Today;
            return contexto.Socios
                .Include(s => s.Membresia)
                .Where(s => s.Activo
                    && s.FechaVencimiento.HasValue
                    && s.FechaVencimiento.Value < hoy)
                .OrderBy(s => s.FechaVencimiento)
                .ToList();
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