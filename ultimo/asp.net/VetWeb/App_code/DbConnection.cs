using System.Configuration; // Necesario para usar ConfigurationManager
using System.Data.SqlClient; // Necesario para SqlConnection

namespace VetWeb.App_Code // Asegúrate de que este namespace coincida con el de tu proyecto
{
    public static class DbConnection
    {
        // Propiedad estática para obtener la cadena de conexión directamente del Web.config
        public static string ConnectionString
        {
            get
            {
                // Asegúrate de que el nombre "Conexion" coincida exactamente con tu Web.config
                return ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;
            }
        }

        // Puedes opcionalmente añadir un método para obtener una conexión abierta,
        // aunque es común crear la conexión directamente donde se usa.
        public static SqlConnection GetOpenConnection()
        {
            SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }
    }
}