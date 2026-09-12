using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SGG.Datos.Contexto;
using SGG.Dominio.Entidades;

namespace SGG.Datos.Repositorios
{
    public class UsuarioRepositorio
    {
        public Usuario? ObtenerPorId(int id)
        {
            using var contexto = new SggDbContext();
            return contexto.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefault(u => u.Id == id);
        }

        public void Actualizar(Usuario usuario)
        {
            using var contexto = new SggDbContext();
            contexto.Usuarios.Update(usuario);
            contexto.SaveChanges();
        }

        public Usuario? ObtenerPorEmail(string email)
        {
            using var contexto = new SggDbContext();
            return contexto.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefault(u => u.Email == email && u.Activo);
        }

        public List<Usuario> ObtenerTodos()
        {
            using var contexto = new SggDbContext();
            return contexto.Usuarios.Include(u => u.Rol).ToList();
        }

        public void Agregar(Usuario usuario)
        {
            using var contexto = new SggDbContext();
            contexto.Usuarios.Add(usuario);
            contexto.SaveChanges();
        }

        public void DarDeBaja(int id)
        {
            using var contexto = new SggDbContext();
            var usuario = contexto.Usuarios.Find(id);
            if (usuario != null)
            {
                usuario.Activo = false;
                contexto.SaveChanges();
            }
        }

        public void Reactivar(int id)
        {
            using var contexto = new SggDbContext();
            var usuario = contexto.Usuarios.Find(id);
            if (usuario != null)
            {
                usuario.Activo = true;
                contexto.SaveChanges();
            }
        }

        public bool ExisteEmail(string email)
        {
            using var contexto = new SggDbContext();
            return contexto.Usuarios.Any(u => u.Email == email);
        }

        public bool ExisteDni(string? dni)
        {
            using var contexto = new SggDbContext();
            return !string.IsNullOrWhiteSpace(dni)
                && contexto.Usuarios.Any(u => u.Dni == dni);
        }

        public bool ExisteEmailExcepto(string email, int excluirId)
        {
            using var contexto = new SggDbContext();
            return contexto.Usuarios.Any(u => u.Email == email && u.Id != excluirId);
        }

        public bool ExisteDniExcepto(string? dni, int excluirId)
        {
            using var contexto = new SggDbContext();
            return !string.IsNullOrWhiteSpace(dni)
                && contexto.Usuarios.Any(u => u.Dni == dni && u.Id != excluirId);
        }
    }
}