using System;
using System.Collections.Generic; // Para usar List<T> si creas objetos de modelo
using System.Data;
using System.Data.SqlClient;
using VetWeb.App_Code; // Asegúrate de que este namespace coincida con el de DbConnection

namespace VetWeb.DAL // Ajusta el namespace a tu proyecto
{
    public class CitasDAL
    {
        // ===========================================
        // Métodos para la tabla Citas
        // ===========================================

        /// <summary>
        /// Inserta una nueva cita en la base de datos y retorna el ID de la cita creada.
        /// </summary>
        public static int InsertarCita(DateTime fecha, int mascotaId, int empleadoId)
        {
            int citaId = -1;
            using (SqlConnection con = new SqlConnection(DbConnection.ConnectionString))
            {
                // Agregamos GETDATE() como valor por defecto en la tabla, si necesitas la fecha de creación
                // Si la fecha en la BD incluye hora y minuto, asegúrate de que el DateTime coincida
                string query = "INSERT INTO Citas (Fecha, MascotaID, EmpleadoID) VALUES (@Fecha, @MascotaID, @EmpleadoID); SELECT SCOPE_IDENTITY();";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Fecha", fecha);
                    cmd.Parameters.AddWithValue("@MascotaID", mascotaId);
                    cmd.Parameters.AddWithValue("@EmpleadoID", empleadoId);

                    con.Open();
                    citaId = Convert.ToInt32(cmd.ExecuteScalar()); // SCOPE_IDENTITY() retorna el último ID insertado
                }
            }
            return citaId;
        }

