using System;
using System.Collections.Generic;
using SGG.Datos.Repositorios;
using SGG.Dominio.Entidades;

namespace SGG.Logica.Servicios
{
    public class ServicioPagos
    {
        private readonly PagoRepositorio _pagoRepositorio = new();
        private readonly SocioRepositorio _socioRepositorio = new();

        public List<Pago> ObtenerTodos()
        {
            return _pagoRepositorio.ObtenerTodos();
        }

        /// <summary>
        /// Valida y persiste un pago contra la base real. Devuelve el Pago ya guardado
        /// (con su Id) para que la pantalla pueda emitir el comprobante correspondiente
        /// y la nueva fecha de vencimiento de la cuota del socio (null si no se pudo calcular).
        /// </summary>
        public (bool Exitoso, string Mensaje, Pago? Pago, DateTime? NuevoVencimiento) RegistrarPago(
            int socioId, decimal monto, string metodoPago)
        {
            if (socioId <= 0)
                return (false, "Debe seleccionar un socio.", null, null);

            var socio = _socioRepositorio.ObtenerPorId(socioId);
            if (socio == null)
                return (false, "El socio seleccionado no existe.", null, null);

            if (!socio.Activo)
                return (false, "El socio seleccionado se encuentra dado de baja.", null, null);

            if (monto <= 0)
                return (false, "El monto del pago debe ser mayor a cero.", null, null);

            if (string.IsNullOrWhiteSpace(metodoPago))
                return (false, "Debe seleccionar un método de pago.", null, null);

            var pago = new Pago
            {
                SocioId = socioId,
                Monto = monto,
                FechaPago = DateTime.Now,
                MetodoPago = metodoPago.Trim()
            };

            var pagoGuardado = _pagoRepositorio.Agregar(pago);

            // Registrar el pago renueva la cuota del socio por exactamente un mes.
            // Si la cuota todavía no venció se apila el mes sobre el vencimiento
            // actual (no se pierden los días restantes); si ya venció, se cuenta desde hoy.
            // El vencimiento es del socio, no de la membresía (que es el catálogo de planes).
            var baseFecha = socio.FechaVencimiento.HasValue
                            && socio.FechaVencimiento.Value.Date >= DateTime.Today
                ? socio.FechaVencimiento.Value.Date
                : DateTime.Today;

            socio.FechaVencimiento = baseFecha.AddMonths(1);
            _socioRepositorio.Actualizar(socio);

            return (true, "Pago registrado correctamente.", pagoGuardado, socio.FechaVencimiento);
        }
    }
}
