using System;
using System.Collections.Generic;
using SGG.Datos.Repositorios;
using SGG.Dominio.Entidades;

namespace SGG.Logica.Servicios
{
    public class ServicioMembresias
    {
        private readonly MembresiaRepositorio _membresiaRepositorio = new();

        public List<Membresia> ObtenerTodas()
        {
            return _membresiaRepositorio.ObtenerTodas();
        }

        public List<Membresia> ObtenerVigentes()
        {
            return _membresiaRepositorio.ObtenerVigentes();
        }

        public Membresia? ObtenerPorId(int id)
        {
            return _membresiaRepositorio.ObtenerPorId(id);
        }

        public (bool Exitoso, string Mensaje) AltaMembresia(
            TipoActividad tipoActividad, decimal precio, DateTime fechaInicio, DateTime fechaVencimiento)
        {
            if (precio <= 0)
                return (false, "El precio debe ser mayor a cero.");

            if (fechaVencimiento <= fechaInicio)
                return (false, "La fecha de vencimiento debe ser posterior a la de inicio.");

            var nuevaMembresia = new Membresia
            {
                TipoActividad = tipoActividad,
                Precio = precio,
                FechaInicio = fechaInicio,
                FechaVencimiento = fechaVencimiento,
                Vigente = true
            };

            _membresiaRepositorio.Agregar(nuevaMembresia);
            return (true, "Membresía creada con éxito.");
        }

        public (bool Exitoso, string Mensaje) EditarMembresia(
            int id, TipoActividad tipoActividad, decimal precio, DateTime fechaInicio, DateTime fechaVencimiento)
        {
            var membresia = _membresiaRepositorio.ObtenerPorId(id);
            if (membresia == null)
                return (false, "La membresía no existe.");

            if (precio <= 0)
                return (false, "El precio debe ser mayor a cero.");

            if (fechaVencimiento <= fechaInicio)
                return (false, "La fecha de vencimiento debe ser posterior a la de inicio.");

            membresia.TipoActividad = tipoActividad;
            membresia.Precio = precio;
            membresia.FechaInicio = fechaInicio;
            membresia.FechaVencimiento = fechaVencimiento;

            _membresiaRepositorio.Actualizar(membresia);
            return (true, "Membresía actualizada con éxito.");
        }

        public void DarDeBaja(int id)
        {
            _membresiaRepositorio.DarDeBaja(id);
        }
    }
}
