using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SGG.Datos.Contexto;
using SGG.Dominio.Entidades;

namespace SGG.Logica.Servicios
{
    public class ServicioReportes
    {
        public List<Pago> ObtenerPagosPorMes(int anio, int mes)
        {
            using var contexto = new SggDbContext();
            return contexto.Pagos
                .Include(p => p.Socio)
                .Where(p => p.FechaPago.Year == anio && p.FechaPago.Month == mes)
                .ToList();
        }

        public List<Asistencia> ObtenerAsistenciasPorRango(DateTime inicio, DateTime fin)
        {
            using var contexto = new SggDbContext();
            return contexto.Asistencias
                .Include(a => a.Socio)
                .Where(a => a.FechaHoraIngreso >= inicio && a.FechaHoraIngreso <= fin)
                .ToList();
        }

        public List<Socio> ObtenerTodosLosSocios()
        {
            using var contexto = new SggDbContext();
            return contexto.Socios
                .Include(s => s.Membresia)
                .ToList();
        }

        public (int activos, int inactivos) ObtenerResumenSocios()
        {
            var socios = ObtenerTodosLosSocios();
            int activos = socios.Count(s => s.Activo);
            int inactivos = socios.Count(s => !s.Activo);
            return (activos, inactivos);
        }
    }
}
