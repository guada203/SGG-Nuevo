using System;
using System.Collections.Generic;
using SGG.Datos.Repositorios;
using SGG.Dominio.Entidades;

namespace SGG.Logica.Servicios
{
    public class ServicioSocios
    {
        private readonly SocioRepositorio _socioRepositorio = new();
        private readonly MembresiaRepositorio _membresiaRepositorio = new();

        public List<Socio> ObtenerTodos()
        {
            return _socioRepositorio.ObtenerTodos();
        }

        public (bool Exitoso, string Mensaje) AltaSocio(
            string nombre, string apellido, string dni, DateTime fechaNacimiento,
            string telefono, string email, int membresiaId, int? entrenadorId = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(apellido))
                return (false, "El apellido es obligatorio.");

            if (string.IsNullOrWhiteSpace(dni))
                return (false, "El DNI es obligatorio.");

            if (_socioRepositorio.ExisteDni(dni))
                return (false, "Ya existe un socio con ese DNI.");

            var membresia = _membresiaRepositorio.ObtenerPorId(membresiaId);
            if (membresia == null || !membresia.Vigente)
                return (false, "La membresía seleccionada no existe o no está vigente.");

            var nuevoSocio = new Socio
            {
                Nombre = nombre,
                Apellido = apellido,
                Dni = dni,
                FechaNacimiento = fechaNacimiento,
                Telefono = telefono,
                Email = email,
                MembresiaId = membresiaId,
                EntrenadorId = entrenadorId,
                Activo = true,
                // Alta hoy, pero todavía sin período pago: el primer pago fija el vencimiento.
                FechaAlta = DateTime.Today,
                FechaVencimiento = null
            };

            _socioRepositorio.Agregar(nuevoSocio);
            return (true, "Socio registrado con éxito.");
        }

        /// <summary>
        /// RF-02: actualiza los datos de un socio existente. El DNI es inalterable y la
        /// cuota del socio (FechaAlta / FechaVencimiento) no se modifica al editar los datos.
        /// La asignación de entrenador es el único campo extra que se escribe.
        /// </summary>
        public (bool Exitoso, string Mensaje) EditarSocio(
            int id, string nombre, string apellido, DateTime fechaNacimiento,
            string telefono, string email, int membresiaId, int? entrenadorId = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(apellido))
                return (false, "El apellido es obligatorio.");

            if (fechaNacimiento > DateTime.Today)
                return (false, "La fecha de nacimiento no puede ser futura.");

            var socio = _socioRepositorio.ObtenerPorId(id);
            if (socio == null)
                return (false, "El socio no existe.");

            var membresia = _membresiaRepositorio.ObtenerPorId(membresiaId);
            if (membresia == null || !membresia.Vigente)
                return (false, "La membresía seleccionada no existe o no está vigente.");

            socio.Nombre = nombre;
            socio.Apellido = apellido;
            socio.FechaNacimiento = fechaNacimiento;
            socio.Telefono = telefono;
            socio.Email = email;
            socio.MembresiaId = membresiaId;
            socio.EntrenadorId = entrenadorId;

            _socioRepositorio.Actualizar(socio);
            return (true, "Socio actualizado con éxito.");
        }

        public (bool Exitoso, string Mensaje) DarDeBaja(int id)
        {
            var socio = _socioRepositorio.ObtenerPorId(id);
            if (socio == null)
                return (false, "El socio no existe.");

            if (!socio.Activo)
                return (false, "El socio ya se encuentra dado de baja.");

            _socioRepositorio.DarDeBaja(id);
            return (true, "Socio dado de baja con éxito.");
        }

        public (bool Exitoso, string Mensaje) Reactivar(int id)
        {
            var socio = _socioRepositorio.ObtenerPorId(id);
            if (socio == null)
                return (false, "El socio no existe.");

            if (socio.Activo)
                return (false, "El socio ya se encuentra activo.");

            _socioRepositorio.Reactivar(id);
            return (true, "Socio reactivado con éxito.");
        }
    }
}
