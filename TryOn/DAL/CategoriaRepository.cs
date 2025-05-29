using DAL;
using ENTITIES;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TryOn.DAL
{
    public class CategoryRepository : BaseDatos, IRepository<Categoria>
    {
        public void Add(Categoria categoria)
        {
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"INSERT INTO categorias (nombre, descripcion) 
                                        VALUES (@nombre, @descripcion) 
                                        RETURNING id";

                    cmd.Parameters.AddWithValue("@nombre", categoria.Nombre);
                    cmd.Parameters.AddWithValue("@descripcion", NpgsqlDbType.Text, (object)categoria.Descripcion ?? DBNull.Value);

                    categoria.Id = (int)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar categoría: " + ex.Message);
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

                // Verificar si hay prendas que usan esta categoría
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = "SELECT COUNT(*) FROM prendas WHERE categoria_id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    int prendasCount = Convert.ToInt32(cmd.ExecuteScalar());

                    if (prendasCount > 0)
                    {
                        throw new Exception("No se puede eliminar la categoría porque hay prendas asociadas a ella. Actualice primero las prendas relacionadas.");
                    }
                }

                // Eliminar la categoría
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = "DELETE FROM categorias WHERE id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar categoría: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        public IEnumerable<Categoria> Find(Func<Categoria, bool> predicate)
        {
            return GetAll().Where(predicate);
        }

        public IEnumerable<Categoria> GetAll()
        {
            var categorias = new List<Categoria>();
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = "SELECT id, nombre, descripcion FROM categorias ORDER BY nombre";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categorias.Add(MapearCategoria(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener categorías: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return categorias;
        }

        public Categoria GetById(int id)
        {
            Categoria categoria = null;
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = "SELECT id, nombre, descripcion FROM categorias WHERE id = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            categoria = MapearCategoria(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener categoría: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return categoria;
        }

        public Categoria GetByName(string nombre)
        {
            Categoria categoria = null;
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    // Usar TRIM y LOWER para hacer la búsqueda más robusta
                    cmd.CommandText = "SELECT id, nombre, descripcion FROM categorias WHERE LOWER(TRIM(nombre)) = LOWER(TRIM(@nombre))";
                    cmd.Parameters.AddWithValue("@nombre", nombre);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            categoria = MapearCategoria(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener categoría por nombre: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return categoria;
        }


        public void Update(Categoria categoria)
        {
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"UPDATE categorias 
                                        SET nombre = @nombre, 
                                            descripcion = @descripcion
                                        WHERE id = @id";

                    cmd.Parameters.AddWithValue("@id", categoria.Id);
                    cmd.Parameters.AddWithValue("@nombre", categoria.Nombre);
                    cmd.Parameters.AddWithValue("@descripcion", NpgsqlDbType.Text, (object)categoria.Descripcion ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar categoría: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        public bool ExistsByName(string nombre, int? excludeId = null)
        {
            bool exists = false;
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    string query = "SELECT COUNT(*) FROM categorias WHERE LOWER(TRIM(nombre)) = LOWER(TRIM(@nombre))";
                    if (excludeId.HasValue)
                    {
                        query += " AND id != @excludeId";
                    }

                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@nombre", nombre);
                    if (excludeId.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@excludeId", excludeId.Value);
                    }

                    exists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar existencia de categoría: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return exists;
        }

        private Categoria MapearCategoria(NpgsqlDataReader reader)
        {
            return new Categoria
            {
                Id = Convert.ToInt32(reader["id"]),
                Nombre = reader["nombre"].ToString(),
                Descripcion = reader["descripcion"] == DBNull.Value ? null : reader["descripcion"].ToString()
            };
        }
    }
}
