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
    public class PedidoRepository : BaseDatos, IRepository<Pedido>
    {
        public void Add(Pedido pedido)
        {
            try
            {
                AbrirConexion();

                // Iniciar transacción
                using (var transaction = conexion.BeginTransaction())
                {
                    try
                    {
                        // Insertar pedido
                        using (var cmd = new NpgsqlCommand())
                        {
                            cmd.Connection = conexion;
                            cmd.Transaction = transaction;
                            cmd.CommandText = @"INSERT INTO pedidos (usuario_id, estado, total, direccion_envio, metodo_pago) 
                                            VALUES (@usuario_id, @estado, @total, @direccion_envio, @metodo_pago) 
                                            RETURNING id";

                            cmd.Parameters.AddWithValue("@usuario_id", pedido.UsuarioId);
                            cmd.Parameters.AddWithValue("@estado", pedido.Estado);
                            cmd.Parameters.AddWithValue("@total", pedido.Total);
                            cmd.Parameters.AddWithValue("@direccion_envio", NpgsqlDbType.Text, (object)pedido.DireccionEnvio ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@metodo_pago", NpgsqlDbType.Varchar, (object)pedido.MetodoPago ?? DBNull.Value);

                            pedido.Id = (int)cmd.ExecuteScalar();
                        }

                        // Insertar detalles del pedido
                        if (pedido.Detalles != null && pedido.Detalles.Count > 0)
                        {
                            foreach (var detalle in pedido.Detalles)
                            {
                                using (var cmd = new NpgsqlCommand())
                                {
                                    cmd.Connection = conexion;
                                    cmd.Transaction = transaction;
                                    cmd.CommandText = @"INSERT INTO detalles_pedido (pedido_id, inventario_id, cantidad, precio_unitario, subtotal) 
                                                    VALUES (@pedido_id, @inventario_id, @cantidad, @precio_unitario, @subtotal) 
                                                    RETURNING id";

                                    cmd.Parameters.AddWithValue("@pedido_id", pedido.Id);
                                    cmd.Parameters.AddWithValue("@inventario_id", detalle.InventarioId);
                                    cmd.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                                    cmd.Parameters.AddWithValue("@precio_unitario", detalle.PrecioUnitario);
                                    cmd.Parameters.AddWithValue("@subtotal", detalle.Subtotal);

                                    detalle.Id = (int)cmd.ExecuteScalar();
                                }

                                // Actualizar inventario
                                using (var cmd = new NpgsqlCommand())
                                {
                                    cmd.Connection = conexion;
                                    cmd.Transaction = transaction;
                                    cmd.CommandText = @"UPDATE inventario 
                                                    SET cantidad = cantidad - @cantidad 
                                                    WHERE id = @inventario_id";

                                    cmd.Parameters.AddWithValue("@inventario_id", detalle.InventarioId);
                                    cmd.Parameters.AddWithValue("@cantidad", detalle.Cantidad);

                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar pedido: " + ex.Message);
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
                using (var transaction = conexion.BeginTransaction())
                {
                    try
                    {
                        // Eliminar detalles del pedido
                        using (var cmd = new NpgsqlCommand())
                        {
                            cmd.Connection = conexion;
                            cmd.Transaction = transaction;
                            cmd.CommandText = "DELETE FROM detalles_pedido WHERE pedido_id = @pedido_id";
                            cmd.Parameters.AddWithValue("@pedido_id", id);
                            cmd.ExecuteNonQuery();
                        }

                        // Eliminar pedido
                        using (var cmd = new NpgsqlCommand())
                        {
                            cmd.Connection = conexion;
                            cmd.Transaction = transaction;
                            cmd.CommandText = "DELETE FROM pedidos WHERE id = @id";
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar pedido: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        public IEnumerable<Pedido> Find(Func<Pedido, bool> predicate)
        {
            return GetAll().Where(predicate);
        }

        public IEnumerable<Pedido> GetAll()
        {
            var pedidos = new List<Pedido>();
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"SELECT p.*, u.nombre as usuario_nombre, u.apellido as usuario_apellido 
                                        FROM pedidos p 
                                        JOIN usuarios u ON p.usuario_id = u.id";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pedidos.Add(MapearPedido(reader));
                        }
                    }
                }

                // Cargar detalles para cada pedido
                foreach (var pedido in pedidos)
                {
                    pedido.Detalles = GetDetallesByPedidoId(pedido.Id).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener pedidos: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return pedidos;
        }

        public Pedido GetById(int id)
        {
            Pedido pedido = null;
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"SELECT p.*, u.nombre as usuario_nombre, u.apellido as usuario_apellido 
                                        FROM pedidos p 
                                        JOIN usuarios u ON p.usuario_id = u.id 
                                        WHERE p.id = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            pedido = MapearPedido(reader);
                        }
                    }
                }

                if (pedido != null)
                {
                    pedido.Detalles = GetDetallesByPedidoId(pedido.Id).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener pedido: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return pedido;
        }

        public IEnumerable<DetallePedido> GetDetallesByPedidoId(int pedidoId)
        {
            var detalles = new List<DetallePedido>();
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"SELECT dp.*, i.prenda_id, i.talla, i.color, p.nombre as prenda_nombre, p.imagen_url 
                                        FROM detalles_pedido dp 
                                        JOIN inventario i ON dp.inventario_id = i.id 
                                        JOIN prendas p ON i.prenda_id = p.id 
                                        WHERE dp.pedido_id = @pedido_id";
                    cmd.Parameters.AddWithValue("@pedido_id", pedidoId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            detalles.Add(MapearDetallePedido(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener detalles del pedido: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return detalles;
        }


        public void Update(Pedido pedido)
        {
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"UPDATE pedidos 
                                        SET estado = @estado, 
                                            direccion_envio = @direccion_envio, 
                                            metodo_pago = @metodo_pago 
                                        WHERE id = @id";

                    cmd.Parameters.AddWithValue("@id", pedido.Id);
                    cmd.Parameters.AddWithValue("@estado", pedido.Estado);
                    cmd.Parameters.AddWithValue("@direccion_envio", NpgsqlDbType.Text, (object)pedido.DireccionEnvio ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@metodo_pago", NpgsqlDbType.Varchar, (object)pedido.MetodoPago ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar pedido: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        private Pedido MapearPedido(NpgsqlDataReader reader)
        {
            var pedido = new Pedido
            {
                Id = Convert.ToInt32(reader["id"]),
                UsuarioId = Convert.ToInt32(reader["usuario_id"]),
                FechaPedido = Convert.ToDateTime(reader["fecha_pedido"]),
                Estado = reader["estado"].ToString(),
                Total = Convert.ToDecimal(reader["total"]),
                DireccionEnvio = reader["direccion_envio"] == DBNull.Value ? null : reader["direccion_envio"].ToString(),
                MetodoPago = reader["metodo_pago"] == DBNull.Value ? null : reader["metodo_pago"].ToString()
            };

            pedido.Usuario = new Usuario
            {
                Id = Convert.ToInt32(reader["usuario_id"]),
                Nombre = reader["usuario_nombre"].ToString(),
                Apellido = reader["usuario_apellido"].ToString()
            };

            return pedido;
        }

        private DetallePedido MapearDetallePedido(NpgsqlDataReader reader)
        {
            var detalle = new DetallePedido
            {
                Id = Convert.ToInt32(reader["id"]),
                PedidoId = Convert.ToInt32(reader["pedido_id"]),
                InventarioId = Convert.ToInt32(reader["inventario_id"]),
                Cantidad = Convert.ToInt32(reader["cantidad"]),
                PrecioUnitario = Convert.ToDecimal(reader["precio_unitario"]),
                Subtotal = Convert.ToDecimal(reader["subtotal"])
            };

            detalle.Inventario = new Inventario
            {
                Id = Convert.ToInt32(reader["inventario_id"]),
                PrendaId = Convert.ToInt32(reader["prenda_id"]),
                Talla = reader["talla"].ToString(),
                Color = reader["color"].ToString(),
                Prenda = new Prenda
                {
                    Id = Convert.ToInt32(reader["prenda_id"]),
                    Nombre = reader["prenda_nombre"].ToString(),
                    ImagenUrl = reader["imagen_url"] == DBNull.Value ? null : reader["imagen_url"].ToString()
                }
            };

            return detalle;
        }
    }
}
