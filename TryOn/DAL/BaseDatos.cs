﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class BaseDatos
    {
        string cadenaConexion = "Host=localhost;Port=5432;Username=postgres;Password=upctryon;Database=tryon";
        protected NpgsqlConnection conexion;

        public BaseDatos()
        {
            conexion = new NpgsqlConnection();
            conexion.ConnectionString = cadenaConexion;
        }

        public void AbrirConexion()
        {
            if (conexion.State != System.Data.ConnectionState.Open)
            {
                try
                {
                    conexion.Open();
                }
                catch (Exception ex)
                {
                    throw new Exception("No se pudo abrir la conexión: " + ex.Message);
                }
            }
        }

        public void CerrarConexion()
        {
            if (conexion.State == System.Data.ConnectionState.Open)
            {
                conexion.Close();
            }
        }
    }
}
