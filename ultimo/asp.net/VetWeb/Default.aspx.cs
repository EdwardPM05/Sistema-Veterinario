using System;
using System.Collections.Generic; // Para Dictionary y List
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization; // Para CultureInfo
using System.Web.Script.Serialization; // Para JavaScriptSerializer
using System.Web.UI; // Necesario para ScriptManager
using System.Web.UI.WebControls;

namespace VetWeb
{
    public partial class Default : System.Web.UI.Page
    {
        // Cadena de conexión obtenida del archivo Web.config
        string cadena = ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarDatosDashboard();
            }
        }

        /// <summary>
        /// Carga todos los datos necesarios para el dashboard (tarjetas de resumen, lista de citas y gráficos).
        /// </summary>
        private void CargarDatosDashboard()
        {
            try
            {
                lblTotalClientes.Text = GetTotalCount("Clientes", "ClienteID").ToString();
                lblTotalMascotas.Text = GetTotalCount("Mascotas", "MascotaID").ToString();
                lblTotalEmpleados.Text = GetTotalCount("Empleados", "EmpleadoID").ToString();
                lblCitasPendientesHoy.Text = GetCitasCountToday().ToString();
                CargarProximasCitas(); // Ahora carga en el Repeater
                CargarDatosGraficos();
            }
            catch (Exception ex)
            {
                // Manejo de errores básico: podrías usar un lblError en el ASPX o un log.
                // Por simplicidad, aquí solo se imprime en la consola de depuración.
                System.Diagnostics.Debug.WriteLine("Error al cargar datos del dashboard: " + ex.Message);
                // Opcional: Mostrar un mensaje genérico al usuario si falla la carga.
                // lblMensajeErrorDashboard.Text = "No se pudieron cargar los datos del dashboard. Intente más tarde.";
            }
        }

        /// <summary>
        /// Obtiene el conteo total de registros en una tabla específica.
        /// </summary>
        /// <param name="tableName">El nombre de la tabla.</param>
        /// <param name="idColumnName">El nombre de la columna ID para contar.</param>
        /// <returns>El número total de registros.</returns>
        private int GetTotalCount(string tableName, string idColumnName)
        {
            int count = 0;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = $"SELECT COUNT({idColumnName}) FROM {tableName}";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    count = Convert.ToInt32(result);
                }
                con.Close();
            }
            return count;
        }

        /// <summary>
        /// Obtiene el conteo de citas programadas para hoy.
        /// </summary>
        /// <returns>El número de citas para el día actual.</returns>
        private int GetCitasCountToday()
        {
            int count = 0;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                // Usamos CONVERT(date, GETDATE()) para obtener solo la fecha sin la hora
                string query = "SELECT COUNT(CitaID) FROM Citas WHERE Fecha = CONVERT(date, GETDATE())";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    count = Convert.ToInt32(result);
                }
                con.Close();
            }
            return count;
        }

        /// <summary>
        /// Carga una lista de las próximas citas (ej. de hoy en adelante) y las enlaza al Repeater.
        /// </summary>
        private void CargarProximasCitas()
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"
                    SELECT TOP 10 -- Limitar a las 10 próximas citas
                        C.Fecha,
                        M.Nombre AS NombreMascota,
                        Cl.PrimerNombre + ' ' + Cl.ApellidoPaterno AS NombreCliente,
                        E.PrimerNombre + ' ' + E.ApellidoPaterno AS NombreEmpleado
                    FROM Citas C
                    INNER JOIN Mascotas M ON C.MascotaID = M.MascotaID
                    INNER JOIN Clientes Cl ON M.ClienteID = Cl.ClienteID
                    INNER JOIN Empleados E ON C.EmpleadoID = E.EmpleadoID
                    WHERE C.Fecha >= CONVERT(date, GETDATE()) -- Citas de hoy en adelante
                    ORDER BY C.Fecha ASC"; // Ordenar por fecha ascendente

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    rptProximasCitas.DataSource = dt; // Enlazar al Repeater
                    rptProximasCitas.DataBind();
                    rptProximasCitas.Visible = true;
                    lblNoCitas.Visible = false;
                }
                else
                {
                    rptProximasCitas.Visible = false; // Ocultar el Repeater
                    lblNoCitas.Visible = true;
                }
            }
        }

        /// <summary>
        /// Carga los datos para los gráficos y los pasa al JavaScript del cliente.
        /// </summary>
        private void CargarDatosGraficos()
        {
            // Datos para el gráfico de Citas por Mes
            Tuple<List<string>, List<int>> citasData = GetCitasCountPerMonth(6); // Últimos 6 meses
            string labelsCitas = new JavaScriptSerializer().Serialize(citasData.Item1); // Accede a la lista de etiquetas
            string dataCitas = new JavaScriptSerializer().Serialize(citasData.Item2);   // Accede a la lista de datos

            // Registra el script para dibujar el gráfico de citas
            ScriptManager.RegisterStartupScript(this, this.GetType(), "drawCitasChart",
                $"drawCitasMesChart({labelsCitas}, {dataCitas});", true);

            // Datos para el gráfico de Mascotas por Especie
            Dictionary<string, int> mascotasPorEspecie = GetMascotasCountByEspecie();
            string labelsMascotasEspecie = new JavaScriptSerializer().Serialize(new List<string>(mascotasPorEspecie.Keys));
            string dataMascotasEspecie = new JavaScriptSerializer().Serialize(new List<int>(mascotasPorEspecie.Values));

            ScriptManager.RegisterStartupScript(this, this.GetType(), "drawMascotasEspecieChart",
                $"drawMascotasEspecieChart({labelsMascotasEspecie}, {dataMascotasEspecie});", true);

            // Datos para el gráfico de Top 5 Servicios Más Utilizados
            Dictionary<string, int> topServicios = GetTopServiciosUtilizados(5);
            string labelsTopServicios = new JavaScriptSerializer().Serialize(new List<string>(topServicios.Keys));
            string dataTopServicios = new JavaScriptSerializer().Serialize(new List<int>(topServicios.Values));

            ScriptManager.RegisterStartupScript(this, this.GetType(), "drawTopServiciosChart",
                $"drawTopServiciosChart({labelsTopServicios}, {dataTopServicios});", true);

            // Datos para el gráfico de Top 5 Empleados con Más Citas
            Tuple<List<string>, List<int>> topEmpleadosData = GetTopEmpleadosConCitas(5);
            string labelsTopEmpleados = new JavaScriptSerializer().Serialize(topEmpleadosData.Item1);
            string dataTopEmpleados = new JavaScriptSerializer().Serialize(topEmpleadosData.Item2);

            ScriptManager.RegisterStartupScript(this, this.GetType(), "drawTopEmpleadosChart",
                $"drawTopEmpleadosChart({labelsTopEmpleados}, {dataTopEmpleados});", true);
        }

        // Se eliminó el método GetServiciosCountBySubcategoria() ya que el gráfico fue removido.

        /// <summary>
        /// Obtiene el conteo de citas por mes para los últimos N meses, asegurando el orden cronológico.
        /// </summary>
        /// <param name="numMonths">Número de meses hacia atrás a incluir en el reporte.</param>
        /// <returns>Un Tuple donde Item1 es la lista de etiquetas de meses en orden cronológico y Item2 es la lista de conteos.</returns>
        private Tuple<List<string>, List<int>> GetCitasCountPerMonth(int numMonths)
        {
            List<string> orderedLabels = new List<string>();
            List<int> orderedData = new List<int>();

            // Diccionario temporal para almacenar los conteos de la base de datos, usando el mes formateado como clave
            // Esto nos permite buscar rápidamente el conteo de cada mes después de obtener los datos.
            Dictionary<string, int> dbCounts = new Dictionary<string, int>();

            // 1. Generar las etiquetas de los últimos N meses en orden cronológico y pre-inicializar los conteos
            DateTime currentDate = DateTime.Now;
            for (int i = numMonths - 1; i >= 0; i--) // Iterar desde el mes más antiguo hacia el actual
            {
                DateTime month = currentDate.AddMonths(-i);
                // Formatear el mes como "MMM yy" (ej: "Abr 23"). Usamos es-ES para los nombres de meses en español.
                string formattedMonth = month.ToString("MMM yy", new CultureInfo("es-ES"));
                orderedLabels.Add(formattedMonth); // Añadir a la lista ordenada de etiquetas
                dbCounts[formattedMonth] = 0;      // Inicializar el conteo a 0 en el diccionario temporal
            }

            // 2. Obtener los conteos reales de la base de datos
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"
                    SELECT 
                        FORMAT(Fecha, 'MMM yy', 'es-ES') AS MesAnio, 
                        COUNT(CitaID) AS TotalCitas
                    FROM Citas
                    WHERE Fecha >= DATEADD(month, -@NumMonths, GETDATE())
                    GROUP BY FORMAT(Fecha, 'MMM yy', 'es-ES'), YEAR(Fecha), MONTH(Fecha) -- Agrupar también por año y mes para asegurar consistencia
                    ORDER BY YEAR(Fecha) ASC, MONTH(Fecha) ASC"; // Ordenar para una lectura más clara si se depura, aunque el diccionario maneja el lookup

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@NumMonths", numMonths);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string mesAnio = reader["MesAnio"].ToString();
                    int totalCitas = Convert.ToInt32(reader["TotalCitas"]);
                    // Actualizar el conteo en el diccionario temporal si el mes existe
                    if (dbCounts.ContainsKey(mesAnio))
                    {
                        dbCounts[mesAnio] = totalCitas;
                    }
                }
                reader.Close();
                con.Close();
            }

            // 3. Rellenar la lista de datos ordenada utilizando las etiquetas cronológicas
            foreach (string label in orderedLabels)
            {
                orderedData.Add(dbCounts[label]); // Obtendrá 0 si no hubo citas para ese mes
            }

            return Tuple.Create(orderedLabels, orderedData);
        }

        /// <summary>
        /// Obtiene el conteo de mascotas agrupadas por especie.
        /// </summary>
        /// <returns>Un diccionario con el nombre de la especie y la cantidad de mascotas.</returns>
        private Dictionary<string, int> GetMascotasCountByEspecie()
        {
            Dictionary<string, int> data = new Dictionary<string, int>();
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"
                    SELECT 
                        E.NombreEspecie, 
                        COUNT(M.MascotaID) AS TotalMascotas
                    FROM Mascotas M
                    INNER JOIN Razas R ON M.RazaID = R.RazaID
                    INNER JOIN Especies E ON R.EspecieID = E.EspecieID
                    GROUP BY E.NombreEspecie
                    ORDER BY TotalMascotas DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    data.Add(reader["NombreEspecie"].ToString(), Convert.ToInt32(reader["TotalMascotas"]));
                }
                reader.Close();
                con.Close();
            }
            return data;
        }

        /// <summary>
        /// Obtiene el top N de servicios más utilizados (basado en CitaServicios).
        /// </summary>
        /// <param name="topN">El número de servicios a obtener.</param>
        /// <returns>Un diccionario con el nombre del servicio y la cantidad de veces utilizado.</returns>
        private Dictionary<string, int> GetTopServiciosUtilizados(int topN)
        {
            Dictionary<string, int> data = new Dictionary<string, int>();
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = $@"
                    SELECT TOP {topN}
                        S.NombreServicio, 
                        COUNT(CS.CitaServicioID) AS VecesUtilizado
                    FROM CitaServicios CS
                    INNER JOIN Servicios S ON CS.ServicioID = S.ServicioID
                    GROUP BY S.NombreServicio
                    ORDER BY VecesUtilizado DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    data.Add(reader["NombreServicio"].ToString(), Convert.ToInt32(reader["VecesUtilizado"]));
                }
                reader.Close();
                con.Close();
            }
            return data;
        }

        /// <summary>
        /// Obtiene el top N de empleados con más citas.
        /// </summary>
        /// <param name="topN">El número de empleados a obtener.</param>
        /// <returns>Un Tuple donde Item1 es la lista de nombres de empleados y Item2 es la lista de conteos de citas.</returns>
        private Tuple<List<string>, List<int>> GetTopEmpleadosConCitas(int topN)
        {
            List<string> labels = new List<string>();
            List<int> data = new List<int>();

            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = $@"
                    SELECT TOP {topN}
                        E.PrimerNombre + ' ' + E.ApellidoPaterno AS NombreCompletoEmpleado,
                        COUNT(C.CitaID) AS TotalCitas
                    FROM Citas C
                    INNER JOIN Empleados E ON C.EmpleadoID = E.EmpleadoID
                    GROUP BY E.PrimerNombre, E.ApellidoPaterno
                    ORDER BY TotalCitas DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    labels.Add(reader["NombreCompletoEmpleado"].ToString());
                    data.Add(Convert.ToInt32(reader["TotalCitas"]));
                }
                reader.Close();
                con.Close();
            }
            return Tuple.Create(labels, data);
        }
    }
}
