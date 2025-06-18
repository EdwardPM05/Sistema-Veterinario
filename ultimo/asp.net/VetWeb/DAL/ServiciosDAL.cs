using System.Data;
using System.Data.SqlClient;
using VetWeb.App_Code;

namespace VetWeb.DAL
{
    public class ServiciosDAL
    {
        public static DataTable GetAllServicios()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(DbConnection.ConnectionString))
            {
                string query = "SELECT ServicioID, NombreServicio, Precio FROM Servicios ORDER BY NombreServicio";
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