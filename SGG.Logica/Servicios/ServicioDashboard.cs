using System;
using System.Collections.Generic;
using SGG.Datos.Repositorios;

namespace SGG.Logica.Servicios
{
    public class ServicioDashboard
    {
        private readonly SocioRepositorio _socioRepositorio = new();
        private readonly PagoRepositorio _pagoRepositorio = new();
        private readonly UsuarioRepositorio _usuarioRepositorio = new();
        private readonly MembresiaRepositorio _membresiaRepositorio = new();

        public int ObtenerCantidadSociosActivos()
        {
            return _socioRepositorio.ContarActivos();
        }

        public int ObtenerCantidadSociosInactivos()
        {
            return _socioRepositorio.ContarInactivos();
        }

        public int ObtenerCantidadSociosNuevosDelMes()
        {
            return _socioRepositorio.ContarNuevosDelMes();
        }

        public int ObtenerCantidadMembresiasPorVencer()
        {
            return _membresiaRepositorio.ContarPorVencer(30);
        }

        public int ObtenerCantidadUsuarios()
        {
            return _usuarioRepositorio.ObtenerTodos().Count;
        }

        public decimal ObtenerIngresosDelMes()
        {
            return _pagoRepositorio.SumarPagosDelMesActual();
        }

        public List<(DateTime Mes, decimal Total)> ObtenerIngresosUltimosMeses(int meses)
        {
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var desde = inicioMes.AddMonths(-(meses - 1));
            var pagos = _pagoRepositorio.ObtenerPagosDesde(desde);

            var resultado = new List<(DateTime Mes, decimal Total)>();
            for (int i = 0; i < meses; i++)
            {
                var mes = desde.AddMonths(i);
                resultado.Add((mes, pagos.Where(p => p.FechaPago.Year == mes.Year && p.FechaPago.Month == mes.Month)
                                         .Sum(p => (decimal?)p.Monto) ?? 0));
            }
            return resultado;
        }
    }
}