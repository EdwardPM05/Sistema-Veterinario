﻿using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions; // Necesario para expresiones regulares
using System.Web.UI; // Necesario para ScriptManager
using System.Web.UI.WebControls;
using System.Text; // NECESARIO PARA StringBuilder

namespace VetWeb
{
    public partial class Roles : System.Web.UI.Page
    {
        string cadena = ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarRoles();
                // Estado inicial de los botones del modal (modo agregar)
                btnActualizar.Style["display"] = "none";
                btnAgregar.Style["display"] = "inline-block";
            }
        }

        /// <summary>
        /// Carga los datos de roles desde la base de datos y los enlaza al GridView,
        /// opcionalmente filtrando por nombre de rol.
        /// </summary>
        /// <param name="searchTerm">Término opcional para buscar en NombreRol.</param>
        private void CargarRoles(string searchTerm = null)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "SELECT RolID, NombreRol FROM Roles";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += " WHERE NombreRol LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY NombreRol"; // Ordenar para una visualización consistente

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvRoles.DataSource = dt;
                gvRoles.DataBind();
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Agregar" en el modal.
        /// Agrega un nuevo registro de rol a la base de datos.
        /// </summary>
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "INSERT INTO Roles (NombreRol) VALUES (@NombreRol)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@NombreRol", txtNombreRol.Text.Trim());

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    MostrarMensaje("Rol agregado correctamente.", true);
                    successOperation = true;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE (si NombreRol es único)
                    {
                        MostrarMensaje("Error: Ya existe un rol con el mismo nombre.", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al agregar el rol: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al agregar el rol: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarRoles();
                // El modal se ocultará en MostrarMensaje()
            }
            else
            {
                // Si hay un error, el modal permanece abierto y los datos se conservan.
                CargarRoles(txtBuscarNombreRol.Text.Trim()); // Refrescar el grid con el filtro actual
                // El modal se mantendrá abierto por MostrarMensaje()
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Actualizar" en el modal.
        /// Actualiza un registro de rol existente en la base de datos.
        /// </summary>
        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            int rolID;
            if (!int.TryParse(hfRolID.Value, out rolID))
            {
                MostrarMensaje("Error: El ID del rol no tiene un formato válido para actualizar.", false);
                return;
            }

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "UPDATE Roles SET NombreRol=@NombreRol WHERE RolID=@RolID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@NombreRol", txtNombreRol.Text.Trim());
                cmd.Parameters.AddWithValue("@RolID", rolID);

                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MostrarMensaje("Rol actualizado correctamente.", true);
                        successOperation = true;
                    }
                    else
                    {
                        MostrarMensaje("No se encontró el rol para actualizar o no hubo cambios.", false);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE (si NombreRol es único)
                    {
                        MostrarMensaje("Error: Ya existe un rol con el mismo nombre. Por favor, ingrese un nombre único.", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error al actualizar el rol: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al actualizar el rol: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarRoles();
                // El modal se ocultará en MostrarMensaje()
            }
            else
            {
                // Si hay un error, el modal permanece abierto y los datos se conservan.
                CargarRoles(txtBuscarNombreRol.Text.Trim()); // Refrescar el grid con el filtro actual
                // El modal se mantendrá abierto por MostrarMensaje()
            }
        }

        /// <summary>
        /// Maneja los comandos de fila del GridView (Editar, Eliminar).
        /// </summary>
        protected void gvRoles_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Asegurarse de que el CommandArgument es un índice válido
            int index;
            if (!int.TryParse(e.CommandArgument.ToString(), out index))
            {
                MostrarMensaje("Error: El comando no proporcionó un índice de fila válido.", false);
                return;
            }

            // Validar el índice y obtener el RolID de DataKeys
            if (gvRoles.DataKeys == null || index < 0 || index >= gvRoles.DataKeys.Count)
            {
                MostrarMensaje("Error interno: No se pudo obtener el ID del rol de la fila seleccionada. Recargue la página.", false);
                return;
            }
            int rolID = Convert.ToInt32(gvRoles.DataKeys[index].Value);


            if (e.CommandName == "Editar")
            {
                string nombreRol = "";
                using (SqlConnection con = new SqlConnection(cadena))
                {
                    // ***** CAMBIO CLAVE: Cargar el NombreRol desde la DB usando el RolID *****
                    string query = "SELECT NombreRol FROM Roles WHERE RolID = @RolID";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@RolID", rolID);
                    try
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            nombreRol = result.ToString();
                        }
                        else
                        {
                            // Esto podría pasar si el RolID ya no existe en la DB
                            MostrarMensaje("El rol no se encontró en la base de datos.", false);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        MostrarMensaje("Error al cargar datos del rol para edición: " + ex.Message, false);
                        return;
                    }
                }

                txtNombreRol.Text = nombreRol; // <-- Ahora 'nombreRol' viene de la DB por el ID
                hfRolID.Value = rolID.ToString();

                btnAgregar.Style["display"] = "none";
                btnActualizar.Style["display"] = "inline-block";

                ScriptManager.RegisterStartupScript(this, this.GetType(), "SetRolModalTitle", "document.getElementById('rolModalLabel').innerText = 'Editar Rol';", true);
                MostrarMensaje("", false); // Limpia mensajes
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowRolModalScript", "showRolModal();", true); // Mostrar modal
            }
            else if (e.CommandName == "Eliminar")
            {
                // El código de eliminación que tienes ya usa el rolID de DataKeys, lo cual es correcto.
                // No necesita cambios aquí, solo asegúrate de que el 'rolID' que llega sea el correcto.
                // El resto de tu lógica de eliminación es correcta y robusta con la transacción y validación de dependencias.

                using (SqlConnection con = new SqlConnection(cadena))
                {
                    con.Open();
                    SqlTransaction transaction = con.BeginTransaction();

                    try
                    {
                        SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Empleados WHERE RolID = @RolID", con, transaction);
                        checkCmd.Parameters.AddWithValue("@RolID", rolID);
                        int dependentEmpleados = (int)checkCmd.ExecuteScalar();

                        if (dependentEmpleados > 0)
                        {
                            MostrarMensaje("No se puede eliminar este rol porque hay " + dependentEmpleados + " empleado(s) asociado(s) a él. Elimine o reasigne los empleados primero.", false);
                            transaction.Rollback();
                            return;
                        }

                        SqlCommand cmd = new SqlCommand("DELETE FROM Roles WHERE RolID = @RolID", con, transaction);
                        cmd.Parameters.AddWithValue("@RolID", rolID);
                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                        MostrarMensaje("Rol eliminado correctamente.", true);
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        if (ex.Number == 547)
                        {
                            MostrarMensaje("No se pudo eliminar el rol porque tiene registros asociados (ej. empleados). Elimine los registros asociados primero.", false);
                        }
                        else
                        {
                            MostrarMensaje("Error de base de datos al eliminar rol: " + ex.Message, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        MostrarMensaje("Ocurrió un error inesperado al eliminar el rol: " + ex.Message, false);
                    }
                    finally
                    {
                        con.Close();
                    }
                }
                CargarRoles();
            }
        }

        /// <summary>
        /// Limpia los campos del formulario del modal y restablece la UI al modo "Agregar".
        /// </summary>
        private void LimpiarFormulario()
        {
            txtNombreRol.Text = "";
            hfRolID.Value = "";
            btnAgregar.Style["display"] = "inline-block";
            btnActualizar.Style["display"] = "none";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetRolModalTitle", "document.getElementById('rolModalLabel').innerText = 'Agregar Nuevo Rol';", true);
            MostrarMensaje("", false); // Limpiar mensajes al limpiar formulario
        }

        /// <summary>
        /// Valida los campos de entrada del formulario de Rol.
        /// </summary>
        /// <returns>True si el formulario es válido, false en caso contrario.</returns>
        private bool ValidarFormulario()
        {
            // Validar que el campo no esté vacío
            if (string.IsNullOrWhiteSpace(txtNombreRol.Text))
            {
                MostrarMensaje("Por favor, ingrese el nombre del rol.", false);
                return false;
            }

            // Validar NombreRol (solo letras y espacios, incluyendo tildes y 'ñ')
            // Asegúrate de tener 'using System.Text.RegularExpressions;' al inicio del archivo.
            if (!Regex.IsMatch(txtNombreRol.Text.Trim(), @"^[\p{L}\s]+$"))
            {
                MostrarMensaje("El campo 'Nombre de Rol' solo puede contener letras.", false);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Muestra un mensaje al usuario, ya sea de éxito o error, utilizando estilos de alerta de Bootstrap.
        /// Controla la visibilidad del modal para mantenerlo abierto en caso de error.
        /// </summary>
        /// <param name="mensaje">El mensaje a mostrar.</param>
        /// <param name="exito">True para mensaje de éxito (alerta verde), false para error (alerta roja).</param>
        private void MostrarMensaje(string mensaje, bool exito)
        {
            if (string.IsNullOrEmpty(mensaje))
            {
                lblMensaje.Text = "";
                lblMensaje.CssClass = "";
            }
            else
            {
                lblMensaje.Text = mensaje;
                lblMensaje.CssClass = exito ? "alert alert-success" : "alert alert-danger";

                // Controlar la visibilidad del modal
                if (!exito) // Si es un mensaje de error
                {
                    // Volver a abrir el modal para mostrar el error
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowRolModalOnError", "showRolModal();", true);
                }
                else // Si es un mensaje de éxito
                {
                    // Ocultar el modal en caso de éxito
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideRolModalOnSuccess", "hideRolModal();", true);
                }
            }
            // Asegurar que el mensaje sea visible dentro del modal
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollToMessageRol", "var modalBody = document.querySelector('#rolModal .modal-body'); if(modalBody) modalBody.scrollTop = 0;", true);
        }



        protected void btnBuscarRol_Click(object sender, EventArgs e)
        {
            CargarRoles(txtBuscarNombreRol.Text.Trim());
        }

        /// <summary>
        /// Maneja el evento de clic del botón de limpiar búsqueda.
        /// Limpia el término de búsqueda y recarga la lista completa de roles.
        /// </summary>
        protected void btnLimpiarBusquedaRol_Click(object sender, EventArgs e)
        {
            txtBuscarNombreRol.Text = ""; // Limpiar el textbox de búsqueda
            CargarRoles(); // Recargar todos los roles sin filtro
        }

        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            DataTable dtRoles = new DataTable();
            // Utiliza txtBuscarNombreRol para el filtro de búsqueda de roles
            string filtroAplicado = string.IsNullOrEmpty(txtBuscarNombreRol.Text.Trim()) ? "Ninguno" : txtBuscarNombreRol.Text.Trim();

            using (SqlConnection con = new SqlConnection(cadena))
            {
                // Obtener los datos de los roles (aplicando el filtro de búsqueda actual si lo hay)
                string query = "SELECT NombreRol FROM Roles";

                if (!string.IsNullOrEmpty(txtBuscarNombreRol.Text.Trim()))
                {
                    query += " WHERE NombreRol LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY NombreRol"; // Ordenar para una visualización consistente

                SqlCommand cmd = new SqlCommand(query, con);
                if (!string.IsNullOrEmpty(txtBuscarNombreRol.Text.Trim()))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", txtBuscarNombreRol.Text.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                try
                {
                    con.Open();
                    da.Fill(dtRoles);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al cargar los datos para el Excel: " + ex.Message, false);
                    return;
                }
            }

            if (dtRoles.Rows.Count == 0)
            {
                MostrarMensaje("No hay datos de roles para generar el Excel con el filtro actual.", false);
                return;
            }

            try
            {
                // Configurar la respuesta para descargar un archivo Excel
                Response.Clear();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.ms-excel"; // MIME type para Excel 97-2003
                // Nombre del archivo Excel: ReporteRoles_FechaHoraActual.xls
                Response.AddHeader("Content-Disposition", "attachment;filename=ReporteRoles_" + ".xls");
                Response.Charset = "UTF-8";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble()); // Para UTF-8 con BOM

                // Usar StringBuilder para construir el contenido HTML de la tabla
                StringBuilder sb = new StringBuilder();

                // Cabecera HTML para Excel (opcional pero recomendable para una mejor compatibilidad)
                sb.Append("<html xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");
                sb.Append("<head><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet>");
                sb.Append("<x:Name>Roles</x:Name>"); // Nombre de la hoja en Excel
                sb.Append("<x:WorksheetOptions><x:Panes></x:Panes></x:WorksheetOptions>");
                sb.Append("</x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml></head>");
                sb.Append("<body>");

                // Título del reporte en el Excel
                // Colspan será 1 porque solo hay una columna de datos (NombreRol)
                sb.Append("<table border='0' style='font-family: Arial; font-size: 14pt;'><tr><td colspan='1' align='center'><b>REPORTE DE ROLES</b></td></tr></table>");
                sb.Append("<table border='0' style='font-family: Arial; font-size: 10pt;'><tr><td colspan='1' align='left'>Fecha de Generación: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "</td></tr>");
                sb.Append("<tr><td colspan='1' align='left'>Filtro Aplicado: \"" + filtroAplicado + "\"</td></tr></table>");
                sb.Append("<br>"); // Salto de línea para separar el encabezado de la tabla de datos

                // Crear la tabla HTML para los datos de los roles
                sb.Append("<table border='1px' cellpadding='0' cellspacing='0' style='border-collapse: collapse; font-family: Arial; font-size: 10pt;'>");

                // Añadir fila de encabezados para la tabla de datos
                sb.Append("<tr style='background-color:#36506A; color:#FFFFFF;'>");
                sb.Append("<th>Nombre del Rol</th>"); // Solo un encabezado: Nombre del Rol
                sb.Append("</tr>");

                // Añadir filas de datos
                foreach (DataRow row in dtRoles.Rows)
                {
                    sb.Append("<tr>");
                    sb.Append("<td>" + Server.HtmlEncode(row["NombreRol"].ToString()) + "</td>"); // Solo una columna de datos
                    sb.Append("</tr>");
                }

                sb.Append("</table>");


                sb.Append("</body></html>");

                // Escribir el contenido en el flujo de respuesta
                Response.Write(sb.ToString());
                Response.Flush();
                Response.End();

            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al generar el archivo Excel: " + ex.Message, false);
            }
        }
    }
}