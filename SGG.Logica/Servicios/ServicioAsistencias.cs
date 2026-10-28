using System;
using System.Collections.Generic;
using SGG.Datos.Repositorios;
using SGG.Dominio.Entidades;

namespace SGG.Logica.Servicios
{
    public class ServicioAsistencias
    {
        private readonly AsistenciaRepositorio _asistenciaRepositorio = new();
        private readonly SocioRepositorio _socioRepositorio = new();

        public List<Asistencia> ObtenerDeHoy() => _asistenciaRepositorio.ObtenerDeHoy();

        public List<Asistencia> ObtenerPorSocio(int socioId) => _asistenciaRepositorio.ObtenerPorSocio(socioId);

        public List<Asistencia> ObtenerTodos() => _asistenciaRepositorio.ObtenerTodos();

        /// <summary>
        /// RF-08/RF-09: registra el ingreso del socio validando que exista,
        /// esté activo y tenga la cuota al día.
        /// </summary>
        public (bool Exitoso, string Mensaje, Asistencia? Asistencia) RegistrarIngreso(int socioId)
        {
            if (socioId <= 0)
                return (false, "Debe seleccionar un socio.", null);

            var socio = _socioRepositorio.ObtenerPorId(socioId);
            if (socio == null)
                return (false, "El socio seleccionado no existe.", null);

            if (!socio.Activo)
                return (false, "El socio seleccionado se encuentra dado de baja.", null);

            // RF-09: cuota vencida bloquea el ingreso. Si el socio no tiene fecha
            // de vencimiento se permite el ingreso (aún no abonó ninguna cuota).
            if (socio.FechaVencimiento.HasValue && socio.FechaVencimiento.Value.Date < DateTime.Today)
                return (false, $"El socio tiene la cuota vencida (vence: {socio.FechaVencimiento.Value:dd/MM/yyyy}). No puede ingresar.", null);

            var asistencia = new Asistencia
            {
                SocioId = socioId,
                FechaHoraIngreso = DateTime.Now
            };

            var asistenciaGuardada = _asistenciaRepositorio.Agregar(asistencia);

            return (true, "Ingreso registrado correctamente.", asistenciaGuardada);
        }

        /// <summary>
        /// Estado de cuota del socio: "Vencida" si el vencimiento ya pasó;
        /// "Al día" en caso contrario (o si no tiene fecha de vencimiento).
        /// </summary>
        public static string EstadoCuota(Socio socio) =>
            socio.FechaVencimiento.HasValue && socio.FechaVencimiento.Value.Date < DateTime.Today
                ? "Vencida"
                : "Al día";
    }
}
