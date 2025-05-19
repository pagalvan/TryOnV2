using ENTITIES;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using TryOn.DAL;

namespace TryOn.BLL
{
    public class UsuarioService : IService<Usuario>
    {
        private readonly UsuarioRepository _repository;

        public UsuarioService()
        {
            _repository = new UsuarioRepository();
        }

        public void Add(Usuario usuario)
        {
            // Validar datos
            if (string.IsNullOrEmpty(usuario.Nombre))
                throw new Exception("El nombre es obligatorio");

            if (string.IsNullOrEmpty(usuario.Apellido))
                throw new Exception("El apellido es obligatorio");

            if (string.IsNullOrEmpty(usuario.Email))
                throw new Exception("El email es obligatorio");

            if (string.IsNullOrEmpty(usuario.Password))
                throw new Exception("La contraseña es obligatoria");

            // Verificar si el email ya existe
            var existingUser = _repository.GetByEmail(usuario.Email);
            if (existingUser != null)
                throw new Exception("El email ya está registrado");

            // Encriptar contraseña
            usuario.Password = HashPassword(usuario.Password);

            _repository.Add(usuario);
        }

        public void Delete(int id)
        {
            var usuario = _repository.GetById(id);
            if (usuario == null)
                throw new Exception("Usuario no encontrado");

            _repository.Delete(id);
        }

        public IEnumerable<Usuario> Find(Func<Usuario, bool> predicate)
        {
            return _repository.Find(predicate);
        }

        public IEnumerable<Usuario> GetAll()
        {
            return _repository.GetAll();
        }

        public Usuario GetById(int id)
        {
            return _repository.GetById(id);
        }

        public Usuario GetByEmail(string email)
        {
            return _repository.GetByEmail(email);
        }

        public void Update(Usuario usuario)
        {
            var existingUser = _repository.GetById(usuario.Id);
            if (existingUser == null)
                throw new Exception("Usuario no encontrado");

            // Validar datos
            if (string.IsNullOrEmpty(usuario.Nombre))
                throw new Exception("El nombre es obligatorio");

            if (string.IsNullOrEmpty(usuario.Apellido))
                throw new Exception("El apellido es obligatorio");

            if (string.IsNullOrEmpty(usuario.Email))
                throw new Exception("El email es obligatorio");

            // Verificar si el email ya existe (si se cambió)
            if (usuario.Email != existingUser.Email)
            {
                var userWithSameEmail = _repository.GetByEmail(usuario.Email);
                if (userWithSameEmail != null)
                    throw new Exception("El email ya está registrado");
            }

            // Mantener la contraseña encriptada si no se cambió
            if (string.IsNullOrEmpty(usuario.Password))
                usuario.Password = existingUser.Password;
            else
                usuario.Password = HashPassword(usuario.Password);

            _repository.Update(usuario);
        }

        public bool ValidateLogin(string email, string password)
        {
            var usuario = _repository.GetByEmail(email);
            if (usuario == null)
                return false;

            return VerifyPassword(password, usuario.Password);
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            string hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }
    }
}