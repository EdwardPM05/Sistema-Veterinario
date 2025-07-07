using iTextSharp.text; // Necesario para iTextSharp
using iTextSharp.text.pdf; // Necesario para iTextSharp
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO; // Necesario para MemoryStream
using System.Linq;
using System.Text.RegularExpressions;
using System.Web; // Necesario para HttpResponse
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

namespace VetWeb
{
    public partial class Clientes : System.Web.UI.Page
    {
        string cadena = ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarClientes();
                // Estado inicial de los botones del modal (modo agregar)
                btnActualizar.Style["display"] = "none";
                btnAgregar.Style["display"] = "inline-block";
            }
        }

        /// <summary>
        /// Carga los datos de clientes desde la base de datos y los enlaza al GridView,
        /// opcionalmente filtrando por nombre o DNI.
        /// </summary>
        /// <param name="searchTerm">Término opcional para buscar en PrimerNombre, ApellidoPaterno o DNI.</param>
        private void CargarClientes(string searchTerm = null)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "SELECT ClienteID, PrimerNombre, ApellidoPaterno, ApellidoMaterno, DNI, Telefono, Direccion, Correo FROM Clientes";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += " WHERE PrimerNombre LIKE '%' + @SearchTerm + '%' " +
                             "OR ApellidoPaterno LIKE '%' + @SearchTerm + '%' " +
                             "OR ApellidoMaterno LIKE '%' + @SearchTerm + '%' " +
                             "OR DNI LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY PrimerNombre, ApellidoPaterno"; // Ordenar para una visualización consistente

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvClientes.DataSource = dt;
                gvClientes.DataBind();
            }
        }

        /// <summary>
        /// Maneja el cambio de página en el GridView. (Se mantuvo por si es necesario para paginación)
        /// </summary>
        protected void gvClientes_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvClientes.PageIndex = e.NewPageIndex;
            CargarClientes(txtBuscarNombreCliente.Text.Trim()); // Mantener el filtro de búsqueda al cambiar de página
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Agregar" en el modal.
        /// Agrega un nuevo registro de cliente a la base de datos.
        /// </summary>
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            bool successOperation = false; // Bandera para controlar el éxito de la operación
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "INSERT INTO Clientes (PrimerNombre, ApellidoPaterno, ApellidoMaterno, DNI, Telefono, Direccion, Correo) " +
                               "VALUES (@PrimerNombre, @ApellidoPaterno, @ApellidoMaterno, @DNI, @Telefono, @Direccion, @Correo)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PrimerNombre", txtPrimerNombre.Text.Trim());
                cmd.Parameters.AddWithValue("@ApellidoPaterno", txtApellidoPaterno.Text.Trim());
                cmd.Parameters.AddWithValue("@ApellidoMaterno", txtApellidoMaterno.Text.Trim());
                cmd.Parameters.AddWithValue("@DNI", txtDNI.Text.Trim());
                cmd.Parameters.AddWithValue("@Telefono", txtTelefono.Text.Trim());
                cmd.Parameters.AddWithValue("@Direccion", txtDireccion.Text.Trim());
                cmd.Parameters.AddWithValue("@Correo", txtCorreo.Text.Trim());

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    MostrarMensaje("Cliente agregado correctamente.", true);
                    successOperation = true;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE (ej. DNI o Correo ya existen si tienen unique constraint)
                    {
                        if (ex.Message.Contains("IX_Clientes_DNI")) // Asumiendo un índice único en DNI
                        {
                            MostrarMensaje("Error: Ya existe un cliente con el DNI ingresado. Por favor, verifique.", false);
                        }
                        else if (ex.Message.Contains("IX_Clientes_Correo")) // Asumiendo un índice único en Correo
                        {
                            MostrarMensaje("Error: Ya existe el correo electrónico ingresado. ", false);
                        }
                        else
                        {
                            MostrarMensaje("Ya existe un registro con datos duplicados. " + ex.Message, false);
                        }
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error al agregar el cliente: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al agregar el cliente: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            // Actualizar UI basado en el éxito o fallo
            if (successOperation)
            {
                LimpiarFormulario(); // Limpiar solo si la operación fue exitosa
                CargarClientes(); // Recargar el GridView (sin filtro de búsqueda)
            }
            else
            {
                // Si hubo un error, el modal permanece abierto y los datos se conservan.
                // Recargar el grid con el término de búsqueda actual si lo hay.
                CargarClientes(txtBuscarNombreCliente.Text.Trim());
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Actualizar" en el modal.
        /// Actualiza un registro de cliente existente en la base de datos.
        /// </summary>
        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            int clienteID;
            if (!int.TryParse(hfClienteID.Value, out clienteID))
            {
                MostrarMensaje("Error: El ID del cliente no tiene un formato válido para actualizar.", false);
                return;
            }

            bool successOperation = false; // Bandera para controlar el éxito de la operación
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "UPDATE Clientes SET PrimerNombre=@PrimerNombre, ApellidoPaterno=@ApellidoPaterno, ApellidoMaterno=@ApellidoMaterno, " +
                               "DNI=@DNI, Telefono=@Telefono, Direccion=@Direccion, Correo=@Correo WHERE ClienteID=@ClienteID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PrimerNombre", txtPrimerNombre.Text.Trim());
                cmd.Parameters.AddWithValue("@ApellidoPaterno", txtApellidoPaterno.Text.Trim());
                cmd.Parameters.AddWithValue("@ApellidoMaterno", txtApellidoMaterno.Text.Trim());
                cmd.Parameters.AddWithValue("@DNI", txtDNI.Text.Trim());
                cmd.Parameters.AddWithValue("@Telefono", txtTelefono.Text.Trim());
                cmd.Parameters.AddWithValue("@Direccion", txtDireccion.Text.Trim());
                cmd.Parameters.AddWithValue("@Correo", txtCorreo.Text.Trim());
                cmd.Parameters.AddWithValue("@ClienteID", clienteID); // Usar el ID parseado

                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MostrarMensaje("Cliente actualizado correctamente.", true);
                        successOperation = true;
                    }
                    else
                    {
                        MostrarMensaje("No se encontró el cliente para actualizar o no hubo cambios.", false);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE
                    {
                        if (ex.Message.Contains("IX_Clientes_DNI"))
                        {
                            MostrarMensaje("Error: Ya existe un cliente con el DNI ingresado.", false);
                        }
                        else if (ex.Message.Contains("IX_Clientes_Correo"))
                        {
                            MostrarMensaje("Error: Ya existe un cliente con el correo ingresado.", false);
                        }
                        else
                        {
                            MostrarMensaje("Error al actualizar cliente: Datos duplicados. " + ex.Message, false);
                        }
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error al actualizar el cliente: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error al actualizar el cliente: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            // Actualizar UI basado en el éxito o fallo
            if (successOperation)
            {
                LimpiarFormulario(); // Limpiar solo si la operación fue exitosa
                CargarClientes(); // Recargar el GridView (sin filtro de búsqueda)
            }
            else
            {
                CargarClientes(txtBuscarNombreCliente.Text.Trim()); // Recargar el grid con el término de búsqueda actual si lo hay
            }
        }

        /// <summary>
        /// Maneja los comandos de fila del GridView (Editar, Eliminar).
        /// </summary>
        protected void gvClientes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);

            // Obtener ClienteID usando DataKeys para robustez
            if (gvClientes.DataKeys == null || index < 0 || index >= gvClientes.DataKeys.Count)
            {
                MostrarMensaje("Error interno: No se pudo obtener el ID del cliente. Por favor, recargue la página.", false);
                return;
            }
            int clienteID = Convert.ToInt32(gvClientes.DataKeys[index].Value);

            GridViewRow row = gvClientes.Rows[index];

            if (e.CommandName == "Editar")
            {
                // Asegúrate de que los índices de las celdas coinciden con el orden de tus BoundFields en el ASPX
                // (ID está oculto, así que PrimerNombre es Cells[0], etc.)
                // Los índices de las celdas corresponden a las columnas VISIBLES del GridView.
                // En tu ASPX, tienes: PrimerNombre, ApellidoPaterno, ApellidoMaterno, DNI, Telefono, Direccion, Correo.
                // Así que PrimerNombre es Cells[0], ApellidoPaterno es Cells[1], etc.
                txtPrimerNombre.Text = HttpUtility.HtmlDecode(row.Cells[0].Text);
                txtApellidoPaterno.Text = HttpUtility.HtmlDecode(row.Cells[1].Text);
                txtApellidoMaterno.Text = HttpUtility.HtmlDecode(row.Cells[2].Text);
                txtDNI.Text = HttpUtility.HtmlDecode(row.Cells[3].Text);
                txtTelefono.Text = HttpUtility.HtmlDecode(row.Cells[4].Text);
                txtDireccion.Text = HttpUtility.HtmlDecode(row.Cells[5].Text);
                txtCorreo.Text = HttpUtility.HtmlDecode(row.Cells[6].Text);
                hfClienteID.Value = clienteID.ToString();

                // Cambiar visibilidad de botones y título del modal
                btnAgregar.Style["display"] = "none";
                btnActualizar.Style["display"] = "inline-block";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "SetClienteModalTitle", "document.getElementById('clienteModalLabel').innerText = 'Editar Cliente';", true);

                MostrarMensaje("", false); // Limpiar mensajes previos
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowClienteModalScript", "showClienteModal();", true); // Mostrar modal
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
                        // Verificar si hay mascotas asociadas a este cliente
                        SqlCommand checkMascotasCmd = new SqlCommand("SELECT COUNT(*) FROM Mascotas WHERE ClienteID = @ClienteID", con, transaction);
                        checkMascotasCmd.Parameters.AddWithValue("@ClienteID", clienteID);
                        int numMascotas = (int)checkMascotasCmd.ExecuteScalar();

                        if (numMascotas > 0)
                        {
                            MostrarMensaje("No se puede eliminar este cliente porque tiene " + numMascotas + " mascota(s) asociada(s).", false);
                            transaction.Rollback(); // Revertir si hay dependencias
                            return;
                        }

                        // Si no hay mascotas asociadas, proceder con la eliminación
                        SqlCommand cmd = new SqlCommand("DELETE FROM Clientes WHERE ClienteID = @ClienteID", con, transaction);
                        cmd.Parameters.AddWithValue("@ClienteID", clienteID);
                        cmd.ExecuteNonQuery();

                        transaction.Commit(); // Confirmar la transacción
                        MostrarMensaje("Cliente eliminado correctamente.", true);
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback(); // Revertir en caso de error
                        if (ex.Number == 547) // Error de clave foránea
                        {
                            MostrarMensaje("No se pudo eliminar el cliente porque tiene registros asociados (ej. citas, ventas).", false);
                        }
                        else
                        {
                            MostrarMensaje("Error de base de datos al eliminar cliente: " + ex.Message, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        MostrarMensaje("Ocurrió un error inesperado al eliminar el cliente: " + ex.Message, false);
                    }
                    finally
                    {
                        con.Close();
                    }
                }
                CargarClientes(); // Refrescar el GridView después de eliminar
            }
        }

        /// <summary>
        /// Limpia los campos del formulario y restablece la UI al modo "Agregar".
        /// </summary>
        private void LimpiarFormulario()
        {
            txtPrimerNombre.Text = "";
            txtApellidoPaterno.Text = "";
            txtApellidoMaterno.Text = "";
            txtDNI.Text = "";
            txtTelefono.Text = "";
            txtDireccion.Text = "";
            txtCorreo.Text = "";
            hfClienteID.Value = "";
            btnAgregar.Style["display"] = "inline-block";
            btnActualizar.Style["display"] = "none";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetClienteModalTitle", "document.getElementById('clienteModalLabel').innerText = 'Agregar Nuevo Cliente';", true);
            MostrarMensaje("", false); // Limpiar mensajes al limpiar formulario
        }

        /// <summary>
        /// Valida los campos de entrada del formulario de Cliente.
        /// </summary>
        /// <returns>True si el formulario es válido, false en caso contrario.</returns>
        private bool ValidarFormulario()
        {
            // Validaciones iniciales de campos obligatorios vacíos
            if (string.IsNullOrWhiteSpace(txtPrimerNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellidoPaterno.Text) ||
                string.IsNullOrWhiteSpace(txtDNI.Text) ||
                string.IsNullOrWhiteSpace(txtTelefono.Text) ||
                string.IsNullOrWhiteSpace(txtDireccion.Text) ||
                string.IsNullOrWhiteSpace(txtCorreo.Text))
            {
                MostrarMensaje("Por favor, complete todos los campos obligatorios.", false);
                return false;
            }

            // Validar PrimerNombre, ApellidoPaterno, ApellidoMaterno (solo letras y espacios, incluyendo tildes y 'ñ')
            if (!Regex.IsMatch(txtPrimerNombre.Text.Trim(), @"^[\p{L}\s]+$"))
            {
                MostrarMensaje("El campo Primer Nombre solo puede contener letras.  ", false);
                return false;
            }
            if (!Regex.IsMatch(txtApellidoPaterno.Text.Trim(), @"^[\p{L}\s]+$"))
            {
                MostrarMensaje("El campo Apellido Paterno solo puede contener letras.", false);
                return false;
            }
            if (!string.IsNullOrWhiteSpace(txtApellidoMaterno.Text) && !Regex.IsMatch(txtApellidoMaterno.Text.Trim(), @"^[\p{L}\s]+$"))
            {
                MostrarMensaje("El campo Apellido Materno solo puede contener letras.", false);
                return false;
            }

            // Validar DNI (exactamente 8 dígitos numéricos)
            if (!Regex.IsMatch(txtDNI.Text.Trim(), @"^\d{8}$"))
            {
                MostrarMensaje("El DNI debe contener exactamente 8 dígitos numéricos.", false);
                return false;
            }

            // Validar Telefono (exactamente 9 dígitos numéricos)
            if (!Regex.IsMatch(txtTelefono.Text.Trim(), @"^\d{9}$"))
            {
                MostrarMensaje("El Teléfono debe contener exactamente 9 dígitos numéricos.", false);
                return false;
            }

            // Validar Correo (formato básico de email)
            if (!Regex.IsMatch(txtCorreo.Text.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MostrarMensaje("El Correo electrónico no tiene un formato válido. Ej: usuario@ejemplo.com", false);
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
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowClienteModalOnError", "showClienteModal();", true);
                }
                else // Si es un mensaje de éxito
                {
                    // Ocultar el modal en caso de éxito
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideClienteModalOnSuccess", "hideClienteModal();", true);
                }
            }
            // Asegurar que el mensaje sea visible dentro del modal
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollToMessageCliente", "var modalBody = document.querySelector('#clienteModal .modal-body'); if(modalBody) modalBody.scrollTop = 0;", true);
        }

        /// <summary>
        /// Maneja el evento de clic del botón de búsqueda.
        /// Filtra la lista de clientes basada en el término de búsqueda.
        /// </summary>
        protected void btnBuscarCliente_Click(object sender, EventArgs e)
        {
            CargarClientes(txtBuscarNombreCliente.Text.Trim());
        }

        /// <summary>
        /// Maneja el evento de clic del botón de limpiar búsqueda.
        /// Limpia el término de búsqueda y recarga la lista completa de clientes.
        /// </summary>
        protected void btnLimpiarBusquedaCliente_Click(object sender, EventArgs e)
        {
            txtBuscarNombreCliente.Text = ""; // Limpiar el textbox de búsqueda
            CargarClientes(); // Recargar todos los clientes sin filtro
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Imprimir PDF".
        /// Genera un archivo PDF con los datos de la tabla de clientes.
        /// </summary>
        protected void btnImprimirPdf_Click(object sender, EventArgs e)
        {
            DataTable dtClientes = new DataTable();

            using (SqlConnection con = new SqlConnection(cadena))
            {
                // Obtener los datos de los clientes (aplicando el filtro de búsqueda actual si lo hay)
                string query = "SELECT PrimerNombre, ApellidoPaterno, ApellidoMaterno, DNI, Telefono, Direccion, Correo FROM Clientes";
                if (!string.IsNullOrEmpty(txtBuscarNombreCliente.Text.Trim()))
                {
                    query += " WHERE PrimerNombre LIKE '%' + @SearchTerm + '%' " +
                             "OR ApellidoPaterno LIKE '%' + @SearchTerm + '%' " +
                             "OR ApellidoMaterno LIKE '%' + @SearchTerm + '%' " +
                             "OR DNI LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY PrimerNombre, ApellidoPaterno";

                SqlCommand cmd = new SqlCommand(query, con);
                if (!string.IsNullOrEmpty(txtBuscarNombreCliente.Text.Trim()))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", txtBuscarNombreCliente.Text.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                try
                {
                    con.Open();
                    da.Fill(dtClientes);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al cargar los datos para el PDF: " + ex.Message, false);
                    return;
                }
            }

            if (dtClientes.Rows.Count == 0)
            {
                MostrarMensaje("No hay datos de clientes para generar el PDF con el filtro actual.", false);
                return;
            }

            // Crear el documento PDF
            Document doc = new Document(PageSize.A4, 30f, 30f, 40f, 30f); // Márgenes (izquierda, derecha, arriba, abajo) ajustados

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    // ====================================================================
                    // 1. ENCABEZADO DEL DOCUMENTO (Logo, Info de la Clínica, Título)
                    // ====================================================================

                    // Tabla principal para el encabezado (Logo a la izquierda, info de la empresa a la derecha)
                    PdfPTable headerTable = new PdfPTable(2);
                    headerTable.WidthPercentage = 100;
                    headerTable.SetWidths(new float[] { 1f, 3f }); // Ancho para logo y ancho para info de la empresa
                    headerTable.DefaultCell.Border = PdfPCell.NO_BORDER;
                    headerTable.SpacingAfter = 20f;

                    // Celda 1: Logo de la Patita
                    // Asegúrate de que la ruta de la imagen sea correcta y la imagen exista
                    string logoPath = Server.MapPath("~/Assets/Images/logo.png"); // <--- ¡AJUSTA ESTA RUTA!
                    if (File.Exists(logoPath))
                    {
                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                        logo.ScaleToFit(70f, 70f); // Ajustar tamaño del logo
                        PdfPCell logoCell = new PdfPCell(logo);
                        logoCell.Border = PdfPCell.NO_BORDER;
                        logoCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        logoCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        logoCell.Padding = 5;
                        headerTable.AddCell(logoCell);
                    }
                    else
                    {
                        // Si el logo no se encuentra, añadir una celda vacía o un placeholder
                        // Considera loguear este error también
                        headerTable.AddCell(new PdfPCell(new Phrase("Logo no encontrado", FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8, BaseColor.RED))) { Border = PdfPCell.NO_BORDER });
                    }

                    // Celda 2: Información de la Empresa (VetWeb)
                    PdfPCell companyInfoCell = new PdfPCell();
                    companyInfoCell.Border = PdfPCell.NO_BORDER;
                    companyInfoCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    companyInfoCell.VerticalAlignment = Element.ALIGN_TOP;
                    companyInfoCell.Padding = 5;

                    Font fontCompanyName = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(54, 80, 106)); // Color similar al encabezado de tu GridView
                    Font fontCompanyDetails = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);

                    companyInfoCell.AddElement(new Paragraph("VETWEB", fontCompanyName) { Alignment = Element.ALIGN_RIGHT });
                    companyInfoCell.AddElement(new Paragraph("Villa el Salvador, Lima, Perú", fontCompanyDetails) { Alignment = Element.ALIGN_RIGHT }); //
                    companyInfoCell.AddElement(new Paragraph("Teléfono: +51 907377938", fontCompanyDetails) { Alignment = Element.ALIGN_RIGHT });
                    companyInfoCell.AddElement(new Paragraph("Email: info@vetweb.com", fontCompanyDetails) { Alignment = Element.ALIGN_RIGHT });

                    headerTable.AddCell(companyInfoCell);
                    doc.Add(headerTable);

                    // Título del Reporte
                    Font reportTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, BaseColor.DARK_GRAY);
                    Paragraph reportTitle = new Paragraph("REPORTE DE CLIENTES", reportTitleFont);
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

                    string filtroAplicado = string.IsNullOrEmpty(txtBuscarNombreCliente.Text.Trim()) ? "Ninguno" : txtBuscarNombreCliente.Text.Trim();
                    docDetailsTable.AddCell(new PdfPCell(new Phrase($"Filtro aplicado: \"{filtroAplicado}\"", fontDocDetails)) { Colspan = 2, Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT });

                    doc.Add(docDetailsTable);

                    // ====================================================================
                    // 2. TABLA DE DATOS DE CLIENTES
                    // ====================================================================

                    // Crear la tabla PDF
                    PdfPTable pdfTable = new PdfPTable(dtClientes.Columns.Count);
                    pdfTable.WidthPercentage = 100; // Ocupar el 100% del ancho disponible
                    pdfTable.SpacingBefore = 10f; // Espacio antes de la tabla
                    pdfTable.DefaultCell.Padding = 5; // Padding de las celdas
                    pdfTable.HeaderRows = 1; // Para que el encabezado se repita en cada página

                    // Configurar anchos de columna (ajusta estos valores según tus datos reales para que no se superpongan)
                    // Las columnas son: PrimerNombre, ApellidoPaterno, ApellidoMaterno, DNI, Telefono, Direccion, Correo
                    float[] widths = new float[] { 1.5f, 1.5f, 1.5f, 1f, 1.2f, 2.5f, 2f }; // Anchos reajustados para mejor legibilidad
                    if (dtClientes.Columns.Count == widths.Length)
                    {
                        pdfTable.SetWidths(widths);
                    }
                    else
                    {
                        // Fallback si el número de columnas no coincide (distribuye equitativamente)
                        pdfTable.SetWidths(Enumerable.Repeat(1f, dtClientes.Columns.Count).ToArray());
                    }

                    // Añadir encabezados de columna
                    Font fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);
                    BaseColor headerColor = new BaseColor(54, 80, 106); // Un azul/gris oscuro, similar al de tu GridView
                    string[] headers = { "Nombre", "A. Paterno", "A. Materno", "DNI", "Teléfono", "Dirección", "Correo" }; // Nombres amigables para el encabezado

                    for (int i = 0; i < dtClientes.Columns.Count; i++)
                    {
                        PdfPCell headerCell = new PdfPCell(new Phrase(headers[i], fontHeader));
                        headerCell.BackgroundColor = headerColor;
                        headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        headerCell.Padding = 7; // Más padding para encabezados
                        headerCell.BorderColor = BaseColor.LIGHT_GRAY; // Bordes sutiles
                        pdfTable.AddCell(headerCell);
                    }

                    // Añadir filas de datos
                    Font fontCell = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.BLACK);
                    foreach (DataRow row in dtClientes.Rows)
                    {
                        for (int i = 0; i < dtClientes.Columns.Count; i++)
                        {
                            PdfPCell dataCell = new PdfPCell(new Phrase(row[i].ToString(), fontCell));
                            dataCell.HorizontalAlignment = Element.ALIGN_LEFT;
                            dataCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            dataCell.Padding = 5;
                            // Alternar color de fondo para filas para mejor legibilidad
                            if (dtClientes.Rows.IndexOf(row) % 2 == 0)
                            {
                                dataCell.BackgroundColor = new BaseColor(245, 245, 245); // Gris muy claro para alternancia
                            }
                            dataCell.BorderColor = BaseColor.LIGHT_GRAY; // Bordes sutiles
                            pdfTable.AddCell(dataCell);
                        }
                    }

                    doc.Add(pdfTable);

                    // ====================================================================
                    // 3. PIE DE PÁGINA DEL DOCUMENTO (Notas, etc.)
                    // ====================================================================
                    Font fontFooter = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9, BaseColor.GRAY);
                    Paragraph footerNote = new Paragraph("Este es un reporte interno de clientes de VetWeb.", fontFooter);
                    footerNote.Alignment = Element.ALIGN_CENTER;
                    footerNote.SpacingBefore = 20f;
                    doc.Add(footerNote);

                    Paragraph thankYouNote = new Paragraph("Generado por VetWeb - Tu solución para la gestión veterinaria.", FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.LIGHT_GRAY));
                    thankYouNote.Alignment = Element.ALIGN_CENTER;
                    doc.Add(thankYouNote);

                    doc.Close();

                    // Enviar el PDF al navegador
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=ReporteClientes.pdf");
                    Response.Buffer = true;
                    Response.Clear();
                    Response.BinaryWrite(ms.ToArray());
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                // En un ambiente de producción, aquí deberías loguear el error y mostrar un mensaje más amigable
                MostrarMensaje("Error al generar el PDF: " + ex.Message, false);
            }
            finally
            {
                // Asegurarse de que el documento se cierre incluso si hay un error en la generación
                if (doc.IsOpen())
                {
                    doc.Close();
                }
            }
        }
        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            DataTable dtClientes = new DataTable();
            string filtroAplicado = string.IsNullOrEmpty(txtBuscarNombreCliente.Text.Trim()) ? "Ninguno" : txtBuscarNombreCliente.Text.Trim();

            using (SqlConnection con = new SqlConnection(cadena))
            {
                // Obtener los datos de los clientes (aplicando el filtro de búsqueda actual si lo hay)
                string query = "SELECT PrimerNombre, ApellidoPaterno, ApellidoMaterno, DNI, Telefono, Direccion, Correo FROM Clientes";

                if (!string.IsNullOrEmpty(txtBuscarNombreCliente.Text.Trim()))
                {
                    query += " WHERE PrimerNombre LIKE '%' + @SearchTerm + '%' " +
                             "OR ApellidoPaterno LIKE '%' + @SearchTerm + '%' " +
                             "OR ApellidoMaterno LIKE '%' + @SearchTerm + '%' " +
                             "OR DNI LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY PrimerNombre, ApellidoPaterno"; // Ordenar para una visualización consistente

                SqlCommand cmd = new SqlCommand(query, con);
                if (!string.IsNullOrEmpty(txtBuscarNombreCliente.Text.Trim()))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", txtBuscarNombreCliente.Text.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                try
                {
                    con.Open();
                    da.Fill(dtClientes);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al cargar los datos para el Excel: " + ex.Message, false);
                    return;
                }
            }

            if (dtClientes.Rows.Count == 0)
            {
                MostrarMensaje("No hay datos de clientes para generar el Excel con el filtro actual.", false);
                return;
            }

            try
            {
                // Configurar la respuesta para descargar un archivo Excel
                Response.Clear();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.ms-excel"; // MIME type para Excel 97-2003
                Response.AddHeader("Content-Disposition", "attachment;filename=ReporteClientes_" + ".xls");
                Response.Charset = "UTF-8";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble()); // Para UTF-8 con BOM

                // Usar StringBuilder para construir el contenido HTML de la tabla
                StringBuilder sb = new StringBuilder();

                // Cabecera HTML para Excel (opcional pero recomendable para una mejor compatibilidad)
                sb.Append("<html xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");
                sb.Append("<head><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet>");
                sb.Append("<x:Name>Clientes</x:Name>");
                sb.Append("<x:WorksheetOptions><x:Panes></x:Panes></x:WorksheetOptions>");
                sb.Append("</x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml></head>");
                sb.Append("<body>");

                // Título del reporte en el Excel
                sb.Append("<table border='0' style='font-family: Arial; font-size: 14pt;'><tr><td colspan='7' align='center'><b>REPORTE DE CLIENTES</b></td></tr></table>");
                sb.Append("<table border='0' style='font-family: Arial; font-size: 10pt;'><tr><td colspan='7' align='left'>Fecha de Generación: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "</td></tr>");
                sb.Append("<tr><td colspan='7' align='left'>Filtro Aplicado: \"" + filtroAplicado + "\"</td></tr></table>");
                sb.Append("<br>"); // Salto de línea para separar el encabezado de la tabla de datos

                // Crear la tabla HTML para los datos
                sb.Append("<table border='1px' cellpadding='0' cellspacing='0' style='border-collapse: collapse; font-family: Arial; font-size: 10pt;'>");

                // Añadir fila de encabezados
                sb.Append("<tr style='background-color:#36506A; color:#FFFFFF;'>");
                sb.Append("<th>Primer Nombre</th>");
                sb.Append("<th>Apellido Paterno</th>");
                sb.Append("<th>Apellido Materno</th>");
                sb.Append("<th>DNI</th>");
                sb.Append("<th>Teléfono</th>");
                sb.Append("<th>Dirección</th>");
                sb.Append("<th>Correo</th>");
                sb.Append("</tr>");

                // Añadir filas de datos
                foreach (DataRow row in dtClientes.Rows)
                {
                    sb.Append("<tr>");
                    sb.Append("<td>" + Server.HtmlEncode(row["PrimerNombre"].ToString()) + "</td>");
                    sb.Append("<td>" + Server.HtmlEncode(row["ApellidoPaterno"].ToString()) + "</td>");
                    sb.Append("<td>" + Server.HtmlEncode(row["ApellidoMaterno"].ToString()) + "</td>");
                    sb.Append("<td>" + Server.HtmlEncode(row["DNI"].ToString()) + "</td>");
                    sb.Append("<td>" + Server.HtmlEncode(row["Telefono"].ToString()) + "</td>");
                    sb.Append("<td>" + Server.HtmlEncode(row["Direccion"].ToString()) + "</td>");
                    sb.Append("<td>" + Server.HtmlEncode(row["Correo"].ToString()) + "</td>");
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