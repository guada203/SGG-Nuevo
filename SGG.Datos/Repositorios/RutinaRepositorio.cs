
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SGG.Datos.Contexto;
using SGG.Dominio.Entidades;

namespace SGG.Datos.Repositorios
{
    public class RutinaRepositorio
    {
        // Obtiene todas las rutinas con su socio y ejercicios.
        public List<Rutina> ObtenerTodas()
        {
            using var contexto = new SggDbContext();

            return contexto.Rutinas
                .Include(r => r.Socio)
                .Include(r => r.Ejercicios)
                .ToList();
        }

        // Busca una rutina por su identificador.
        public Rutina? ObtenerPorId(int id)
        {
            using var contexto = new SggDbContext();

            return contexto.Rutinas
                .Include(r => r.Socio)
                .Include(r => r.Ejercicios)
                .FirstOrDefault(r => r.Id == id);
        }

        // Obtiene las rutinas de un socio determinado.
        public List<Rutina> ObtenerPorSocio(int socioId)
        {
            using var contexto = new SggDbContext();

            return contexto.Rutinas
                .Include(r => r.Ejercicios)
                .Where(r => r.SocioId == socioId)
                .ToList();
        }

        // Guarda una rutina nueva junto con sus ejercicios.
        public void Agregar(Rutina rutina)
        {
            using var contexto = new SggDbContext();

            contexto.Rutinas.Add(rutina);
            contexto.SaveChanges();
        }

        // Actualiza los datos de una rutina existente.
        public void Actualizar(Rutina rutina)
        {
            using var contexto = new SggDbContext();

            var rutinaDb = contexto.Rutinas
                .Include(r => r.Ejercicios)
                .FirstOrDefault(r => r.Id == rutina.Id);

            if (rutinaDb == null)
                return;

            rutinaDb.SocioId = rutina.SocioId;
            rutinaDb.Nombre = rutina.Nombre;
            rutinaDb.Objetivo = rutina.Objetivo;
            rutinaDb.DuracionSemanas = rutina.DuracionSemanas;
            rutinaDb.FrecuenciaSemanal = rutina.FrecuenciaSemanal;
            rutinaDb.Nivel = rutina.Nivel;
            rutinaDb.Estado = rutina.Estado;
            rutinaDb.Notas = rutina.Notas;

            // Reemplaza los ejercicios anteriores por los nuevos.
            contexto.Ejercicios.RemoveRange(rutinaDb.Ejercicios);

            foreach (var ejercicio in rutina.Ejercicios)
            {
                rutinaDb.Ejercicios.Add(new Ejercicio
                {
                    Nombre = ejercicio.Nombre,
                    GrupoMuscular = ejercicio.GrupoMuscular,
                    Series = ejercicio.Series,
                    Repeticiones = ejercicio.Repeticiones,
                    DescansoSegundos = ejercicio.DescansoSegundos
                });
            }

            contexto.SaveChanges();
        }
    }
}
