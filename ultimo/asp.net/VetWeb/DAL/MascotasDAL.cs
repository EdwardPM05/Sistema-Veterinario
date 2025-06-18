using System.Data;
using System.Data.SqlClient;
using VetWeb.App_Code;

namespace VetWeb.DAL
{
    public class MascotasDAL
    {
        public static DataTable GetMascotasByClienteID(int clienteId)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(DbConnection.ConnectionString))
            {
                string query = "SELECT MascotaID, Nombre FROM Mascotas WHERE ClienteID = @ClienteID ORDER BY Nombre";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ClienteID", clienteId);
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            return dt;
        }
    }
}