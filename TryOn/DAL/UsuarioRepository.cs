using DAL;
using ENTITIES;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace TryOn.DAL
{
    public class UsuarioRepository : BaseDatos, IRepository<Usuario>
    {
        public void Add(Usuario usuario)
        {
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"INSERT INTO usuarios (nombre, apellido, email, password, telefono, direccion, es_admin) 
                                        VALUES (@nombre, @apellido, @email, @password, @telefono, @direccion, @es_admin) 
                                        RETURNING id";

                    cmd.Parameters.AddWithValue("@nombre", usuario.Nombre);
                    cmd.Parameters.AddWithValue("@apellido", usuario.Apellido);
                    cmd.Parameters.AddWithValue("@email", usuario.Email);
                    cmd.Parameters.AddWithValue("@password", usuario.Password);
                    cmd.Parameters.AddWithValue("@telefono", NpgsqlDbType.Varchar, (object)usuario.Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@direccion", NpgsqlDbType.Text, (object)usuario.Direccion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@es_admin", usuario.EsAdmin);

                    usuario.Id = (int)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar usuario: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        public void Delete(int id)
        {
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = "DELETE FROM usuarios WHERE id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar usuario: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        public IEnumerable<Usuario> Find(Func<Usuario, bool> predicate)
        {
            return GetAll().Where(predicate);
        }

        public IEnumerable<Usuario> GetAll()
        {
            var usuarios = new List<Usuario>();
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = "SELECT * FROM usuarios";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usuarios.Add(MapearUsuario(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener usuarios: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return usuarios;
        }

        public Usuario GetById(int id)
        {
            Usuario usuario = null;
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = "SELECT * FROM usuarios WHERE id = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = MapearUsuario(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener usuario: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return usuario;
        }

        public Usuario GetByEmail(string email)
        {
            Usuario usuario = null;
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = "SELECT * FROM usuarios WHERE email = @email";
                    cmd.Parameters.AddWithValue("@email", email);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = MapearUsuario(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener usuario por email: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return usuario;
        }

        public void Save()
        {
            // No es necesario implementar en este caso ya que cada método maneja su propia transacción
        }

        public void Update(Usuario usuario)
        {
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"UPDATE usuarios 
                                        SET nombre = @nombre, 
                                            apellido = @apellido, 
                                            email = @email, 
                                            telefono = @telefono, 
                                            direccion = @direccion, 
                                            es_admin = @es_admin 
                                        WHERE id = @id";

                    cmd.Parameters.AddWithValue("@id", usuario.Id);
                    cmd.Parameters.AddWithValue("@nombre", usuario.Nombre);
                    cmd.Parameters.AddWithValue("@apellido", usuario.Apellido);
                    cmd.Parameters.AddWithValue("@email", usuario.Email);
                    cmd.Parameters.AddWithValue("@telefono", NpgsqlDbType.Varchar, (object)usuario.Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@direccion", NpgsqlDbType.Text, (object)usuario.Direccion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@es_admin", usuario.EsAdmin);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar usuario: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        private Usuario MapearUsuario(NpgsqlDataReader reader)
        {
            return new Usuario
            {
                Id = Convert.ToInt32(reader["id"]),
                Nombre = reader["nombre"].ToString(),
                Apellido = reader["apellido"].ToString(),
                Email = reader["email"].ToString(),
                Password = reader["password"].ToString(),
                Telefono = reader["telefono"] == DBNull.Value ? null : reader["telefono"].ToString(),
                Direccion = reader["direccion"] == DBNull.Value ? null : reader["direccion"].ToString(),
                EsAdmin = Convert.ToBoolean(reader["es_admin"]),
                FechaRegistro = Convert.ToDateTime(reader["fecha_registro"])
            };
        }
    }
}