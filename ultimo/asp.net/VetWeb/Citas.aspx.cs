using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions; // Necesario para expresiones regulares
using System.Web.UI; // Necesario para ScriptManager
using System.Web.UI.WebControls;
using iTextSharp.text; // Add this for iTextSharp
using iTextSharp.text.pdf; // Add this for iTextSharp
using System.IO; // Add this for MemoryStream and File.Exists
using System.Linq; // Add this for Enumerable.Repeat

namespace VetWeb
{
    public partial class Citas : System.Web.UI.Page
    {
        string cadena = ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarClientes();
                // Si hay un cliente seleccionado (ej. al volver de otra página con datos en caché),
                // cargar sus mascotas. De lo contrario, ddlMascotas tendrá solo la opción por defecto.
                if (!string.IsNullOrEmpty(ddlClientes.SelectedValue))
                {
                    CargarMascotasPorCliente(Convert.ToInt32(ddlClientes.SelectedValue));
                }
                else
                {
                    // Asegurarse de que ddlMascotas tenga la opción por defecto si no hay cliente seleccionado
                    ddlMascotas.Items.Clear();
                    ddlMascotas.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione una Mascota", ""));
                }
                CargarEmpleados();
                CargarCitas();
                // Establecer el estado inicial de los botones del modal (modo agregar)
                btnActualizar.Style["display"] = "none";
                btnAgregar.Style["display"] = "inline-block";
                // Limpiar el formulario y el mensaje al cargar la página por primera vez
                LimpiarFormulario();
            }
        }

        /// <summary>
        /// Carga el DropDownList de Clientes desde la base de datos.
        /// </summary>
        private void CargarClientes()
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT ClienteID, PrimerNombre + ' ' + ApellidoPaterno AS NombreCompleto FROM Clientes ORDER BY PrimerNombre", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlClientes.DataSource = dt;
                ddlClientes.DataTextField = "NombreCompleto";
                ddlClientes.DataValueField = "ClienteID";
                ddlClientes.DataBind();
                ddlClientes.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione un Cliente", ""));
            }
        }

        /// <summary>
        /// Maneja el cambio de selección en el DropDownList de Clientes para cargar las mascotas correspondientes.
        /// </summary>
        protected void ddlClientes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlClientes.SelectedValue))
            {
                CargarMascotasPorCliente(Convert.ToInt32(ddlClientes.SelectedValue));
            }
            else
            {
                ddlMascotas.Items.Clear();
                ddlMascotas.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione una Mascota", ""));
            }
            // Asegurarse de que el modal permanezca abierto si se activa desde allí
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCitaModalOnDdlChange", "showCitaModal();", true);
        }

        /// <summary>
        /// Carga el DropDownList de Mascotas filtrando por ClienteID.
        /// </summary>
        /// <param name="clienteID">El ID del cliente para filtrar las mascotas.</param>
        private void CargarMascotasPorCliente(int clienteID)
        {
            ddlMascotas.Items.Clear(); // Limpiar antes de cargar
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "SELECT MascotaID, Nombre FROM Mascotas WHERE ClienteID = @ClienteID ORDER BY Nombre";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ClienteID", clienteID);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlMascotas.DataSource = dt;
                ddlMascotas.DataTextField = "Nombre";
                ddlMascotas.DataValueField = "MascotaID";
                ddlMascotas.DataBind();
                ddlMascotas.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione una Mascota", ""));
            }
        }

        /// <summary>
        /// Carga el DropDownList de Empleados desde la base de datos.
        /// </summary>
        private void CargarEmpleados()
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "SELECT EmpleadoID, PrimerNombre + ' ' + ApellidoPaterno AS NombreCompleto FROM Empleados ORDER BY PrimerNombre";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlEmpleados.DataSource = dt;
                ddlEmpleados.DataTextField = "NombreCompleto";
                ddlEmpleados.DataValueField = "EmpleadoID";
                ddlEmpleados.DataBind();
                ddlEmpleados.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione un Empleado", ""));
            }
        }

        /// <summary>
        /// Carga los datos de citas desde la base de datos y los enlaza al GridView,
        /// opcionalmente filtrando por nombre de cliente, mascota o empleado.
        /// </summary>
        /// <param name="searchTerm">Término opcional para buscar.</param>
        private void CargarCitas(string searchTerm = null)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"
                    SELECT
                        C.CitaID,
                        C.Fecha,
                        Cl.PrimerNombre + ' ' + Cl.ApellidoPaterno AS NombreCliente,
                        M.Nombre AS NombreMascota,
                        E.PrimerNombre + ' ' + E.ApellidoPaterno AS NombreEmpleado,
                        M.MascotaID, 
                        E.EmpleadoID, 
                        Cl.ClienteID 
                    FROM Citas C
                    INNER JOIN Mascotas M ON C.MascotaID = M.MascotaID
                    INNER JOIN Clientes Cl ON M.ClienteID = Cl.ClienteID 
                    INNER JOIN Empleados E ON C.EmpleadoID = E.EmpleadoID";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += " WHERE Cl.PrimerNombre LIKE '%' + @SearchTerm + '%' " +
                             "OR Cl.ApellidoPaterno LIKE '%' + @SearchTerm + '%' " +
                             "OR M.Nombre LIKE '%' + @SearchTerm + '%' " +
                             "OR E.PrimerNombre LIKE '%' + @SearchTerm + '%' " +
                             "OR E.ApellidoPaterno LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY C.Fecha DESC";

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvCitas.DataSource = dt;
                gvCitas.DataBind();
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Agregar" en el modal.
        /// Agrega un nuevo registro de cita a la base de datos.
        /// </summary>
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "INSERT INTO Citas (Fecha, MascotaID, EmpleadoID) VALUES (@Fecha, @MascotaID, @EmpleadoID)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Fecha", Convert.ToDateTime(txtFecha.Text.Trim()));
                cmd.Parameters.AddWithValue("@MascotaID", Convert.ToInt32(ddlMascotas.SelectedValue));
                cmd.Parameters.AddWithValue("@EmpleadoID", Convert.ToInt32(ddlEmpleados.SelectedValue));

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    MostrarMensaje("Cita agendada correctamente.", true);
                    successOperation = true;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE (si la hubiera, por ejemplo, cita duplicada para misma mascota/fecha)
                    {
                        MostrarMensaje("Error: Ya existe una cita similar programada. Por favor, verifique.", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al agendar la cita: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al agendar la cita: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarCitas();
            }
            else
            {
                CargarCitas(txtBuscarCita.Text.Trim()); // Refrescar el grid con el filtro actual
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Actualizar" en el modal.
        /// Actualiza un registro de cita existente en la base de datos.
        /// </summary>
        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            int citaID;
            if (!int.TryParse(hfCitaID.Value, out citaID))
            {
                MostrarMensaje("Error: El ID de la cita no tiene un formato válido para actualizar. Por favor, intente editar de nuevo.", false);
                return;
            }

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "UPDATE Citas SET Fecha=@Fecha, MascotaID=@MascotaID, EmpleadoID=@EmpleadoID WHERE CitaID=@CitaID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Fecha", Convert.ToDateTime(txtFecha.Text.Trim()));
                cmd.Parameters.AddWithValue("@MascotaID", Convert.ToInt32(ddlMascotas.SelectedValue));
                cmd.Parameters.AddWithValue("@EmpleadoID", Convert.ToInt32(ddlEmpleados.SelectedValue));
                cmd.Parameters.AddWithValue("@CitaID", citaID);

                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MostrarMensaje("Cita actualizada correctamente.", true);
                        successOperation = true;
                    }
                    else
                    {
                        MostrarMensaje("No se encontró la cita para actualizar o no hubo cambios.", false);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE
                    {
                        MostrarMensaje("Error: Ya existe una cita similar programada. Por favor, verifique.", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al actualizar la cita: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al actualizar la cita: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarCitas();
            }
            else
            {
                CargarCitas(txtBuscarCita.Text.Trim()); // Refrescar el grid con el filtro actual
            }
        }

        /// <summary>
        /// Maneja los comandos de fila del GridView (Editar, Eliminar).
        /// </summary>
        protected void gvCitas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);

            // Obtener IDs usando DataKeys para robustez
            if (gvCitas.DataKeys == null || index < 0 || index >= gvCitas.DataKeys.Count)
            {
                MostrarMensaje("Error interno: No se pudo obtener el ID de la cita. Por favor, recargue la página.", false);
                return;
            }
            int citaID = Convert.ToInt32(gvCitas.DataKeys[index]["CitaID"]);
            int mascotaID = Convert.ToInt32(gvCitas.DataKeys[index]["MascotaID"]);
            int empleadoID = Convert.ToInt32(gvCitas.DataKeys[index]["EmpleadoID"]);
            int clienteID = Convert.ToInt32(gvCitas.DataKeys[index]["ClienteID"]);

            GridViewRow row = gvCitas.Rows[index];

            if (e.CommandName == "Editar")
            {
                // Formatear la fecha a YYYY-MM-DD para el control Flatpickr
                // gvCitas.DataKeys[index]["Fecha"] es más fiable si CitaID es la única DataKey
                // Si 'Fecha' no está en DataKeys, obténlo de la celda.
                // Asegúrate de que el formato de la fecha de la celda sea parseable.
                string fechaCelda = row.Cells[0].Text.Trim(); // Asumiendo que la Fecha es la primera columna visible
                DateTime fechaCita;
                if (DateTime.TryParse(fechaCelda, out fechaCita))
                {
                    txtFecha.Text = fechaCita.ToString("yyyy-MM-dd");
                }
                else
                {
                    // Fallback o mensaje de error si la fecha de la celda no se puede parsear
                    MostrarMensaje("Error al cargar la fecha de la cita para edición. Formato incorrecto.", false);
                    // Opcional: No continuar con la edición si la fecha es crítica y está mal formateada.
                }

                // Seleccionar Cliente (esto NO dispara ddlClientes_SelectedIndexChanged inmediatamente en el cliente)
                ddlClientes.SelectedValue = clienteID.ToString();

                // Recargar Mascotas para el cliente seleccionado (crucial porque el AutoPostBack no se disparó aquí)
                CargarMascotasPorCliente(clienteID);
                // Ahora que las mascotas están cargadas, seleccionar la mascota correcta
                ddlMascotas.SelectedValue = mascotaID.ToString();

                // Seleccionar Empleado
                ddlEmpleados.SelectedValue = empleadoID.ToString();

                hfCitaID.Value = citaID.ToString();

                // Cambia la visibilidad de los botones en el modal
                btnAgregar.Style["display"] = "none";
                btnActualizar.Style["display"] = "inline-block";

                // Actualiza el título del modal antes de mostrarlo
                ScriptManager.RegisterStartupScript(this, this.GetType(), "SetCitaModalTitle", "document.getElementById('citaModalLabel').innerText = 'Editar Cita';", true);

                // Limpia el mensaje si lo hubiera y luego muestra el modal.
                MostrarMensaje("", false);
            }
            else if (e.CommandName == "Eliminar")
            {
                using (SqlConnection con = new SqlConnection(cadena))
                {
                    con.Open();
                    SqlTransaction transaction = con.BeginTransaction(); // Iniciar transacción

                    try
                    {
                        // **VALIDACIÓN DE DEPENDENCIAS ANTES DE ELIMINAR**
                        // Verificar si hay CitaServicios asociadas a esta cita
                        SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM CitaServicios WHERE CitaID = @CitaID", con, transaction);
                        checkCmd.Parameters.AddWithValue("@CitaID", citaID);
                        int dependentCitaServicios = (int)checkCmd.ExecuteScalar();

                        if (dependentCitaServicios > 0)
                        {
                            MostrarMensaje("No se puede eliminar esta cita porque tiene " + dependentCitaServicios + " servicio(s) asociado(s). Elimine los servicios asociados a la cita primero.", false);
                            transaction.Rollback(); // Revertir si hay dependencias
                            return;
                        }

                        // Si no hay dependencias, proceder con la eliminación
                        SqlCommand cmd = new SqlCommand("DELETE FROM Citas WHERE CitaID = @CitaID", con, transaction);
                        cmd.Parameters.AddWithValue("@CitaID", citaID);
                        cmd.ExecuteNonQuery();

                        transaction.Commit(); // Confirmar la transacción
                        MostrarMensaje("Cita eliminada correctamente.", true);
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback(); // Revertir en caso de error
                        if (ex.Number == 547) // Error de clave foránea
                        {
                            MostrarMensaje("No se pudo eliminar la cita debido a registros asociados. Elimine los registros asociados primero.", false);
                        }
                        else
                        {
                            MostrarMensaje("Error de base de datos al eliminar cita: " + ex.Message, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        MostrarMensaje("Ocurrió un error inesperado al eliminar la cita: " + ex.Message, false);
                    }
                    finally
                    {
                        con.Close();
                    }
                }
                CargarCitas(); // Refrescar el GridView después de eliminar
            }
        }

        /// <summary>
        /// Limpia los campos del formulario del modal y restablece la UI al modo "Agregar".
        /// </summary>
        private void LimpiarFormulario()
        {
            txtFecha.Text = "";
            ddlClientes.ClearSelection();
            if (ddlClientes.Items.Count > 0) ddlClientes.Items.FindByValue("").Selected = true;
            ddlMascotas.Items.Clear(); // Limpiar y resetear mascotas
            ddlMascotas.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione una Mascota", ""));
            ddlEmpleados.ClearSelection();
            if (ddlEmpleados.Items.Count > 0) ddlEmpleados.Items.FindByValue("").Selected = true;
            hfCitaID.Value = "";
            btnAgregar.Style["display"] = "inline-block";
            btnActualizar.Style["display"] = "none";

            // También restablecer el título del modal y limpiar el mensaje
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetCitaModalTitle", "document.getElementById('citaModalLabel').innerText = 'Agendar Nueva Cita';", true);
            // Limpiar mensaje explícitamente sin invocar show/hide modal
            lblMensaje.Text = "";
            lblMensaje.CssClass = "";
        }

        /// <summary>
        /// Valida los campos de entrada del formulario de Cita.
        /// </summary>
        /// <returns>True si el formulario es válido, false en caso contrario.</returns>
        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtFecha.Text) ||
                string.IsNullOrEmpty(ddlClientes.SelectedValue) || ddlClientes.SelectedValue == "" ||
                string.IsNullOrEmpty(ddlMascotas.SelectedValue) || ddlMascotas.SelectedValue == "" ||
                string.IsNullOrEmpty(ddlEmpleados.SelectedValue) || ddlEmpleados.SelectedValue == "")
            {
                MostrarMensaje("Por favor, complete todos los campos obligatorios: Fecha, Cliente, Mascota y Empleado.", false);
                return false;
            }

            DateTime fechaCita;
            // Intentar parsear con el formato esperado por Flatpickr (YYYY-MM-DD)
            if (!DateTime.TryParseExact(txtFecha.Text.Trim(), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out fechaCita))
            {
                MostrarMensaje("El formato de la fecha no es válido. Use YYYY-MM-DD.", false);
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
                lblMensaje.CssClass = ""; // ¡IMPORTANTE! Limpiar la clase CSS
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
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideCitaModalOnSuccess", "hideCitaModal();", true);
            }
            else // Esto cubre mensajes de error y la lógica de "editar" (donde el mensaje está vacío y exito es false)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCitaModalGeneral", "showCitaModal();", true);
            }

            // Asegurar que el mensaje sea visible dentro del modal
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollToMessageCita" + Guid.NewGuid().ToString(),
                "var modalBody = document.querySelector('#citaModal .modal-body'); if(modalBody) modalBody.scrollTop = 0;", true);
        }

        /// <summary>
        /// Maneja el evento de clic del botón de búsqueda.
        /// Filtra la lista de citas basada en el término de búsqueda.
        /// </summary>
        protected void btnBuscarCita_Click(object sender, EventArgs e)
        {
            CargarCitas(txtBuscarCita.Text.Trim());
        }

        /// <summary>
        /// Maneja el evento de clic del botón de limpiar búsqueda.
        /// Limpia el término de búsqueda y recarga la lista completa de citas.
        /// </summary>
        protected void btnLimpiarBusquedaCita_Click(object sender, EventArgs e)
        {
            txtBuscarCita.Text = ""; // Limpiar el textbox de búsqueda
            CargarCitas(); // Recargar todas las citas sin filtro
        }

        protected void btnImprimirPdf_Click(object sender, EventArgs e)
        {
            DataTable dtCitas = new DataTable();

            using (SqlConnection con = new SqlConnection(cadena))
            {
                // Obtener los datos de las citas (aplicando el filtro de búsqueda actual si lo hay)
                string query = @"
                    SELECT
                        C.Fecha,
                        Cl.PrimerNombre + ' ' + Cl.ApellidoPaterno AS NombreCliente,
                        M.Nombre AS NombreMascota,
                        E.PrimerNombre + ' ' + E.ApellidoPaterno AS NombreEmpleado
                    FROM Citas C
                    INNER JOIN Mascotas M ON C.MascotaID = M.MascotaID
                    INNER JOIN Clientes Cl ON M.ClienteID = Cl.ClienteID
                    INNER JOIN Empleados E ON C.EmpleadoID = E.EmpleadoID";

                // Aplicar el filtro de búsqueda si el campo txtBuscarCita no está vacío
                if (!string.IsNullOrEmpty(txtBuscarCita.Text.Trim()))
                {
                    query += " WHERE Cl.PrimerNombre LIKE '%' + @SearchTerm + '%' " +
                             "OR Cl.ApellidoPaterno LIKE '%' + @SearchTerm + '%' " +
                             "OR M.Nombre LIKE '%' + @SearchTerm + '%' " +
                             "OR E.PrimerNombre LIKE '%' + @SearchTerm + '%' " +
                             "OR E.ApellidoPaterno LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY C.Fecha DESC"; // Ordenar las citas por fecha

                SqlCommand cmd = new SqlCommand(query, con);
                if (!string.IsNullOrEmpty(txtBuscarCita.Text.Trim()))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", txtBuscarCita.Text.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                try
                {
                    con.Open();
                    da.Fill(dtCitas);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al cargar los datos de las citas para el PDF: " + ex.Message, false);
                    return;
                }
            }

            if (dtCitas.Rows.Count == 0)
            {
                MostrarMensaje("No hay datos de citas para generar el PDF con el filtro actual.", false);
                return;
            }

            // Crear el documento PDF
            Document doc = new Document(PageSize.A4, 30f, 30f, 40f, 30f); // Márgenes

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                    doc.Open();

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
                    Paragraph reportTitle = new Paragraph("REPORTE DE CITAS", reportTitleFont);
                    reportTitle.Alignment = Element.ALIGN_CENTER;
                    reportTitle.SpacingAfter = 15f;
                    doc.Add(reportTitle);

                    // Información del Documento (Folio, Fecha de Generación, Filtro Aplicado)
                    PdfPTable docDetailsTable = new PdfPTable(2);
                    docDetailsTable.WidthPercentage = 100;
                    docDetailsTable.DefaultCell.Border = PdfPCell.NO_BORDER;
                    docDetailsTable.SetWidths(new float[] { 1f, 1f });
                    docDetailsTable.SpacingAfter = 10f;

                    Font fontDocDetails = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);

                    docDetailsTable.AddCell(new PdfPCell(new Phrase($"FOLIO: {new Random().Next(100000, 999999)}", fontDocDetails)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT });
                    docDetailsTable.AddCell(new PdfPCell(new Phrase($"Fecha de Generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", fontDocDetails)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

                    string filtroAplicado = string.IsNullOrEmpty(txtBuscarCita.Text.Trim()) ? "Ninguno" : txtBuscarCita.Text.Trim();
                    docDetailsTable.AddCell(new PdfPCell(new Phrase($"Filtro aplicado: \"{filtroAplicado}\"", fontDocDetails)) { Colspan = 2, Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT });

                    doc.Add(docDetailsTable);

                    // ====================================================================
                    // 2. TABLA DE DATOS DE CITAS
                    // ====================================================================

                    PdfPTable pdfTable = new PdfPTable(dtCitas.Columns.Count);
                    pdfTable.WidthPercentage = 100;
                    pdfTable.SpacingBefore = 10f;
                    pdfTable.DefaultCell.Padding = 5;
                    pdfTable.HeaderRows = 1;

                    // Las columnas son: Fecha, NombreCliente, NombreMascota, NombreEmpleado
                    float[] widths = new float[] { 1.5f, 2.5f, 2f, 2.5f };
                    if (dtCitas.Columns.Count == widths.Length)
                    {
                        pdfTable.SetWidths(widths);
                    }
                    else
                    {
                        pdfTable.SetWidths(Enumerable.Repeat(1f, dtCitas.Columns.Count).ToArray());
                    }

                    // Añadir encabezados de columna
                    Font fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);
                    BaseColor headerColor = new BaseColor(54, 80, 106);
                    string[] headers = { "Fecha", "Cliente", "Mascota", "Empleado" };

                    for (int i = 0; i < dtCitas.Columns.Count; i++)
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
                    Font fontCell = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK);
                    foreach (DataRow row in dtCitas.Rows)
                    {
                        for (int i = 0; i < dtCitas.Columns.Count; i++)
                        {
                            string cellText = row[i].ToString();
                            // Formatear la fecha si es la columna de Fecha
                            if (dtCitas.Columns[i].ColumnName == "Fecha" && DateTime.TryParse(cellText, out DateTime dateValue))
                            {
                                cellText = dateValue.ToString("dd/MM/yyyy HH:mm"); // Formato deseado para la fecha y hora
                            }

                            PdfPCell dataCell = new PdfPCell(new Phrase(cellText, fontCell));
                            dataCell.HorizontalAlignment = Element.ALIGN_LEFT;
                            dataCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            dataCell.Padding = 5;
                            if (dtCitas.Rows.IndexOf(row) % 2 == 0)
                            {
                                dataCell.BackgroundColor = new BaseColor(245, 245, 245);
                            }
                            dataCell.BorderColor = BaseColor.LIGHT_GRAY;
                            pdfTable.AddCell(dataCell);
                        }
                    }

                    doc.Add(pdfTable);

                    // ====================================================================
                    // 3. PIE DE PÁGINA DEL DOCUMENTO (Notas, etc.)
                    // ====================================================================
                    Font fontFooter = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9, BaseColor.GRAY);
                    Paragraph footerNote = new Paragraph("Este es un reporte interno de citas de VetWeb.", fontFooter);
                    footerNote.Alignment = Element.ALIGN_CENTER;
                    footerNote.SpacingBefore = 20f;
                    doc.Add(footerNote);

                    Paragraph thankYouNote = new Paragraph("Generado por VetWeb - Tu solución para la gestión veterinaria.", FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.LIGHT_GRAY));
                    thankYouNote.Alignment = Element.ALIGN_CENTER;
                    doc.Add(thankYouNote);

                    doc.Close();

                    // Enviar el PDF al navegador
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=ReporteCitas.pdf");
                    Response.Buffer = true;
                    Response.Clear();
                    Response.BinaryWrite(ms.ToArray());
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al generar el PDF de citas: " + ex.Message, false);
            }
            finally
            {
                if (doc.IsOpen())
                {
                    doc.Close();
                }
            }
        }
    }
}
