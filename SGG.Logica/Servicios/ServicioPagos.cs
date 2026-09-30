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
        /// (con su Id) para que la pantalla pueda emitir el comprobante correspondiente.
        /// </summary>
        public (bool Exitoso, string Mensaje, Pago? Pago) RegistrarPago(
            int socioId, decimal monto, string metodoPago)
        {
            if (socioId <= 0)
                return (false, "Debe seleccionar un socio.", null);

            var socio = _socioRepositorio.ObtenerPorId(socioId);
            if (socio == null)
                return (false, "El socio seleccionado no existe.", null);

            if (!socio.Activo)
                return (false, "El socio seleccionado se encuentra dado de baja.", null);

            if (monto <= 0)
                return (false, "El monto del pago debe ser mayor a cero.", null);

            if (string.IsNullOrWhiteSpace(metodoPago))
                return (false, "Debe seleccionar un método de pago.", null);

            var pago = new Pago
            {
                SocioId = socioId,
                Monto = monto,
                FechaPago = DateTime.Now,
                MetodoPago = metodoPago.Trim()
            };

            return (true, "Pago registrado correctamente.", _pagoRepositorio.Agregar(pago));
        }
    }
}
