using System.Data;
using System.Data.SqlClient;
using VetWeb.App_Code;

namespace VetWeb.DAL
{
    public class ClientesDAL
    {
        public static DataTable GetAllClientes()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(DbConnection.ConnectionString))
            {
                string query = "SELECT ClienteID, PrimerNombre + ' ' + ApellidoPaterno AS NombreCompleto FROM Clientes ORDER BY PrimerNombre";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            return dt;
        }
    }
}