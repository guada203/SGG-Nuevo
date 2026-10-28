
using System;
using System.Collections.Generic;
using SGG.Datos.Repositorios;
using SGG.Dominio.Entidades;

namespace SGG.Logica.Servicios
{
    public class ServicioRutinas
    {
        private readonly RutinaRepositorio _rutinaRepositorio = new();
        private readonly SocioRepositorio _socioRepositorio = new();

        // Obtiene todas las rutinas registradas.
        public List<Rutina> ObtenerTodas()
        {
            return _rutinaRepositorio.ObtenerTodas();
        }

        // Obtiene una rutina por su identificador.
        public Rutina? ObtenerPorId(int id)
        {
            return _rutinaRepositorio.ObtenerPorId(id);
        }

        // Obtiene las rutinas de un socio.
        public List<Rutina> ObtenerPorSocio(int socioId)
        {
            return _rutinaRepositorio.ObtenerPorSocio(socioId);
        }

        // Valida y guarda una rutina nueva.
        public (bool Exitoso, string Mensaje) CrearRutina(Rutina rutina)
        {
            var validacion = ValidarRutina(rutina);

            if (!validacion.Exitoso)
                return validacion;

            rutina.FechaAsignacion = DateTime.Today;

            _rutinaRepositorio.Agregar(rutina);

            return (true, "Rutina creada con éxito.");
        }

        // Valida y actualiza una rutina existente.
        public (bool Exitoso, string Mensaje) ModificarRutina(Rutina rutina)
        {
            var rutinaExistente = _rutinaRepositorio.ObtenerPorId(rutina.Id);

            if (rutinaExistente == null)
                return (false, "La rutina que intentás modificar no existe.");

            var validacion = ValidarRutina(rutina);

            if (!validacion.Exitoso)
                return validacion;

            _rutinaRepositorio.Actualizar(rutina);

            return (true, "Rutina modificada con éxito.");
        }

        // Validaciones compartidas para crear y modificar.
        private (bool Exitoso, string Mensaje) ValidarRutina(Rutina rutina)
        {
            if (string.IsNullOrWhiteSpace(rutina.Nombre))
                return (false, "El nombre de la rutina es obligatorio.");

            if (rutina.SocioId <= 0)
                return (false, "Debés seleccionar un socio.");

            var socio = _socioRepositorio.ObtenerPorId(rutina.SocioId);

            if (socio == null || !socio.Activo)
                return (false, "El socio seleccionado no existe o está inactivo.");

            if (rutina.DuracionSemanas <= 0)
                return (false, "La duración debe ser mayor a cero.");

            if (rutina.FrecuenciaSemanal <= 0)
                return (false, "La frecuencia semanal debe ser mayor a cero.");

            if (string.IsNullOrWhiteSpace(rutina.Objetivo))
                return (false, "El objetivo de la rutina es obligatorio.");

            if (rutina.Ejercicios == null || rutina.Ejercicios.Count == 0)
                return (false, "La rutina debe contener al menos un ejercicio.");

            foreach (var ejercicio in rutina.Ejercicios)
            {
                if (string.IsNullOrWhiteSpace(ejercicio.Nombre))
                    return (false, "Todos los ejercicios deben tener nombre.");

                if (ejercicio.Series <= 0 || ejercicio.Repeticiones <= 0)
                    return (false, "Las series y repeticiones deben ser mayores a cero.");

                if (ejercicio.DescansoSegundos < 0)
                    return (false, "El descanso no puede ser negativo.");
            }

            return (true, "Datos válidos.");
        }
    }
}
