using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SGG.Datos.Contexto;
using SGG.Dominio.Entidades;

namespace SGG.Datos.Repositorios
{
    public class PagoRepositorio
    {
        // Se incluye el Socio para que los consumidores (historial, comprobantes) puedan
        // mostrar nombre y DNI sin tener que volver a consultar la tabla Socios.
        public List<Pago> ObtenerTodos()
        {
            using var contexto = new SggDbContext();
            return contexto.Pagos
                .Include(p => p.Socio)
                .ToList();
        }

        // Devuelve el mismo Pago recibido, ya con el Id generado por la base
        // (Pagos.Id es IDENTITY) para poder referenciarlo en el comprobante.
        public Pago Agregar(Pago pago)
        {
            using var contexto = new SggDbContext();
            contexto.Pagos.Add(pago);
            contexto.SaveChanges();
            return pago;
        }

        public decimal SumarPagosDelMesActual()
        {
            using var contexto = new SggDbContext();
            var ahora = DateTime.Now;

            return contexto.Pagos
                .Where(p => p.FechaPago.Month == ahora.Month && p.FechaPago.Year == ahora.Year)
                .Sum(p => (decimal?)p.Monto) ?? 0;
        }

        public List<Pago> ObtenerPagosDesde(DateTime desde)
        {
            using var contexto = new SggDbContext();
            return contexto.Pagos.Where(p => p.FechaPago >= desde).ToList();
        }
    }
}