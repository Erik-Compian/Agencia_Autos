using System;
using System.Data;
using MySql.Data.MySqlClient; 

namespace AgenciaMVC1.Data
{
    public class ConexionBD
    {
        private static ConexionBD _instancia = null;
        private MySqlConnection _conexion;
        private string _cadenaConexion = "Server=localhost;Database=agencia_autos;Uid=root;";

        // Constructor privado para evitar instanciación externa (POO)
        private ConexionBD()
        {
            _conexion = new MySqlConnection(_cadenaConexion);
        }

        // Método para obtener la única instancia (Singleton)
        public static ConexionBD Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new ConexionBD();
                }
                return _instancia;
            }
        }

        public MySqlConnection ObtenerConexion()
        {
            if (_conexion.State == ConnectionState.Closed)
            {
                _conexion.Open();
            }
            return _conexion;
        }

        public void CerrarConexion()
        {
            if (_conexion.State == ConnectionState.Open)
            {
                _conexion.Close();
            }
        }
    }
}