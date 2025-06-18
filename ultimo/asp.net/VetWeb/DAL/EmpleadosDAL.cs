using System.Data;
using System.Data.SqlClient;
using VetWeb.App_Code; // Asegúrate de que este namespace coincida

namespace VetWeb.DAL
{
    public class EmpleadosDAL
    {
        public static DataTable GetAllEmpleados()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(DbConnection.ConnectionString))
            {
                string query = "SELECT EmpleadoID, PrimerNombre + ' ' + ApellidoPaterno AS NombreCompleto FROM Empleados ORDER BY PrimerNombre";
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