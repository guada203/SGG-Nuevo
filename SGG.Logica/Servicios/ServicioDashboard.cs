using SGG.Datos.Repositorios;

namespace SGG.Logica.Servicios
{
    public class ServicioDashboard
    {
        private readonly SocioRepositorio _socioRepositorio = new();
        private readonly PagoRepositorio _pagoRepositorio = new();
        private readonly UsuarioRepositorio _usuarioRepositorio = new();

        public int ObtenerCantidadSociosActivos()
        {
            return _socioRepositorio.ContarActivos();
        }

        public int ObtenerCantidadUsuarios()
        {
            return _usuarioRepositorio.ObtenerTodos().Count;
        }

        public decimal ObtenerIngresosDelMes()
        {
            return _pagoRepositorio.SumarPagosDelMesActual();
        }
    }
}