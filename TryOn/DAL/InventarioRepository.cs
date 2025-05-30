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
    public class InventarioRepository : BaseDatos, IRepository<Inventario>
    {
        public void Add(Inventario inventario)
        {
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"INSERT INTO inventario (prenda_id, talla, color, cantidad, ubicacion) 
                                        VALUES (@prenda_id, @talla, @color, @cantidad, @ubicacion) 
                                        RETURNING id";

                    cmd.Parameters.AddWithValue("@prenda_id", inventario.PrendaId);
                    cmd.Parameters.AddWithValue("@talla", inventario.Talla);
                    cmd.Parameters.AddWithValue("@color", inventario.Color);
                    cmd.Parameters.AddWithValue("@cantidad", inventario.Cantidad);
                    cmd.Parameters.AddWithValue("@ubicacion", NpgsqlDbType.Varchar, (object)inventario.Ubicacion ?? DBNull.Value);

                    inventario.Id = (int)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar inventario: " + ex.Message);
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
                    cmd.CommandText = "SELECT eliminar_inventario_completo(@inventario_id)";
                    cmd.Parameters.AddWithValue("@inventario_id", id);

                    string resultado = cmd.ExecuteScalar()?.ToString();

                    // Opcional: usar el resultado para mostrar información
                    Console.WriteLine(resultado);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar inventario: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }
        

        public IEnumerable<Inventario> Find(Func<Inventario, bool> predicate)
        {
            return GetAll().Where(predicate);
        }

        public IEnumerable<Inventario> GetAll()
        {
            var inventarios = new List<Inventario>();
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"SELECT i.*, p.codigo, p.nombre as prenda_nombre, p.precio_venta, p.imagen_url 
                                        FROM inventario i 
                                        JOIN prendas p ON i.prenda_id = p.id";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            inventarios.Add(MapearInventario(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener inventarios: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return inventarios;
        }

        public Inventario GetById(int id)
        {
            Inventario inventario = null;
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"SELECT i.*, p.codigo, p.nombre as prenda_nombre, p.precio_venta, p.imagen_url 
                                        FROM inventario i 
                                        JOIN prendas p ON i.prenda_id = p.id 
                                        WHERE i.id = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            inventario = MapearInventario(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener inventario: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return inventario;
        }

        public IEnumerable<Inventario> GetByPrendaId(int prendaId)
        {
            var inventarios = new List<Inventario>();
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"SELECT i.*, p.codigo, p.nombre as prenda_nombre, p.precio_venta, p.imagen_url 
                                        FROM inventario i 
                                        JOIN prendas p ON i.prenda_id = p.id 
                                        WHERE i.prenda_id = @prenda_id";
                    cmd.Parameters.AddWithValue("@prenda_id", prendaId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            inventarios.Add(MapearInventario(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener inventario por prenda: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return inventarios;
        }

        public void Update(Inventario inventario)
        {
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"UPDATE inventario 
                                        SET prenda_id = @prenda_id, 
                                            talla = @talla, 
                                            color = @color, 
                                            cantidad = @cantidad, 
                                            ubicacion = @ubicacion 
                                        WHERE id = @id";

                    cmd.Parameters.AddWithValue("@id", inventario.Id);
                    cmd.Parameters.AddWithValue("@prenda_id", inventario.PrendaId);
                    cmd.Parameters.AddWithValue("@talla", inventario.Talla);
                    cmd.Parameters.AddWithValue("@color", inventario.Color);
                    cmd.Parameters.AddWithValue("@cantidad", inventario.Cantidad);
                    cmd.Parameters.AddWithValue("@ubicacion", NpgsqlDbType.Varchar, (object)inventario.Ubicacion ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar inventario: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        private Inventario MapearInventario(NpgsqlDataReader reader)
        {
            var inventario = new Inventario
            {
                Id = Convert.ToInt32(reader["id"]),
                PrendaId = Convert.ToInt32(reader["prenda_id"]),
                Talla = reader["talla"].ToString(),
                Color = reader["color"].ToString(),
                Cantidad = Convert.ToInt32(reader["cantidad"]),
                Ubicacion = reader["ubicacion"] == DBNull.Value ? null : reader["ubicacion"].ToString()
            };

            inventario.Prenda = new Prenda
            {
                Id = Convert.ToInt32(reader["prenda_id"]),
                Codigo = reader["codigo"].ToString(),
                Nombre = reader["prenda_nombre"].ToString(),
                PrecioVenta = Convert.ToDecimal(reader["precio_venta"]),
                ImagenUrl = reader["imagen_url"] == DBNull.Value ? null : reader["imagen_url"].ToString()
            };

            return inventario;
        }
    }
}
