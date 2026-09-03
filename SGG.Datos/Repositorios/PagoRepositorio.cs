using System;
using System.Collections.Generic;
using System.Linq;
using SGG.Datos.Contexto;
using SGG.Dominio.Entidades;

namespace SGG.Datos.Repositorios
{
    public class PagoRepositorio
    {
        public List<Pago> ObtenerTodos()
        {
            using var contexto = new SggDbContext();
            return contexto.Pagos.ToList();
        }

        public decimal SumarPagosDelMesActual()
        {
            using var contexto = new SggDbContext();
            var ahora = DateTime.Now;

            return contexto.Pagos
                .Where(p => p.FechaPago.Month == ahora.Month && p.FechaPago.Year == ahora.Year)
                .Sum(p => (decimal?)p.Monto) ?? 0;
        }
    }
}