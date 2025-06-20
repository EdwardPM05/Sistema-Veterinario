using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization; // Necesario para CultureInfo
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace VetWeb
{
    public partial class CitaServicios : System.Web.UI.Page
    {
        string cadena = ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarCitas();
                CargarServicios();
                // Limpiar el formulario y el mensaje al cargar la página por primera vez
                LimpiarFormulario();
                // Ocultar el GridView hasta que se seleccione una cita
                gvCitaServicios.Visible = false;
                lblTotalCita.Visible = false;
                lblInfoCitaSeleccionada.Visible = false;
            }
        }

        /// <summary>
        /// Carga el DropDownList de Citas con información de Fecha, Cliente y Mascota.
        /// </summary>
        private void CargarCitas()
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"
                    SELECT
                        C.CitaID,
                        C.Fecha,
                        Cl.PrimerNombre + ' ' + Cl.ApellidoPaterno AS NombreCliente,
                        M.Nombre AS NombreMascota
                    FROM Citas C
                    INNER JOIN Mascotas M ON C.MascotaID = M.MascotaID
                    INNER JOIN Clientes Cl ON M.ClienteID = Cl.ClienteID
                    ORDER BY C.Fecha DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlCitas.DataSource = dt;
                // Formato de texto para mostrar la cita: "Fecha - Cliente - Mascota"
                ddlCitas.DataTextField = "CitaInfo"; // Creamos una columna calculada en el DataTable
                ddlCitas.DataValueField = "CitaID";

                // Crear la columna combinada para el DataTextField
                dt.Columns.Add("CitaInfo", typeof(string));
                foreach (DataRow row in dt.Rows)
                {
                    row["CitaInfo"] = $"{Convert.ToDateTime(row["Fecha"]).ToString("dd/MM/yyyy")} - {row["NombreCliente"]} - {row["NombreMascota"]}";
                }

                ddlCitas.DataBind();
                ddlCitas.Items.Insert(0, new ListItem("Seleccione una Cita", ""));
            }
        }

        /// <summary>
        /// Carga el DropDownList de Servicios desde la base de datos.
        /// </summary>
        private void CargarServicios()
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT ServicioID, NombreServicio, Precio FROM Servicios ORDER BY NombreServicio", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlServicios.DataSource = dt;
                ddlServicios.DataTextField = "NombreServicio";
                ddlServicios.DataValueField = "ServicioID";
                ddlServicios.DataBind();
                ddlServicios.Items.Insert(0, new ListItem("Seleccione un Servicio", ""));
            }
        }

        /// <summary>
        /// Maneja el cambio de selección en el DropDownList de Citas.
        /// Carga los servicios asociados a la cita seleccionada y recalcula el total.
        /// </summary>
        protected void ddlCitas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlCitas.SelectedValue))
            {
                int selectedCitaID = Convert.ToInt32(ddlCitas.SelectedValue);
                hfSelectedCitaID.Value = selectedCitaID.ToString(); // Guardar ID de cita seleccionada
                CargarCitaServicios(selectedCitaID);
                CalcularTotalCita(selectedCitaID);

                // Mostrar info de la cita seleccionada
                lblInfoCitaSeleccionada.Text = ddlCitas.SelectedItem.Text;
                gvCitaServicios.Visible = true;
                lblTotalCita.Visible = true;
                lblInfoCitaSeleccionada.Visible = true;
                LimpiarFormulario(); // Resetear el formulario de agregar/editar servicio
            }
            else
            {
                // Limpiar y ocultar si no hay cita seleccionada
                gvCitaServicios.DataSource = null;
                gvCitaServicios.DataBind();
                gvCitaServicios.Visible = false;
                lblTotalCita.Text = "Total de la Cita: S/ 0.00";
                lblTotalCita.Visible = false;
                lblInfoCitaSeleccionada.Text = "";
                lblInfoCitaSeleccionada.Visible = false;
                hfSelectedCitaID.Value = "";
                LimpiarFormulario();
            }
            // Asegurarse de que el modal esté oculto si se activó un postback que no es del modal (ej. cambio de cita)
            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModalAfterCitaChange", "hideCitaServicioModal();", true);
        }

        /// <summary>
        /// Maneja el cambio de selección en el DropDownList de Servicios.
        /// Actualiza el precio unitario actual y el subtotal en el modal.
        /// </summary>
        protected void ddlServicios_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalcularValoresModal();
            // Asegurarse de que el modal permanezca abierto si se activa desde allí
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCitaServicioModalOnDdlChange", "showCitaServicioModal();", true);
        }

        /// <summary>
        /// Maneja el cambio de texto en el campo Cantidad.
        /// Recalcula el subtotal en el modal.
        /// </summary>
        protected void txtCantidad_TextChanged(object sender, EventArgs e)
        {
            CalcularValoresModal();
            // Asegurarse de que el modal permanezca abierto si se activa desde allí
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCitaServicioModalOnTxtChange", "showCitaServicioModal();", true);
        }

        /// <summary>
        /// Calcula y muestra el precio unitario actual y el subtotal dentro del modal.
        /// Esto es importante para que el usuario vea el precio "congelado" y el subtotal al momento de agregar/editar.
        /// </summary>
        private void CalcularValoresModal()
        {
            decimal precioUnitarioActual = 0;
            int cantidad = 0;

            // Obtener precio actual del servicio seleccionado (desde la base de datos o un cache)
            if (!string.IsNullOrEmpty(ddlServicios.SelectedValue))
            {
                int servicioID = Convert.ToInt32(ddlServicios.SelectedValue);
                using (SqlConnection con = new SqlConnection(cadena))
                {
                    SqlCommand cmd = new SqlCommand("SELECT Precio FROM Servicios WHERE ServicioID = @ServicioID", con);
                    cmd.Parameters.AddWithValue("@ServicioID", servicioID);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        precioUnitarioActual = Convert.ToDecimal(result);
                    }
                    con.Close();
                }
            }

            // Obtener cantidad del textbox
            if (!string.IsNullOrWhiteSpace(txtCantidad.Text) && int.TryParse(txtCantidad.Text, out cantidad))
            {
                // Cantidad es válida
            }
            else
            {
                cantidad = 0; // Si no es un número válido, tratar como 0
            }

            // Crear un objeto CultureInfo para Soles Peruanos
            CultureInfo culturePE = new CultureInfo("es-PE");

            // Mostrar el precio unitario actual (del servicio master)
            lblPrecioUnitarioActual.Text = precioUnitarioActual.ToString("C", culturePE); // Formato moneda con cultura es-PE

            // Calcular y mostrar el subtotal (Cantidad * Precio Unitario Actual)
            decimal subtotal = precioUnitarioActual * cantidad;
            lblSubtotalServicio.Text = subtotal.ToString("C", culturePE); // Formato moneda con cultura es-PE

            // Almacenar el precio unitario actual en el HiddenField para ser guardado en CitaServicios.PrecioUnitario
            hfPrecioUnitarioGuardado.Value = precioUnitarioActual.ToString(CultureInfo.InvariantCulture); // Guardar sin símbolo de moneda para el HiddenField
        }


        /// <summary>
        /// Carga los servicios asociados a una cita específica y los enlaza al GridView.
        /// </summary>
        /// <param name="citaID">El ID de la cita para la cual cargar los servicios.</param>
        private void CargarCitaServicios(int citaID)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"
                    SELECT 
                        CS.CitaServicioID, 
                        CS.CitaID, 
                        CS.ServicioID, 
                        S.NombreServicio, 
                        CS.Cantidad, 
                        CS.PrecioUnitario, 
                        (CS.Cantidad * CS.PrecioUnitario) AS TotalServicio 
                    FROM CitaServicios CS
                    INNER JOIN Servicios S ON CS.ServicioID = S.ServicioID
                    WHERE CS.CitaID = @CitaID
                    ORDER BY S.NombreServicio";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CitaID", citaID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvCitaServicios.DataSource = dt;
                gvCitaServicios.DataBind();
            }
        }

        /// <summary>
        /// Calcula el total de todos los servicios asociados a una cita y lo muestra en el label.
        /// </summary>
        /// <param name="citaID">El ID de la cita para la cual calcular el total.</param>
        private void CalcularTotalCita(int citaID)
        {
            decimal totalCita = 0;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "SELECT SUM(Cantidad * PrecioUnitario) FROM CitaServicios WHERE CitaID = @CitaID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CitaID", citaID);
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    totalCita = Convert.ToDecimal(result);
                }
                con.Close();
            }
            // Crear un objeto CultureInfo para Soles Peruanos
            CultureInfo culturePE = new CultureInfo("es-PE");
            lblTotalCita.Text = $"Total de la Cita: {totalCita.ToString("C", culturePE)}"; // Formato moneda con cultura es-PE
        }


        /// <summary>
        /// Maneja el evento de clic del botón "Agregar Servicio" en el modal.
        /// Agrega un nuevo registro de CitaServicio a la base de datos.
        /// </summary>
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            // Asegurarse de que hay una cita seleccionada antes de agregar servicios
            if (string.IsNullOrEmpty(hfSelectedCitaID.Value))
            {
                MostrarMensaje("Por favor, seleccione una Cita antes de agregar servicios.", false);
                return;
            }

            int citaID = Convert.ToInt32(hfSelectedCitaID.Value);
            int servicioID = Convert.ToInt32(ddlServicios.SelectedValue);
            int cantidad = Convert.ToInt32(txtCantidad.Text.Trim());
            decimal precioUnitario = Convert.ToDecimal(hfPrecioUnitarioGuardado.Value, CultureInfo.InvariantCulture); // Usar el precio capturado en el modal

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "INSERT INTO CitaServicios (CitaID, ServicioID, Cantidad, PrecioUnitario) VALUES (@CitaID, @ServicioID, @Cantidad, @PrecioUnitario)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CitaID", citaID);
                cmd.Parameters.AddWithValue("@ServicioID", servicioID);
                cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                cmd.Parameters.AddWithValue("@PrecioUnitario", precioUnitario); // Guardamos el precio en el momento de la adición

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    MostrarMensaje("Servicio agregado a la cita correctamente.", true);
                    successOperation = true;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE (si la hubiera, por ejemplo, ServicioID ya existe para esa CitaID)
                    {
                        MostrarMensaje("Error: Este servicio ya ha sido agregado a esta cita. Considere actualizar la cantidad.", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al agregar el servicio a la cita: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al agregar el servicio a la cita: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarCitaServicios(citaID); // Recargar solo los servicios de la cita actual
                CalcularTotalCita(citaID);
            }
            else
            {
                // Si hay un error, el modal permanece abierto y los datos se conservan.
                // Asegurarse de que el modal permanezca abierto
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCitaServicioModalOnError", "showCitaServicioModal();", true);
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Actualizar Servicio" en el modal.
        /// Actualiza un registro de CitaServicio existente en la base de datos.
        /// </summary>
        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            int citaServicioID;
            if (!int.TryParse(hfCitaServicioID.Value, out citaServicioID))
            {
                MostrarMensaje("Error: El ID del servicio de cita no tiene un formato válido para actualizar. Por favor, intente editar de nuevo.", false);
                return;
            }

            // Asegurarse de que hay una cita seleccionada
            if (string.IsNullOrEmpty(hfSelectedCitaID.Value))
            {
                MostrarMensaje("Error interno: No se ha seleccionado una cita válida.", false);
                return;
            }

            int citaID = Convert.ToInt32(hfSelectedCitaID.Value);
            int servicioID = Convert.ToInt32(ddlServicios.SelectedValue);
            int cantidad = Convert.ToInt32(txtCantidad.Text.Trim());
            // Para la actualización, el PrecioUnitario DEBE ser el que ya está guardado para ese CitaServicioID,
            // no el precio actual del servicio master, para mantener la integridad histórica.
            decimal precioUnitarioExistente = Convert.ToDecimal(hfPrecioUnitarioGuardado.Value, CultureInfo.InvariantCulture);

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "UPDATE CitaServicios SET ServicioID=@ServicioID, Cantidad=@Cantidad, PrecioUnitario=@PrecioUnitario WHERE CitaServicioID=@CitaServicioID AND CitaID=@CitaID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ServicioID", servicioID);
                cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                cmd.Parameters.AddWithValue("@PrecioUnitario", precioUnitarioExistente); // Usar el precio ya guardado
                cmd.Parameters.AddWithValue("@CitaServicioID", citaServicioID);
                cmd.Parameters.AddWithValue("@CitaID", citaID); // Asegura que solo se actualice para la cita correcta

                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MostrarMensaje("Servicio de cita actualizado correctamente.", true);
                        successOperation = true;
                    }
                    else
                    {
                        MostrarMensaje("No se encontró el servicio de cita para actualizar o no hubo cambios.", false);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE (si la hubiera)
                    {
                        MostrarMensaje("Error: Ya existe este servicio con la misma cantidad para esta cita. Considere modificar la cantidad existente.", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al actualizar el servicio de cita: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al actualizar el servicio de cita: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarCitaServicios(citaID);
                CalcularTotalCita(citaID);
            }
            else
            {
                // Si hay un error, el modal permanece abierto y los datos se conservan.
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCitaServicioModalOnErrorUpdate", "showCitaServicioModal();", true);
            }
        }

        /// <summary>
        /// Maneja los comandos de fila del GridView (Editar, Eliminar).
        /// </summary>
        protected void gvCitaServicios_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);

            // Obtener IDs usando DataKeys para robustez
            if (gvCitaServicios.DataKeys == null || index < 0 || index >= gvCitaServicios.DataKeys.Count)
            {
                MostrarMensaje("Error interno: No se pudo obtener el ID del servicio de cita. Por favor, recargue la página.", false);
                return;
            }
            int citaServicioID = Convert.ToInt32(gvCitaServicios.DataKeys[index]["CitaServicioID"]);
            int citaID = Convert.ToInt32(gvCitaServicios.DataKeys[index]["CitaID"]);
            int servicioID = Convert.ToInt32(gvCitaServicios.DataKeys[index]["ServicioID"]);

            GridViewRow row = gvCitaServicios.Rows[index];

            if (e.CommandName == "Editar")
            {
                // Asegurarse de que la cita seleccionada en el ddlCitas es la misma que la del servicio a editar
                if (Convert.ToInt32(ddlCitas.SelectedValue) != citaID)
                {
                    // Esto no debería ocurrir si la UI está bien controlada, pero como precaución
                    MostrarMensaje("Error: No puede editar un servicio de una cita diferente a la seleccionada.", false);
                    return;
                }

                // Cargar los datos en el modal
                try
                {
                    ddlServicios.SelectedValue = servicioID.ToString();
                    txtCantidad.Text = row.Cells[1].Text.Trim(); // Cantidad
                    // Al leer el precio unitario del GridView, asegúrate de que el formato de moneda sea removido
                    // antes de convertir a decimal para evitar errores de formato, usando InvariantCulture.
                    decimal parsedPrecioUnitario = decimal.Parse(row.Cells[2].Text.Replace("S/", "").Replace("$", "").Trim(), NumberStyles.Currency, CultureInfo.InvariantCulture);
                    hfPrecioUnitarioGuardado.Value = parsedPrecioUnitario.ToString(CultureInfo.InvariantCulture); // Guardar el valor numérico en el HiddenField

                    CalcularValoresModal(); // Recalcular el subtotal en el modal basándose en los datos cargados

                    hfCitaServicioID.Value = citaServicioID.ToString();

                    // Cambiar visibilidad de botones y título del modal
                    btnAgregar.Style["display"] = "none";
                    btnActualizar.Style["display"] = "inline-block";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "SetCitaServicioModalTitle", "document.getElementById('citaServicioModalLabel').innerText = 'Editar Servicio de Cita';", true);

                    MostrarMensaje("", false); // Limpia mensajes previos
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCitaServicioModalScript", "showCitaServicioModal();", true);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al cargar los datos del servicio de cita para edición: " + ex.Message, false);
                }
            }
            else if (e.CommandName == "Eliminar")
            {
                using (SqlConnection con = new SqlConnection(cadena))
                {
                    con.Open();
                    SqlTransaction transaction = con.BeginTransaction(); // Iniciar transacción

                    try
                    {
                        // No hay dependencias directas de CitaServicios, así que la eliminación es directa.
                        SqlCommand cmd = new SqlCommand("DELETE FROM CitaServicios WHERE CitaServicioID = @CitaServicioID", con, transaction);
                        cmd.Parameters.AddWithValue("@CitaServicioID", citaServicioID);
                        cmd.ExecuteNonQuery();

                        transaction.Commit(); // Confirmar la transacción
                        MostrarMensaje("Servicio de cita eliminado correctamente.", true);
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback(); // Revertir en caso de error
                        MostrarMensaje("Error de base de datos al eliminar servicio de cita: " + ex.Message, false);
                    }
                    catch (Exception ex)
                    {
                        MostrarMensaje("Ocurrió un error inesperado al eliminar el servicio de cita: " + ex.Message, false);
                    }
                    finally
                    {
                        con.Close();
                    }
                }
                // Después de eliminar, recargar los servicios de la misma cita y recalcular el total
                CargarCitaServicios(citaID);
                CalcularTotalCita(citaID);
            }
        }

        /// <summary>
        /// Limpia los campos del formulario del modal y restablece la UI al modo "Agregar".
        /// </summary>
        private void LimpiarFormulario()
        {
            ddlServicios.ClearSelection();
            if (ddlServicios.Items.Count > 0) ddlServicios.Items.FindByValue("").Selected = true;
            txtCantidad.Text = "";

            // Crear un objeto CultureInfo para Soles Peruanos
            CultureInfo culturePE = new CultureInfo("es-PE");
            lblPrecioUnitarioActual.Text = (0M).ToString("C", culturePE); // Inicializar en formato de Soles
            lblSubtotalServicio.Text = (0M).ToString("C", culturePE);     // Inicializar en formato de Soles

            hfCitaServicioID.Value = "";
            hfPrecioUnitarioGuardado.Value = ""; // Limpiar también el hidden field del precio guardado

            btnAgregar.Style["display"] = "inline-block";
            btnActualizar.Style["display"] = "none";

            // También restablecer el título del modal y limpiar el mensaje
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetCitaServicioModalTitle", "document.getElementById('citaServicioModalLabel').innerText = 'Agregar Servicio a Cita';", true);
            // Limpiar mensaje explícitamente sin invocar show/hide modal
            lblMensaje.Text = "";
            lblMensaje.CssClass = "";
        }

        /// <summary>
        /// Valida los campos de entrada del formulario del modal de CitaServicio.
        /// </summary>
        /// <returns>True si el formulario es válido, false en caso contrario.</returns>
        private bool ValidarFormulario()
        {
            // Validar que se ha seleccionado un servicio
            if (string.IsNullOrEmpty(ddlServicios.SelectedValue) || ddlServicios.SelectedValue == "")
            {
                MostrarMensaje("Por favor, seleccione un Servicio.", false);
                return false;
            }

            // Validar Cantidad
            int cantidad;
            if (!int.TryParse(txtCantidad.Text.Trim(), out cantidad) || cantidad <= 0)
            {
                MostrarMensaje("La Cantidad debe ser un número entero positivo.", false);
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
            // Si el mensaje es vacío, no aplicar ninguna clase CSS ni texto para que no ocupe espacio.
            if (string.IsNullOrEmpty(mensaje))
            {
                lblMensaje.Text = "";
                lblMensaje.CssClass = "";
            }
            else
            {
                lblMensaje.Text = mensaje;
                lblMensaje.CssClass = exito ? "alert alert-success" : "alert alert-danger";
            }

            // Si es un mensaje de éxito Y NO está vacío, ocultar el modal.
            // En cualquier otro caso (mensaje de error, o mensaje vacío en modo de edición), mostrar el modal.
            if (exito && !string.IsNullOrEmpty(mensaje))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideCitaServicioModalOnSuccess", "hideCitaServicioModal();", true);
            }
            else // Esto cubre mensajes de error y la lógica de "editar" (donde el mensaje está vacío y exito es false)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCitaServicioModalGeneral", "showCitaServicioModal();", true);
            }

            // Asegurar que el mensaje sea visible dentro del modal
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollToMessageCitaServicio" + Guid.NewGuid().ToString(),
                "var modalBody = document.querySelector('#citaServicioModal .modal-body'); if(modalBody) modalBody.scrollTop = 0;", true);
        }
    }
}
