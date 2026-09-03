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
            string telefono, string email, int membresiaId)
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
                Activo = true
            };

            _socioRepositorio.Agregar(nuevoSocio);
            return (true, "Socio registrado con éxito.");
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
