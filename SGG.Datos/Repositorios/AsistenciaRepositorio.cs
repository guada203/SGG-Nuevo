using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SGG.Datos.Contexto;
using SGG.Dominio.Entidades;

namespace SGG.Datos.Repositorios
{
    public class AsistenciaRepositorio
    {
        // Se incluye el Socio para que las pantallas muestren nombre y DNI
        // sin tener que volver a consultar la tabla Socios fila por fila.
        public List<Asistencia> ObtenerTodos()
        {
            using var contexto = new SggDbContext();
            return contexto.Asistencias
                .Include(a => a.Socio)
                .OrderByDescending(a => a.FechaHoraIngreso)
                .ToList();
        }

        // Se filtra por rango [hoy, mañana) y no por FechaHoraIngreso.Date == hoy:
        // .Date no se puede traducir de forma confiable a SQL y provocaría
        // una consulta en memoria o un error al ejecutar.
        public List<Asistencia> ObtenerDeHoy()
        {
            using var contexto = new SggDbContext();
            var hoy = DateTime.Today;
            var manana = hoy.AddDays(1);

            return contexto.Asistencias
                .Include(a => a.Socio)
                .Where(a => a.FechaHoraIngreso >= hoy && a.FechaHoraIngreso < manana)
                .OrderBy(a => a.FechaHoraIngreso)
                .ToList();
        }

        public List<Asistencia> ObtenerPorSocio(int socioId)
        {
            using var contexto = new SggDbContext();
            return contexto.Asistencias
                .Include(a => a.Socio)
                .Where(a => a.SocioId == socioId)
                .OrderByDescending(a => a.FechaHoraIngreso)
                .ToList();
        }

        // Devuelve la misma Asistencia recibida, ya con el Id generado por la base
        // (Asistencias.Id es IDENTITY) por si el consumidor necesita referenciarla.
        public Asistencia Agregar(Asistencia asistencia)
        {
            using var contexto = new SggDbContext();
            contexto.Asistencias.Add(asistencia);
            contexto.SaveChanges();
            return asistencia;
        }
    }
}