        /// <summary>
        /// Actualiza una cita existente en la base de datos.
        /// </summary>
        public static bool ActualizarCita(int citaId, DateTime fecha, int mascotaId, int empleadoId)
        {
            using (SqlConnection con = new SqlConnection(DbConnection.ConnectionString))
            {
                string query = "UPDATE Citas SET Fecha = @Fecha, MascotaID = @MascotaID, EmpleadoID = @EmpleadoID WHERE CitaID = @CitaID";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Fecha", fecha);
                    cmd.Parameters.AddWithValue("@MascotaID", mascotaId);
                    cmd.Parameters.AddWithValue("@EmpleadoID", empleadoId);
                    cmd.Parameters.AddWithValue("@CitaID", citaId);

                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Elimina una cita por su ID.
        /// (Considera si necesitas eliminar también los CitaServicios relacionados antes o usar CASCADE DELETE en la BD)
        /// </summary>
        public static bool EliminarCita(int citaId)
        {
            using (SqlConnection con = new SqlConnection(DbConnection.ConnectionString))
            {
                string query = "DELETE FROM Citas WHERE CitaID = @CitaID";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CitaID", citaId);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Obtiene una cita por su ID.
        /// </summary>
        public static DataTable GetCitaByID(int citaId)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(DbConnection.ConnectionString))
            {
                string query = "SELECT CitaID, Fecha, MascotaID, EmpleadoID FROM Citas WHERE CitaID = @CitaID";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CitaID", citaId);
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            return dt;
        }

        /// <summary>
        /// Obtiene todas las citas.
        /// </summary>
        public static DataTable GetAllCitas()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(DbConnection.ConnectionString))
            {
                // Puedes necesitar JOINs con Mascotas y Empleados aquí para mostrar nombres
                string query = @"
                    SELECT C.CitaID, C.Fecha, M.Nombre AS NombreMascota, E.PrimerNombre + ' ' + E.ApellidoPaterno AS NombreEmpleado, C.MascotaID, C.EmpleadoID
                    FROM Citas C
                    INNER JOIN Mascotas M ON C.MascotaID = M.MascotaID
                    INNER JOIN Empleados E ON C.EmpleadoID = E.EmpleadoID
                    ORDER BY C.Fecha DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            return dt;
        }

        /// <summary>
        /// Obtiene citas para un empleado en una fecha específica (para verificar disponibilidad).
        /// </summary>
        public static DataTable GetCitasByEmpleadoAndDate(int empleadoId, DateTime fecha)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(DbConnection.ConnectionString))
            {
                // Queremos ver si hay alguna cita en el mismo día y hora para ese empleado.
                // Aquí, asumo que una cita tiene una duración y se solapa. Para una verificación estricta,
                // podrías buscar por un rango de tiempo.
                // Si la hora es importante para la disponibilidad, usa la fecha y hora completa en la comparación.
                string query = "SELECT CitaID, Fecha FROM Citas WHERE EmpleadoID = @EmpleadoID AND CAST(Fecha AS DATE) = CAST(@Fecha AS DATE)";
                // Si quieres verificar solapamiento exacto de fecha y hora:
                // string query = "SELECT CitaID, Fecha FROM Citas WHERE EmpleadoID = @EmpleadoID AND Fecha = @Fecha";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@EmpleadoID", empleadoId);
                    cmd.Parameters.AddWithValue("@Fecha", fecha.Date); // Compara solo la fecha
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }
            }
            return dt;
        }

        /// <summary>
        /// Verifica si un empleado tiene una cita que se solapa con una fecha y hora específicas.
        /// Asume que una cita tiene una duración implícita. Puedes ajustar la lógica de solapamiento.
        /// Por simplicidad, esto verifica si ya hay una cita EXACTA en esa fecha y hora para el empleado.
        /// Si las citas tienen duración, necesitarías una columna `Duracion` en `Citas` y calcular solapamientos.
        /// </summary>
        public static bool CheckEmpleadoAvailability(int empleadoId, DateTime fechaHoraCita)
        {
            using (SqlConnection con = new SqlConnection(DbConnection.ConnectionString))
            {
                // Consulta para verificar si ya existe una cita para el empleado en la hora y fecha exacta
                // Si las citas tienen una duración, la lógica sería más compleja, por ejemplo:
                // WHERE EmpleadoID = @EmpleadoID AND @FechaHoraCita < DATEADD(minute, DuracionCita, Fecha) AND DATEADD(minute, @DuracionNueva, @FechaHoraCita) > Fecha
                string query = "SELECT COUNT(*) FROM Citas WHERE EmpleadoID = @EmpleadoID AND Fecha = @FechaHoraCita";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@EmpleadoID", empleadoId);
                    cmd.Parameters.AddWithValue("@FechaHoraCita", fechaHoraCita); // Compara fecha y hora exacta
                    con.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count == 0; // Retorna true si no hay citas en ese momento (disponible)
                }
            }
        }


        // ===========================================
        // Métodos para la tabla CitaServicios
        // ===========================================

        /// <summary>
        /// Inserta un servicio asociado a una cita.
        /// </summary>
        public static void InsertarCitaServicio(int citaId, int servicioId, int cantidad)
        {
            using (SqlConnection con = new SqlConnection(DbConnection.ConnectionString))
            {
                string query = "INSERT INTO CitaServicios (CitaID, ServicioID, Cantidad) VALUES (@CitaID, @ServicioID, @Cantidad)";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CitaID", citaId);
                    cmd.Parameters.AddWithValue("@ServicioID", servicioId);
                    cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Elimina los servicios asociados a una cita específica.
        /// </summary>
        public static bool EliminarCitaServiciosByCitaID(int citaId)
        {
            using (SqlConnection con = new SqlConnection(DbConnection.ConnectionString))
            {
                string query = "DELETE FROM CitaServicios WHERE CitaID = @CitaID";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CitaID", citaId);
                    con.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        /// <summary>
        /// Obtiene todos los servicios de una cita específica.
        /// </summary>
        public static DataTable GetServiciosByCitaID(int citaId)
        {
            DataTable dtServicios = new DataTable();
            string query = @"
                SELECT
                    S.NombreServicio,
                    CS.Cantidad
                FROM
                    CitaServicios CS
                INNER JOIN
                    Servicios S ON CS.ServicioID = S.ServicioID
                WHERE
                    CS.CitaID = @CitaID;";

            using (SqlConnection con = new SqlConnection(DbConnection.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CitaID", citaId);
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dtServicios);
                }
            }
            return dtServicios;
        }
        // ===========================================
        // Métodos para el Historial de Citas (JOINs)
        // ===========================================

        /// <summary>
        /// Obtiene el historial de citas para un cliente específico.
        /// Incluye información de mascota y empleado.
        /// </summary>
        public static DataTable GetHistorialCitasByClienteID(int clienteId)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(DbConnection.ConnectionString))
            {
                string query = @"
                    SELECT
                        C.CitaID,
                        C.Fecha,
                        M.Nombre AS NombreMascota,
                        E.PrimerNombre + ' ' + E.ApellidoPaterno AS NombreEmpleado,
                        E.EmpleadoID -- Útil si necesitas más detalles del empleado
                    FROM Citas C
                    INNER JOIN Mascotas M ON C.MascotaID = M.MascotaID
                    INNER JOIN Clientes Cl ON M.ClienteID = Cl.ClienteID
                    INNER JOIN Empleados E ON C.EmpleadoID = E.EmpleadoID
                    WHERE Cl.ClienteID = @ClienteID
                    ORDER BY C.Fecha DESC";
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