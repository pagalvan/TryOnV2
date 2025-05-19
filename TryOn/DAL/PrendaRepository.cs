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
    public class PrendaRepository : BaseDatos, IRepository<Prenda>
    {
        public void Add(Prenda prenda)
        {
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"INSERT INTO prendas (codigo, nombre, descripcion, precio_venta, costo, categoria_id, imagen_url) 
                                        VALUES (@codigo, @nombre, @descripcion, @precio_venta, @costo, @categoria_id, @imagen_url) 
                                        RETURNING id";

                    cmd.Parameters.AddWithValue("@codigo", prenda.Codigo);
                    cmd.Parameters.AddWithValue("@nombre", prenda.Nombre);
                    cmd.Parameters.AddWithValue("@descripcion", NpgsqlDbType.Text, (object)prenda.Descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@precio_venta", prenda.PrecioVenta);
                    cmd.Parameters.AddWithValue("@costo", prenda.Costo);
                    cmd.Parameters.AddWithValue("@categoria_id", prenda.CategoriaId);
                    cmd.Parameters.AddWithValue("@imagen_url", NpgsqlDbType.Varchar, (object)prenda.ImagenUrl ?? DBNull.Value);

                    prenda.Id = (int)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar prenda: " + ex.Message);
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
                    cmd.CommandText = "DELETE FROM prendas WHERE id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar prenda: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        public IEnumerable<Prenda> Find(Func<Prenda, bool> predicate)
        {
            return GetAll().Where(predicate);
        }

        public IEnumerable<Prenda> GetAll()
        {
            var prendas = new List<Prenda>();
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"SELECT p.*, c.nombre as categoria_nombre, c.descripcion as categoria_descripcion 
                                        FROM prendas p 
                                        LEFT JOIN categorias c ON p.categoria_id = c.id";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            prendas.Add(MapearPrenda(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener prendas: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return prendas;
        }

        public Prenda GetById(int id)
        {
            Prenda prenda = null;
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"SELECT p.*, c.nombre as categoria_nombre, c.descripcion as categoria_descripcion 
                                        FROM prendas p 
                                        LEFT JOIN categorias c ON p.categoria_id = c.id 
                                        WHERE p.id = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            prenda = MapearPrenda(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener prenda: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return prenda;
        }

        public void Save()
        {
            // No es necesario implementar en este caso ya que cada método maneja su propia transacción
        }

        public void Update(Prenda prenda)
        {
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"UPDATE prendas 
                                        SET codigo = @codigo, 
                                            nombre = @nombre, 
                                            descripcion = @descripcion, 
                                            precio_venta = @precio_venta, 
                                            costo = @costo, 
                                            categoria_id = @categoria_id, 
                                            imagen_url = @imagen_url 
                                        WHERE id = @id";

                    cmd.Parameters.AddWithValue("@id", prenda.Id);
                    cmd.Parameters.AddWithValue("@codigo", prenda.Codigo);
                    cmd.Parameters.AddWithValue("@nombre", prenda.Nombre);
                    cmd.Parameters.AddWithValue("@descripcion", NpgsqlDbType.Text, (object)prenda.Descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@precio_venta", prenda.PrecioVenta);
                    cmd.Parameters.AddWithValue("@costo", prenda.Costo);
                    cmd.Parameters.AddWithValue("@categoria_id", prenda.CategoriaId);
                    cmd.Parameters.AddWithValue("@imagen_url", NpgsqlDbType.Varchar, (object)prenda.ImagenUrl ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar prenda: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        private Prenda MapearPrenda(NpgsqlDataReader reader)
        {
            var prenda = new Prenda
            {
                Id = Convert.ToInt32(reader["id"]),
                Codigo = reader["codigo"].ToString(),
                Nombre = reader["nombre"].ToString(),
                Descripcion = reader["descripcion"] == DBNull.Value ? null : reader["descripcion"].ToString(),
                PrecioVenta = Convert.ToDecimal(reader["precio_venta"]),
                Costo = Convert.ToDecimal(reader["costo"]),
                CategoriaId = Convert.ToInt32(reader["categoria_id"]),
                ImagenUrl = reader["imagen_url"] == DBNull.Value ? null : reader["imagen_url"].ToString(),
                FechaCreacion = Convert.ToDateTime(reader["fecha_creacion"])
            };

            if (reader["categoria_nombre"] != DBNull.Value)
            {
                prenda.Categoria = new Categoria
                {
                    Id = Convert.ToInt32(reader["categoria_id"]),
                    Nombre = reader["categoria_nombre"].ToString(),
                    Descripcion = reader["categoria_descripcion"] == DBNull.Value ? null : reader["categoria_descripcion"].ToString()
                };
            }

            return prenda;
        }
    }
}
