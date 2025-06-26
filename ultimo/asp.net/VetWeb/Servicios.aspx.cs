using iTextSharp.text; // iTextSharp core
using iTextSharp.text.pdf; // iTextSharp PDF functionality
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO; // Necesario para MemoryStream
using System.Linq; // Necesario para Enumerable.Repeat
using System.Text.RegularExpressions; // Necesario para expresiones regulares
using System.Web.UI; // Necesario para ScriptManager
using System.Web.UI.WebControls;
using System.Text;

namespace VetWeb
{
    public partial class Servicios : System.Web.UI.Page
    {
        string cadena = ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarSubcategorias(); // Cargar el DropDownList de subcategorías
                CargarServicios();
                // Establecer el estado inicial de los botones del modal (modo agregar)
                btnActualizar.Style["display"] = "none";
                btnAgregar.Style["display"] = "inline-block";
                // Limpiar el formulario y el mensaje al cargar la página por primera vez
                LimpiarFormulario();
            }
        }

        /// <summary>
        /// Carga el DropDownList de Subcategorías desde la base de datos.
        /// </summary>
        private void CargarSubcategorias()
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT SubcategoriaID, Nombre FROM Subcategoria ORDER BY Nombre", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlSubcategorias.DataSource = dt;
                ddlSubcategorias.DataTextField = "Nombre";
                ddlSubcategorias.DataValueField = "SubcategoriaID";
                ddlSubcategorias.DataBind();
                ddlSubcategorias.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Ninguna / No aplica", "")); // Opción para no seleccionar subcategoría
            }
        }

        /// <summary>
        /// Carga los datos de servicios desde la base de datos y los enlaza al GridView,
        /// opcionalmente filtrando por nombre de servicio o nombre de subcategoría.
        /// </summary>
        /// <param name="searchTerm">Término opcional para buscar.</param>
        private void CargarServicios(string searchTerm = null)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                // Incluimos la SubcategoriaID y el Nombre de la subcategoría (usando LEFT JOIN para servicios sin subcategoría)
                string query = @"
                    SELECT 
                        S.ServicioID, 
                        S.NombreServicio, 
                        S.Precio, 
                        S.SubcategoriaID, -- Incluir SubcategoriaID para DataKeys
                        ISNULL(SC.Nombre, 'N/A') AS NombreSubcategoria 
                    FROM Servicios S 
                    LEFT JOIN Subcategoria SC ON S.SubcategoriaID = SC.SubcategoriaID";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += " WHERE S.NombreServicio LIKE '%' + @SearchTerm + '%' " +
                             "OR SC.Nombre LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY S.NombreServicio"; // Ordenar para una visualización consistente

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvServicios.DataSource = dt;
                gvServicios.DataBind();
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Agregar" en el modal.
        /// Agrega un nuevo registro de servicio a la base de datos.
        /// </summary>
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "INSERT INTO Servicios (NombreServicio, Precio, SubcategoriaID) VALUES (@NombreServicio, @Precio, @SubcategoriaID)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@NombreServicio", txtNombreServicio.Text.Trim());
                cmd.Parameters.AddWithValue("@Precio", Convert.ToDecimal(txtPrecio.Text.Trim()));

                // Capturar SubcategoriaID. Si no se seleccionó ninguna (valor vacío), será DBNull.Value.
                object subcategoriaID = string.IsNullOrEmpty(ddlSubcategorias.SelectedValue) ? (object)DBNull.Value : Convert.ToInt32(ddlSubcategorias.SelectedValue);
                cmd.Parameters.AddWithValue("@SubcategoriaID", subcategoriaID);

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    MostrarMensaje("Servicio agregado correctamente.", true);
                    successOperation = true;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE (si la hubiera, por ejemplo, NombreServicio)
                    {
                        MostrarMensaje("Error: Ya existe un servicio con el mismo nombre. Por favor, verifique.", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al agregar el servicio: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al agregar el servicio: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarServicios();
            }
            else
            {
                CargarServicios(txtBuscarServicio.Text.Trim()); // Refrescar el grid con el filtro actual
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Actualizar" en el modal.
        /// Actualiza un registro de servicio existente en la base de datos.
        /// </summary>
        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            int servicioID;
            if (!int.TryParse(hfServicioID.Value, out servicioID))
            {
                MostrarMensaje("Error: El ID del servicio no tiene un formato válido para actualizar. Por favor, intente editar de nuevo.", false);
                return;
            }

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "UPDATE Servicios SET NombreServicio=@NombreServicio, Precio=@Precio, SubcategoriaID=@SubcategoriaID WHERE ServicioID=@ServicioID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@NombreServicio", txtNombreServicio.Text.Trim());
                cmd.Parameters.AddWithValue("@Precio", Convert.ToDecimal(txtPrecio.Text.Trim()));

                // Capturar SubcategoriaID. Si no se seleccionó ninguna (valor vacío), será DBNull.Value.
                object subcategoriaID = string.IsNullOrEmpty(ddlSubcategorias.SelectedValue) ? (object)DBNull.Value : Convert.ToInt32(ddlSubcategorias.SelectedValue);
                cmd.Parameters.AddWithValue("@SubcategoriaID", subcategoriaID);
                cmd.Parameters.AddWithValue("@ServicioID", servicioID);

                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MostrarMensaje("Servicio actualizado correctamente.", true);
                        successOperation = true;
                    }
                    else
                    {
                        MostrarMensaje("No se encontró el servicio para actualizar o no hubo cambios.", false);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE
                    {
                        MostrarMensaje("Error: Ya existe un servicio con el mismo nombre. Por favor, verifique.", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al actualizar el servicio: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al actualizar el servicio: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarServicios();
            }
            else
            {
                CargarServicios(txtBuscarServicio.Text.Trim()); // Refrescar el grid con el filtro actual
            }
        }

        /// <summary>
        /// Maneja los comandos de fila del GridView (Editar, Eliminar).
        /// </summary>
        protected void gvServicios_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);

            // Obtener ServicioID y SubcategoriaID usando DataKeys para robustez
            if (gvServicios.DataKeys == null || index < 0 || index >= gvServicios.DataKeys.Count)
            {
                MostrarMensaje("Error interno: No se pudo obtener el ID del servicio. Por favor, recargue la página.", false);
                return;
            }
            int servicioID = Convert.ToInt32(gvServicios.DataKeys[index]["ServicioID"]);
            // SubcategoriaID puede ser DBNull, así que lo manejamos como object
            object subcategoriaIDObj = gvServicios.DataKeys[index]["SubcategoriaID"];
            int? subcategoriaID = (subcategoriaIDObj != DBNull.Value) ? (int?)Convert.ToInt32(subcategoriaIDObj) : null;

            GridViewRow row = gvServicios.Rows[index];

            if (e.CommandName == "Editar")
            {
                // Los índices de celdas corresponden al orden de las BoundFields VISIBLES en el ASPX:
                // [0] = NombreServicio, [1] = Precio, [2] = NombreSubcategoria
                txtNombreServicio.Text = row.Cells[0].Text.Trim();
                // Para el precio, debemos asegurarnos de obtener solo el valor numérico, eliminando símbolos de moneda
                string precioText = row.Cells[1].Text.Replace("$", "").Replace("S/.", "").Trim(); // Ejemplo: remueve $ o S/.
                txtPrecio.Text = precioText;

                // Seleccionar la subcategoría correcta
                ddlSubcategorias.ClearSelection(); // Limpiar selección previa
                if (subcategoriaID.HasValue) // Si hay un ID de subcategoría válido
                {
                    try
                    {
                        System.Web.UI.WebControls.ListItem subcategoriaItem = ddlSubcategorias.Items.FindByValue(subcategoriaID.ToString());
                        if (subcategoriaItem != null)
                        {
                            subcategoriaItem.Selected = true;
                        }
                        else
                        {
                            // Si el SubcategoriaID no se encuentra (posiblemente subcategoría eliminada),
                            // selecciona la opción por defecto y muestra una advertencia.
                            ddlSubcategorias.Items.FindByValue("").Selected = true; // Selecciona "Ninguna / No aplica"
                            MostrarMensaje("Advertencia: La subcategoría asociada a este servicio no se encontró. Por favor, seleccione una nueva.", false);
                        }
                    }
                    catch (Exception)
                    {
                        // En caso de error al intentar seleccionar, volvemos a la opción por defecto.
                        ddlSubcategorias.Items.FindByValue("").Selected = true;
                        MostrarMensaje("Advertencia: Hubo un problema al cargar la subcategoría. Seleccione una nueva.", false);
                    }
                }
                else
                {
                    ddlSubcategorias.Items.FindByValue("").Selected = true; // Selecciona "Ninguna / No aplica"
                }

                hfServicioID.Value = servicioID.ToString();

                // Cambia la visibilidad de los botones en el modal
                btnAgregar.Style["display"] = "none";
                btnActualizar.Style["display"] = "inline-block";

                // Actualiza el título del modal antes de mostrarlo
                ScriptManager.RegisterStartupScript(this, this.GetType(), "SetServicioModalTitle", "document.getElementById('servicioModalLabel').innerText = 'Editar Servicio';", true);

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
                        // Solo se mantiene la verificación para CitaServicios, ya que VentaServicios fue eliminada.

                        // Verificar si hay CitaServicios asociadas a este servicio
                        SqlCommand checkCitaCmd = new SqlCommand("SELECT COUNT(*) FROM CitaServicios WHERE ServicioID = @ServicioID", con, transaction);
                        checkCitaCmd.Parameters.AddWithValue("@ServicioID", servicioID);
                        int dependentCitas = (int)checkCitaCmd.ExecuteScalar();

                        if (dependentCitas > 0)
                        {
                            MostrarMensaje("No se puede eliminar este servicio porque está asociado a " + dependentCitas + " cita(s). Elimine o reasigne las citas primero.", false);
                            transaction.Rollback(); // Revertir si hay dependencias
                            return;
                        }

                        // Si no hay dependencias, proceder con la eliminación
                        SqlCommand cmd = new SqlCommand("DELETE FROM Servicios WHERE ServicioID = @ServicioID", con, transaction);
                        cmd.Parameters.AddWithValue("@ServicioID", servicioID);
                        cmd.ExecuteNonQuery();

                        transaction.Commit(); // Confirmar la transacción
                        MostrarMensaje("Servicio eliminado correctamente.", true);
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback(); // Revertir en caso de error
                        if (ex.Number == 547) // Error de clave foránea
                        {
                            MostrarMensaje("No se pudo eliminar el servicio debido a registros asociados (ej. inventario, ventas, citas). Elimine los registros asociados primero.", false);
                        }
                        else
                        {
                            MostrarMensaje("Error de base de datos al eliminar servicio: " + ex.Message, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        MostrarMensaje("Ocurrió un error inesperado al eliminar el servicio: " + ex.Message, false);
                    }
                    finally
                    {
                        con.Close();
                    }
                }
                CargarServicios(); // Refrescar el GridView después de eliminar
            }
        }

        /// <summary>
        /// Limpia los campos del formulario del modal y restablece la UI al modo "Agregar".
        /// </summary>
        private void LimpiarFormulario()
        {
            txtNombreServicio.Text = "";
            txtPrecio.Text = "";
            ddlSubcategorias.ClearSelection();
            if (ddlSubcategorias.Items.Count > 0) ddlSubcategorias.Items.FindByValue("").Selected = true;
            hfServicioID.Value = "";
            btnAgregar.Style["display"] = "inline-block";
            btnActualizar.Style["display"] = "none";

            // También restablecer el título del modal y limpiar el mensaje
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetServicioModalTitle", "document.getElementById('servicioModalLabel').innerText = 'Agregar Nuevo Servicio';", true);
            // Limpiar mensaje explícitamente sin invocar show/hide modal
            lblMensaje.Text = "";
            lblMensaje.CssClass = "";
        }

        /// <summary>
        /// Valida los campos de entrada del formulario de Servicio.
        /// </summary>
        /// <returns>True si el formulario es válido, false en caso contrario.</returns>
        private bool ValidarFormulario()
        {
            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(txtNombreServicio.Text) ||
                string.IsNullOrWhiteSpace(txtPrecio.Text))
            {
                MostrarMensaje("Por favor, complete todos los campos obligatorios: Nombre del Servicio y Precio.", false);
                return false;
            }

            // Validar Nombre del Servicio (solo letras, números, espacios y caracteres comunes como '-', '_')
            if (!Regex.IsMatch(txtNombreServicio.Text.Trim(), @"^[\p{L}\p{N}\s\-_.,]+$")) // Permite letras, números, espacios, guiones, puntos, comas
            {
                MostrarMensaje("El campo 'Nombre del Servicio' solo puede contener letras, números, espacios y los caracteres - _ . ,", false);
                return false;
            }

            // Validar Precio (número decimal válido y positivo)
            decimal precio;
            if (!decimal.TryParse(txtPrecio.Text.Trim(), System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.CurrentCulture, out precio) || precio < 0)
            {
                // Intentar con InvariantCulture si falla con CurrentCulture
                if (!decimal.TryParse(txtPrecio.Text.Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out precio) || precio < 0)
                {
                    MostrarMensaje("El Precio debe ser un número decimal válido y positivo.", false);
                    return false;
                }
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
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideServicioModalOnSuccess", "hideServicioModal();", true);
            }
            else // Esto cubre mensajes de error y la lógica de "editar" (donde el mensaje está vacío y exito es false)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowServicioModalGeneral", "showServicioModal();", true);
            }

            // Asegurar que el mensaje sea visible dentro del modal
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollToMessageServicio" + Guid.NewGuid().ToString(),
                "var modalBody = document.querySelector('#servicioModal .modal-body'); if(modalBody) modalBody.scrollTop = 0;", true);
        }

        /// <summary>
        /// Maneja el evento de clic del botón de búsqueda.
        /// Filtra la lista de servicios basada en el término de búsqueda.
        /// </summary>
        protected void btnBuscarServicio_Click(object sender, EventArgs e)
        {
            CargarServicios(txtBuscarServicio.Text.Trim());
        }

        /// <summary>
        /// Maneja el evento de clic del botón de limpiar búsqueda.
        /// Limpia el término de búsqueda y recarga la lista completa de servicios.
        /// </summary>
        protected void btnLimpiarBusquedaServicio_Click(object sender, EventArgs e)
        {
            txtBuscarServicio.Text = ""; // Limpiar el textbox de búsqueda
            CargarServicios(); // Recargar todos los servicios sin filtro
        }

        protected void btnImprimirPdf_Click(object sender, EventArgs e)
        {
            DataTable dtServicios = new DataTable();

            using (SqlConnection con = new SqlConnection(cadena))
            {
                // Obtener los datos de los servicios (aplicando el filtro de búsqueda actual si lo hay)
                string query = @"
                    SELECT 
                        S.NombreServicio, 
                        S.Precio, 
                        ISNULL(SC.Nombre, 'N/A') AS NombreSubcategoria 
                    FROM Servicios S 
                    LEFT JOIN Subcategoria SC ON S.SubcategoriaID = SC.SubcategoriaID";

                if (!string.IsNullOrEmpty(txtBuscarServicio.Text.Trim()))
                {
                    query += " WHERE S.NombreServicio LIKE '%' + @SearchTerm + '%' " +
                             "OR SC.Nombre LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY S.NombreServicio";

                SqlCommand cmd = new SqlCommand(query, con);
                if (!string.IsNullOrEmpty(txtBuscarServicio.Text.Trim()))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", txtBuscarServicio.Text.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                try
                {
                    con.Open();
                    da.Fill(dtServicios);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al cargar los datos para el PDF: " + ex.Message, false);
                    return;
                }
            }

            if (dtServicios.Rows.Count == 0)
            {
                MostrarMensaje("No hay datos de servicios para generar el PDF con el filtro actual.", false);
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
                    Paragraph reportTitle = new Paragraph("REPORTE DE SERVICIOS", reportTitleFont);
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

                    string filtroAplicado = string.IsNullOrEmpty(txtBuscarServicio.Text.Trim()) ? "Ninguno" : txtBuscarServicio.Text.Trim();
                    docDetailsTable.AddCell(new PdfPCell(new Phrase($"Filtro aplicado: \"{filtroAplicado}\"", fontDocDetails)) { Colspan = 2, Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT });

                    doc.Add(docDetailsTable);

                    // ====================================================================
                    // 2. TABLA DE DATOS DE SERVICIOS
                    // ====================================================================

                    // Crear la tabla PDF
                    PdfPTable pdfTable = new PdfPTable(dtServicios.Columns.Count);
                    pdfTable.WidthPercentage = 100; // Ocupar el 100% del ancho disponible
                    pdfTable.SpacingBefore = 10f; // Espacio antes de la tabla
                    pdfTable.DefaultCell.Padding = 5; // Padding de las celdas
                    pdfTable.HeaderRows = 1; // Para que el encabezado se repita en cada página

                    // Configurar anchos de columna (ajusta estos valores según tus datos reales para que no se superpongan)
                    // Las columnas son: NombreServicio, Precio, NombreSubcategoria
                    float[] widths = new float[] { 2.5f, 1f, 2f }; // Anchos reajustados para mejor legibilidad
                    if (dtServicios.Columns.Count == widths.Length)
                    {
                        pdfTable.SetWidths(widths);
                    }
                    else
                    {
                        // Fallback si el número de columnas no coincide (distribuye equitativamente)
                        pdfTable.SetWidths(Enumerable.Repeat(1f, dtServicios.Columns.Count).ToArray());
                    }

                    // Añadir encabezados de columna
                    Font fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);
                    BaseColor headerColor = new BaseColor(54, 80, 106); // Un azul/gris oscuro, similar al de tu GridView
                    string[] headers = { "Nombre del Servicio", "Precio", "Subcategoría" }; // Nombres amigables para el encabezado

                    for (int i = 0; i < dtServicios.Columns.Count; i++)
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
                    foreach (DataRow row in dtServicios.Rows)
                    {
                        for (int i = 0; i < dtServicios.Columns.Count; i++)
                        {
                            // Formatear el precio para que aparezca con el símbolo de moneda local (S/.)
                            string cellValue = row[i].ToString();
                            if (dtServicios.Columns[i].ColumnName == "Precio")
                            {
                                if (decimal.TryParse(cellValue, out decimal price))
                                {
                                    cellValue = price.ToString("C", new System.Globalization.CultureInfo("es-PE")); // Formato de moneda para Perú
                                }
                            }

                            PdfPCell dataCell = new PdfPCell(new Phrase(cellValue, fontCell));
                            dataCell.HorizontalAlignment = Element.ALIGN_LEFT;
                            dataCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            dataCell.Padding = 5;
                            // Alternar color de fondo para filas para mejor legibilidad
                            if (dtServicios.Rows.IndexOf(row) % 2 == 0)
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
                    Paragraph footerNote = new Paragraph("Este es un reporte interno de servicios de VetWeb.", fontFooter);
                    footerNote.Alignment = Element.ALIGN_CENTER;
                    footerNote.SpacingBefore = 20f;
                    doc.Add(footerNote);

                    Paragraph thankYouNote = new Paragraph("Generado por VetWeb - Tu solución para la gestión veterinaria.", FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.LIGHT_GRAY));
                    thankYouNote.Alignment = Element.ALIGN_CENTER;
                    doc.Add(thankYouNote);

                    doc.Close();

                    // Enviar el PDF al navegador
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=ReporteServicios.pdf");
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
            DataTable dtServicios = new DataTable();
            string filtroAplicado = string.IsNullOrEmpty(txtBuscarServicio.Text.Trim()) ? "Ninguno" : txtBuscarServicio.Text.Trim();

            using (SqlConnection con = new SqlConnection(cadena))
            {
                // Obtener los datos de los servicios (aplicando el filtro de búsqueda actual si lo hay)
                string query = @"
                    SELECT
                        S.NombreServicio,
                        S.Precio,
                        ISNULL(SC.Nombre, 'N/A') AS NombreSubcategoria
                    FROM Servicios S
                    LEFT JOIN Subcategoria SC ON S.SubcategoriaID = SC.SubcategoriaID";

                if (!string.IsNullOrEmpty(txtBuscarServicio.Text.Trim()))
                {
                    query += " WHERE S.NombreServicio LIKE '%' + @SearchTerm + '%' " +
                                  "OR SC.Nombre LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY S.NombreServicio";

                SqlCommand cmd = new SqlCommand(query, con);
                if (!string.IsNullOrEmpty(txtBuscarServicio.Text.Trim()))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", txtBuscarServicio.Text.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                try
                {
                    con.Open();
                    da.Fill(dtServicios);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al cargar los datos para el Excel: " + ex.Message, false);
                    return;
                }
            }

            if (dtServicios.Rows.Count == 0)
            {
                MostrarMensaje("No hay datos de servicios para generar el Excel con el filtro actual.", false);
                return;
            }

            try
            {
                Response.Clear();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.ms-excel";
                Response.AddHeader("Content-Disposition", "attachment;filename=ReporteServicios_" + ".xls");
                Response.Charset = "UTF-8";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());

                StringBuilder sb = new StringBuilder();

                // Encabezado para la compatibilidad con Excel
                sb.Append("<html xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");
                sb.Append("<head><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet>");
                sb.Append("<x:Name>Servicios</x:Name>");
                sb.Append("<x:WorksheetOptions><x:Panes></x:Panes></x:WorksheetOptions>");
                sb.Append("</x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml></head>");
                sb.Append("<body>");

                // Título del reporte en el Excel
                sb.Append("<table border='0' style='font-family: Arial; font-size: 14pt;'><tr><td colspan='3' align='center'><b>REPORTE DE SERVICIOS</b></td></tr></table>"); // Cambiado colspan a 3 para 3 columnas
                sb.Append("<table border='0' style='font-family: Arial; font-size: 10pt;'><tr><td colspan='3' align='left'>Fecha de Generación: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "</td></tr>");
                sb.Append("<tr><td colspan='3' align='left'>Filtro Aplicado: \"" + filtroAplicado + "\"</td></tr></table>"); // Usa filtroAplicado aquí, colspan 3
                sb.Append("<br>");

                sb.Append("<table border='1px' cellpadding='0' cellspacing='0' style='border-collapse: collapse; font-family: Arial; font-size: 10pt;'>");

                // Encabezados de la tabla Excel
                sb.Append("<tr style='background-color:#36506A; color:#FFFFFF;'>");
                sb.Append("<th>Nombre del Servicio</th>");
                sb.Append("<th>Precio</th>");
                sb.Append("<th>Subcategoría</th>");
                sb.Append("</tr>");

                // Datos de la tabla Excel
                foreach (DataRow row in dtServicios.Rows)
                {
                    sb.Append("<tr>");
                    sb.Append("<td>" + Server.HtmlEncode(row["NombreServicio"].ToString()) + "</td>");
                    // Formatear el precio para Excel
                    sb.Append("<td>" + Convert.ToDecimal(row["Precio"]).ToString("F2") + "</td>"); // Formato decimal con 2 decimales
                    sb.Append("<td>" + Server.HtmlEncode(row["NombreSubcategoria"].ToString()) + "</td>");
                    sb.Append("</tr>");
                }

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