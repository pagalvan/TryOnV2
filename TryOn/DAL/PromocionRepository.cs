using ENTITIES;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class PromocionRepository : BaseDatos, IRepository<Promocion>
    {
        public void Add(Promocion promocion)
        {
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"INSERT INTO promociones (titulo, descripcion, porcentaje_descuento, fecha_inicio, fecha_fin, 
                                        prenda_id, categoria_id, codigo_promocion, activa) 
                                        VALUES (@titulo, @descripcion, @porcentaje_descuento, @fecha_inicio, @fecha_fin, 
                                        @prenda_id, @categoria_id, @codigo_promocion, @activa) 
                                        RETURNING id";

                    cmd.Parameters.AddWithValue("@titulo", promocion.Titulo);
                    cmd.Parameters.AddWithValue("@descripcion", NpgsqlDbType.Text, (object)promocion.Descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@porcentaje_descuento", promocion.PorcentajeDescuento);
                    cmd.Parameters.AddWithValue("@fecha_inicio", promocion.FechaInicio);
                    cmd.Parameters.AddWithValue("@fecha_fin", promocion.FechaFin);
                    cmd.Parameters.AddWithValue("@prenda_id", NpgsqlDbType.Integer, (object)promocion.PrendaId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@categoria_id", NpgsqlDbType.Integer, (object)promocion.CategoriaId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@codigo_promocion", promocion.CodigoPromocion);
                    cmd.Parameters.AddWithValue("@activa", promocion.Activa);

                    promocion.Id = (int)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar promoción: " + ex.Message);
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
                    cmd.CommandText = "DELETE FROM promociones WHERE id = @id";
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar promoción: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        public IEnumerable<Promocion> Find(Func<Promocion, bool> predicate)
        {
            return GetAll().Where(predicate);
        }

        public IEnumerable<Promocion> GetAll()
        {
            var promociones = new List<Promocion>();
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"SELECT p.*, pr.nombre as prenda_nombre, c.nombre as categoria_nombre 
                                        FROM promociones p 
                                        LEFT JOIN prendas pr ON p.prenda_id = pr.id 
                                        LEFT JOIN categorias c ON p.categoria_id = c.id";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            promociones.Add(MapearPromocion(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener promociones: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return promociones;
        }

        public Promocion GetById(int id)
        {
            Promocion promocion = null;
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"SELECT p.*, pr.nombre as prenda_nombre, c.nombre as categoria_nombre 
                                        FROM promociones p 
                                        LEFT JOIN prendas pr ON p.prenda_id = pr.id 
                                        LEFT JOIN categorias c ON p.categoria_id = c.id 
                                        WHERE p.id = @id";
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            promocion = MapearPromocion(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener promoción: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return promocion;
        }

        public IEnumerable<Promocion> GetPromocionesActivas()
        {
            var promociones = new List<Promocion>();
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"SELECT p.*, pr.nombre as prenda_nombre, c.nombre as categoria_nombre 
                                        FROM promociones p 
                                        LEFT JOIN prendas pr ON p.prenda_id = pr.id 
                                        LEFT JOIN categorias c ON p.categoria_id = c.id 
                                        WHERE p.activa = true AND p.fecha_fin >= CURRENT_TIMESTAMP";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            promociones.Add(MapearPromocion(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener promociones activas: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
            return promociones;
        }

        public void Save()
        {
            // No es necesario implementar en este caso ya que cada método maneja su propia transacción
        }

        public void Update(Promocion promocion)
        {
            try
            {
                AbrirConexion();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conexion;
                    cmd.CommandText = @"UPDATE promociones 
                                        SET titulo = @titulo, 
                                            descripcion = @descripcion, 
                                            porcentaje_descuento = @porcentaje_descuento, 
                                            fecha_inicio = @fecha_inicio, 
                                            fecha_fin = @fecha_fin, 
                                            prenda_id = @prenda_id, 
                                            categoria_id = @categoria_id, 
                                            codigo_promocion = @codigo_promocion, 
                                            activa = @activa 
                                        WHERE id = @id";

                    cmd.Parameters.AddWithValue("@id", promocion.Id);
                    cmd.Parameters.AddWithValue("@titulo", promocion.Titulo);
                    cmd.Parameters.AddWithValue("@descripcion", NpgsqlDbType.Text, (object)promocion.Descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@porcentaje_descuento", promocion.PorcentajeDescuento);
                    cmd.Parameters.AddWithValue("@fecha_inicio", promocion.FechaInicio);
                    cmd.Parameters.AddWithValue("@fecha_fin", promocion.FechaFin);
                    cmd.Parameters.AddWithValue("@prenda_id", NpgsqlDbType.Integer, (object)promocion.PrendaId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@categoria_id", NpgsqlDbType.Integer, (object)promocion.CategoriaId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@codigo_promocion", promocion.CodigoPromocion);
                    cmd.Parameters.AddWithValue("@activa", promocion.Activa);

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar promoción: " + ex.Message);
            }
            finally
            {
                CerrarConexion();
            }
        }

        private Promocion MapearPromocion(NpgsqlDataReader reader)
        {
            var promocion = new Promocion
            {
                Id = Convert.ToInt32(reader["id"]),
                Titulo = reader["titulo"].ToString(),
                Descripcion = reader["descripcion"] == DBNull.Value ? null : reader["descripcion"].ToString(),
                PorcentajeDescuento = Convert.ToDecimal(reader["porcentaje_descuento"]),
                FechaInicio = Convert.ToDateTime(reader["fecha_inicio"]),
                FechaFin = Convert.ToDateTime(reader["fecha_fin"]),
                PrendaId = reader["prenda_id"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["prenda_id"]),
                CategoriaId = reader["categoria_id"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["categoria_id"]),
                CodigoPromocion = reader["codigo_promocion"].ToString(),
                Activa = Convert.ToBoolean(reader["activa"])
            };

            if (reader["prenda_id"] != DBNull.Value && reader["prenda_nombre"] != DBNull.Value)
            {
                promocion.Prenda = new Prenda
                {
                    Id = Convert.ToInt32(reader["prenda_id"]),
                    Nombre = reader["prenda_nombre"].ToString()
                };
            }

            if (reader["categoria_id"] != DBNull.Value && reader["categoria_nombre"] != DBNull.Value)
            {
                promocion.Categoria = new Categoria
                {
                    Id = Convert.ToInt32(reader["categoria_id"]),
                    Nombre = reader["categoria_nombre"].ToString()
                };
            }

            return promocion;
        }
    }
}
