using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text; // Importa iTextSharp
using iTextSharp.text.pdf; // Importa iTextSharp.pdf
using System.IO; // Necesario para MemoryStream
using System.Linq; // Necesario para Enumerable.Repeat
using System.Text;

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
                ddlCitas.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione una Cita", ""));
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
                ddlServicios.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione un Servicio", ""));
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
        protected void btnImprimirPdf_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hfSelectedCitaID.Value))
            {
                MostrarMensaje("Por favor, seleccione una cita para generar el reporte de servicios.", false);
                return;
            }

            int selectedCitaID = Convert.ToInt32(hfSelectedCitaID.Value);
            DataTable dtCitaServicios = new DataTable();
            DataTable dtCitaInfo = new DataTable();
            decimal totalCita = 0;

            // 1. Obtener la información general de la cita
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string queryCitaInfo = @"
                    SELECT
                        C.CitaID,
                        C.Fecha,
                        Cl.PrimerNombre + ' ' + Cl.ApellidoPaterno AS NombreCliente,
                        M.Nombre AS NombreMascota,
                        E.PrimerNombre + ' ' + E.ApellidoPaterno AS NombreEmpleado
                    FROM Citas C
                    INNER JOIN Mascotas M ON C.MascotaID = M.MascotaID
                    INNER JOIN Clientes Cl ON M.ClienteID = Cl.ClienteID
                    INNER JOIN Empleados E ON C.EmpleadoID = E.EmpleadoID
                    WHERE C.CitaID = @CitaID";

                SqlCommand cmdCitaInfo = new SqlCommand(queryCitaInfo, con);
                cmdCitaInfo.Parameters.AddWithValue("@CitaID", selectedCitaID);
                SqlDataAdapter daCitaInfo = new SqlDataAdapter(cmdCitaInfo);

                try
                {
                    con.Open();
                    daCitaInfo.Fill(dtCitaInfo);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al obtener la información de la cita para el PDF: " + ex.Message, false);
                    return;
                }
            }

            if (dtCitaInfo.Rows.Count == 0)
            {
                MostrarMensaje("No se encontró información para la cita seleccionada.", false);
                return;
            }

            DataRow citaRow = dtCitaInfo.Rows[0];
            string fechaCita = Convert.ToDateTime(citaRow["Fecha"]).ToString("dd/MM/yyyy");
            string nombreCliente = citaRow["NombreCliente"].ToString();
            string nombreMascota = citaRow["NombreMascota"].ToString();
            string nombreEmpleado = citaRow["NombreEmpleado"].ToString();

            // 2. Obtener los servicios asociados a la cita y calcular el total
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string queryServicios = @"
                    SELECT
                        S.NombreServicio,
                        CS.Cantidad,
                        CS.PrecioUnitario,
                        (CS.Cantidad * CS.PrecioUnitario) AS Subtotal
                    FROM CitaServicios CS
                    INNER JOIN Servicios S ON CS.ServicioID = S.ServicioID
                    WHERE CS.CitaID = @CitaID
                    ORDER BY S.NombreServicio";

                SqlCommand cmdServicios = new SqlCommand(queryServicios, con);
                cmdServicios.Parameters.AddWithValue("@CitaID", selectedCitaID);
                SqlDataAdapter daServicios = new SqlDataAdapter(cmdServicios);

                try
                {
                    if (con.State != ConnectionState.Open) con.Open(); // Reutilizar conexión si está cerrada
                    daServicios.Fill(dtCitaServicios);

                    // Calcular el total de la cita
                    SqlCommand cmdTotal = new SqlCommand("SELECT SUM(Cantidad * PrecioUnitario) FROM CitaServicios WHERE CitaID = @CitaID", con);
                    cmdTotal.Parameters.AddWithValue("@CitaID", selectedCitaID);
                    object resultTotal = cmdTotal.ExecuteScalar(); // Correcto: Llama ExecuteScalar en el SqlCommand

                    if (resultTotal != null && resultTotal != DBNull.Value)
                    {
                        totalCita = Convert.ToDecimal(resultTotal);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al obtener los servicios de la cita para el PDF: " + ex.Message, false);
                    return;
                }
            }

            // Crear el documento PDF
            Document doc = new Document(PageSize.A4, 30f, 30f, 40f, 30f); // Márgenes

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    // Cultura para formato de moneda (Soles Peruanos)
                    CultureInfo culturePE = new CultureInfo("es-PE");

                    // ====================================================================
                    // 1. ENCABEZADO DEL DOCUMENTO (Logo, Info de la Clínica, Título)
                    // ====================================================================

                    PdfPTable headerTable = new PdfPTable(2);
                    headerTable.WidthPercentage = 100;
                    headerTable.SetWidths(new float[] { 1f, 3f });
                    headerTable.DefaultCell.Border = PdfPCell.NO_BORDER;
                    headerTable.SpacingAfter = 20f;

                    string logoPath = Server.MapPath("~/Assets/Images/logo.png"); // <--- ¡VERIFICA ESTA RUTA!
                    if (File.Exists(logoPath))
                    {
                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                        logo.ScaleToFit(70f, 70f);
                        PdfPCell logoCell = new PdfPCell(logo);
                        logoCell.Border = PdfPCell.NO_BORDER;
                        logoCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        logoCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        logoCell.Padding = 5;
                        headerTable.AddCell(logoCell);
                    }
                    else
                    {
                        headerTable.AddCell(new PdfPCell(new Phrase("Logo no encontrado", FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8, BaseColor.RED))) { Border = PdfPCell.NO_BORDER });
                    }

                    PdfPCell companyInfoCell = new PdfPCell();
                    companyInfoCell.Border = PdfPCell.NO_BORDER;
                    companyInfoCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    companyInfoCell.VerticalAlignment = Element.ALIGN_TOP;
                    companyInfoCell.Padding = 5;

                    Font fontCompanyName = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(54, 80, 106));
                    Font fontCompanyDetails = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);

                    companyInfoCell.AddElement(new Paragraph("VETWEB", fontCompanyName) { Alignment = Element.ALIGN_RIGHT });
                    companyInfoCell.AddElement(new Paragraph("Villa el Salvador, Lima, Perú", fontCompanyDetails) { Alignment = Element.ALIGN_RIGHT });
                    companyInfoCell.AddElement(new Paragraph("Teléfono: +51 907377938", fontCompanyDetails) { Alignment = Element.ALIGN_RIGHT });
                    companyInfoCell.AddElement(new Paragraph("Email: info@vetweb.com", fontCompanyDetails) { Alignment = Element.ALIGN_RIGHT });

                    headerTable.AddCell(companyInfoCell);
                    doc.Add(headerTable);

                    // Título del Reporte
                    Font reportTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, BaseColor.DARK_GRAY);
                    Paragraph reportTitle = new Paragraph("REPORTE COMPLETO DE CITA", reportTitleFont);
                    reportTitle.Alignment = Element.ALIGN_CENTER;
                    reportTitle.SpacingAfter = 15f;
                    doc.Add(reportTitle);

                    // Información del Documento (Folio, Fecha de Generación)
                    PdfPTable docDetailsTable = new PdfPTable(2);
                    docDetailsTable.WidthPercentage = 100;
                    docDetailsTable.DefaultCell.Border = PdfPCell.NO_BORDER;
                    docDetailsTable.SetWidths(new float[] { 1f, 1f });
                    docDetailsTable.SpacingAfter = 10f;

                    Font fontDocDetails = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);

                    docDetailsTable.AddCell(new PdfPCell(new Phrase($"FOLIO: {new Random().Next(100000, 999999)}", fontDocDetails)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT });
                    docDetailsTable.AddCell(new PdfPCell(new Phrase($"Fecha de Generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", fontDocDetails)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

                    // Mostrar la cita ID como parte del filtro/contexto del reporte
                    docDetailsTable.AddCell(new PdfPCell(new Phrase($"Cita Reportada: Cita #{selectedCitaID}", fontDocDetails)) { Colspan = 2, Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT });

                    doc.Add(docDetailsTable);

                    // ====================================================================
                    // 2. DETALLES DE LA CITA SELECCIONADA (Formato más simple, no tabla de 2 columnas)
                    // ====================================================================
                    // Agregamos la información de la cita como un párrafo o bloques de texto
                    Paragraph citaInfoParagraph = new Paragraph();
                    citaInfoParagraph.SetLeading(0f, 1.2f); // Espaciado entre líneas
                    citaInfoParagraph.SpacingAfter = 15f;

                    Font fontCitaDetailsLabel = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK);
                    Font fontCitaDetailsValue = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.DARK_GRAY);

                    citaInfoParagraph.Add(new Chunk("Fecha de Cita: ", fontCitaDetailsLabel));
                    citaInfoParagraph.Add(new Chunk(fechaCita + "\n", fontCitaDetailsValue));

                    citaInfoParagraph.Add(new Chunk("Cliente: ", fontCitaDetailsLabel));
                    citaInfoParagraph.Add(new Chunk(nombreCliente + "\n", fontCitaDetailsValue));

                    citaInfoParagraph.Add(new Chunk("Mascota: ", fontCitaDetailsLabel));
                    citaInfoParagraph.Add(new Chunk(nombreMascota + "\n", fontCitaDetailsValue));

                    citaInfoParagraph.Add(new Chunk("Empleado Asignado: ", fontCitaDetailsLabel));
                    citaInfoParagraph.Add(new Chunk(nombreEmpleado + "\n", fontCitaDetailsValue));

                    doc.Add(citaInfoParagraph);


                    // ====================================================================
                    // 3. TABLA DE SERVICIOS DE LA CITA (Con estilo de Reporte de Clientes)
                    // ====================================================================

                    if (dtCitaServicios.Rows.Count == 0)
                    {
                        Paragraph noServices = new Paragraph("No se registraron servicios para esta cita.", fontCompanyDetails);
                        noServices.Alignment = Element.ALIGN_CENTER;
                        doc.Add(noServices);
                    }
                    else
                    {
                        PdfPTable pdfTable = new PdfPTable(dtCitaServicios.Columns.Count);
                        pdfTable.WidthPercentage = 100;
                        pdfTable.SpacingBefore = 10f;
                        pdfTable.DefaultCell.Padding = 5;
                        pdfTable.HeaderRows = 1;

                        // Las columnas son: NombreServicio, Cantidad, PrecioUnitario, Subtotal
                        float[] widths = new float[] { 3f, 1f, 1.5f, 1.5f };
                        if (dtCitaServicios.Columns.Count == widths.Length)
                        {
                            pdfTable.SetWidths(widths);
                        }
                        else
                        {
                            pdfTable.SetWidths(Enumerable.Repeat(1f, dtCitaServicios.Columns.Count).ToArray());
                        }

                        // Añadir encabezados de columna
                        Font fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);
                        BaseColor headerColor = new BaseColor(54, 80, 106);
                        string[] headers = { "Servicio", "Cantidad", "P. Unitario", "Subtotal" };

                        for (int i = 0; i < dtCitaServicios.Columns.Count; i++)
                        {
                            PdfPCell headerCell = new PdfPCell(new Phrase(headers[i], fontHeader));
                            headerCell.BackgroundColor = headerColor;
                            headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                            headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            headerCell.Padding = 7;
                            headerCell.BorderColor = BaseColor.LIGHT_GRAY;
                            pdfTable.AddCell(headerCell);
                        }

                        // Añadir filas de datos
                        Font fontCell = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                        foreach (DataRow row in dtCitaServicios.Rows)
                        {
                            PdfPCell cellServicio = new PdfPCell(new Phrase(row["NombreServicio"].ToString(), fontCell));
                            cellServicio.HorizontalAlignment = Element.ALIGN_LEFT;

                            PdfPCell cellCantidad = new PdfPCell(new Phrase(row["Cantidad"].ToString(), fontCell));
                            cellCantidad.HorizontalAlignment = Element.ALIGN_CENTER;

                            PdfPCell cellPrecioUnitario = new PdfPCell(new Phrase(Convert.ToDecimal(row["PrecioUnitario"]).ToString("C", culturePE), fontCell));
                            cellPrecioUnitario.HorizontalAlignment = Element.ALIGN_RIGHT;

                            PdfPCell cellSubtotal = new PdfPCell(new Phrase(Convert.ToDecimal(row["Subtotal"]).ToString("C", culturePE), fontCell));
                            cellSubtotal.HorizontalAlignment = Element.ALIGN_RIGHT;

                            // Alternar color de fondo para filas para mejor legibilidad
                            if (dtCitaServicios.Rows.IndexOf(row) % 2 == 0)
                            {
                                BaseColor lightGray = new BaseColor(245, 245, 245); // Gris muy claro para alternancia
                                cellServicio.BackgroundColor = lightGray;
                                cellCantidad.BackgroundColor = lightGray;
                                cellPrecioUnitario.BackgroundColor = lightGray;
                                cellSubtotal.BackgroundColor = lightGray;
                            }

                            cellServicio.Padding = 5;
                            cellCantidad.Padding = 5;
                            cellPrecioUnitario.Padding = 5;
                            cellSubtotal.Padding = 5;

                            cellServicio.BorderColor = BaseColor.LIGHT_GRAY;
                            cellCantidad.BorderColor = BaseColor.LIGHT_GRAY;
                            cellPrecioUnitario.BorderColor = BaseColor.LIGHT_GRAY;
                            cellSubtotal.BorderColor = BaseColor.LIGHT_GRAY;

                            pdfTable.AddCell(cellServicio);
                            pdfTable.AddCell(cellCantidad);
                            pdfTable.AddCell(cellPrecioUnitario);
                            pdfTable.AddCell(cellSubtotal);
                        }

                        doc.Add(pdfTable);

                        // Mostrar el total de la cita
                        PdfPTable totalTable = new PdfPTable(2);
                        totalTable.WidthPercentage = 100;
                        totalTable.DefaultCell.Border = PdfPCell.NO_BORDER;
                        totalTable.SetWidths(new float[] { 4f, 2f });
                        totalTable.SpacingBefore = 10f;

                        Font fontTotalLabel = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                        Font fontTotalValue = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(54, 80, 106));

                        PdfPCell totalLabelCell = new PdfPCell(new Phrase("TOTAL DE LA CITA:", fontTotalLabel));
                        totalLabelCell.Border = PdfPCell.NO_BORDER;
                        totalLabelCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        totalLabelCell.Padding = 5;
                        totalTable.AddCell(totalLabelCell);

                        PdfPCell totalValueCell = new PdfPCell(new Phrase(totalCita.ToString("C", culturePE), fontTotalValue));
                        totalValueCell.Border = PdfPCell.NO_BORDER;
                        totalValueCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        totalValueCell.Padding = 5;
                        totalTable.AddCell(totalValueCell);

                        doc.Add(totalTable);
                    }

                    // ====================================================================
                    // 4. PIE DE PÁGINA DEL DOCUMENTO
                    // ====================================================================
                    Font fontFooter = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9, BaseColor.GRAY);
                    Paragraph footerNote = new Paragraph($"Este es un reporte de servicios generado para la cita #{selectedCitaID} de VetWeb.", fontFooter);
                    footerNote.Alignment = Element.ALIGN_CENTER;
                    footerNote.SpacingBefore = 20f;
                    doc.Add(footerNote);

                    Paragraph thankYouNote = new Paragraph("Generado por VetWeb - Tu solución para la gestión veterinaria.", FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.LIGHT_GRAY));
                    thankYouNote.Alignment = Element.ALIGN_CENTER;
                    doc.Add(thankYouNote);

                    doc.Close();

                    // Enviar el PDF al navegador
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", $"attachment;filename=ReporteServiciosCita_#{selectedCitaID}.pdf");
                    Response.Buffer = true;
                    Response.Clear();
                    Response.BinaryWrite(ms.ToArray());
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al generar el PDF de servicios de la cita: " + ex.Message, false);
            }
            finally
            {
                if (doc.IsOpen())
                {
                    doc.Close();
                }
            }
        }

        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            // Check if a Cita (appointment) is selected
            if (string.IsNullOrEmpty(hfSelectedCitaID.Value))
            {
                MostrarMensaje("Por favor, seleccione una cita para exportar sus servicios a Excel.", false);
                return;
            }

            int selectedCitaID;
            if (!int.TryParse(hfSelectedCitaID.Value, out selectedCitaID))
            {
                MostrarMensaje("Error: ID de cita no válido para exportar a Excel.", false);
                return;
            }

            DataTable dtCitaServicios = new DataTable();
            string infoCitaSeleccionada = ddlCitas.SelectedItem.Text; // Get full info from dropdown

            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"
                    SELECT
                        S.NombreServicio AS 'Servicio',
                        CS.Cantidad AS 'Cantidad',
                        CS.PrecioUnitario AS 'Precio Unitario',
                        (CS.Cantidad * CS.PrecioUnitario) AS 'Subtotal'
                    FROM CitaServicios CS
                    INNER JOIN Servicios S ON CS.ServicioID = S.ServicioID
                    WHERE CS.CitaID = @CitaID
                    ORDER BY S.NombreServicio";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CitaID", selectedCitaID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                try
                {
                    con.Open();
                    da.Fill(dtCitaServicios);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al cargar los datos de servicios de la cita para el Excel: " + ex.Message, false);
                    return;
                }
            }

            if (dtCitaServicios.Rows.Count == 0)
            {
                MostrarMensaje("No hay servicios asociados a la cita seleccionada para generar el Excel.", false);
                return;
            }

            try
            {
                // Calculate the total for the selected appointment
                decimal totalCita = 0;
                foreach (DataRow row in dtCitaServicios.Rows)
                {
                    totalCita += Convert.ToDecimal(row["Subtotal"]);
                }
                CultureInfo culturePE = new CultureInfo("es-PE"); // For Peruvian Soles currency format

                Response.Clear();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.ms-excel";
                Response.AddHeader("Content-Disposition", "attachment;filename=ReporteServiciosCita_" + ".xls");
                Response.Charset = "UTF-8";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());

                StringBuilder sb = new StringBuilder();

                // HTML header for Excel compatibility
                sb.Append("<html xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");
                sb.Append("<head><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet>");
                sb.Append("<x:Name>ServiciosCita</x:Name>");
                sb.Append("<x:WorksheetOptions><x:Panes></x:Panes></x:WorksheetOptions>");
                sb.Append("</x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml></head>");
                sb.Append("<body>");

                // Report Title and Info
                sb.Append("<table border='0' style='font-family: Arial; font-size: 14pt;'><tr><td colspan='4' align='center'><b>REPORTE DE SERVICIOS POR CITA</b></td></tr></table>");
                sb.Append("<table border='0' style='font-family: Arial; font-size: 10pt;'>");
                sb.Append("<tr><td colspan='4' align='left'>Fecha de Generación: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "</td></tr>");
                sb.Append("<tr><td colspan='4' align='left'>Cita Seleccionada: " + Server.HtmlEncode(infoCitaSeleccionada) + "</td></tr>");
                sb.Append("</table>");
                sb.Append("<br>");

                // Data Table
                sb.Append("<table border='1px' cellpadding='0' cellspacing='0' style='border-collapse: collapse; font-family: Arial; font-size: 10pt;'>");

                // Add header row
                sb.Append("<tr style='background-color:#36506A; color:#FFFFFF;'>");
                foreach (DataColumn column in dtCitaServicios.Columns)
                {
                    sb.Append("<th>" + Server.HtmlEncode(column.ColumnName) + "</th>");
                }
                sb.Append("</tr>");

                // Add data rows
                foreach (DataRow row in dtCitaServicios.Rows)
                {
                    sb.Append("<tr>");
                    foreach (DataColumn column in dtCitaServicios.Columns)
                    {
                        // Apply currency formatting to numeric columns
                        if (column.ColumnName == "Precio Unitario" || column.ColumnName == "Subtotal")
                        {
                            decimal value = Convert.ToDecimal(row[column]);
                            sb.Append("<td>" + value.ToString("C", culturePE) + "</td>");
                        }
                        else
                        {
                            sb.Append("<td>" + Server.HtmlEncode(row[column].ToString()) + "</td>");
                        }
                    }
                    sb.Append("</tr>");
                }

                // Add Total row
                sb.Append("<tr>");
                sb.Append("<td colspan='3' align='right'><b>Total General:</b></td>");
                sb.Append("<td><b>" + totalCita.ToString("C", culturePE) + "</b></td>");
                sb.Append("</tr>");

                sb.Append("</table>");
                sb.Append("</body></html>");

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
