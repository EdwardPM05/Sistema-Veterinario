using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO; // Necesario para MemoryStream y File
using iTextSharp.text; // Necesario para Document, Image, Font, Paragraph, BaseColor
using iTextSharp.text.pdf; // Necesario para PdfWriter, PdfPTable, PdfPCell
using System.Linq; // Necesario para Enumerable.Repeat (si se usa en la lógica de anchos)
using System.Text; // Necesario para StringBuilder

namespace VetWeb
{
    public partial class Subcategorias : System.Web.UI.Page
    {
        string cadena = ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Cargar las categorías de producto al inicio para el DropDownList
                CargarCategoriasProducto();
                // Cargar las subcategorías en el GridView
                CargarSubcategorias();
                // Establecer el estado inicial de los botones del modal (modo agregar)
                btnActualizar.Style["display"] = "none";
                btnAgregar.Style["display"] = "inline-block";
                // Limpiar el formulario y el mensaje al cargar la página por primera vez
                LimpiarFormulario();
            }
        }

        /// <summary>
        /// Carga las categorías de producto desde la base de datos y las llena en el DropDownList.
        /// </summary>
        private void CargarCategoriasProducto()
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT CategoriaProductoID, NombreCategoria FROM CategoriasProductos ORDER BY NombreCategoria", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlCategoriasProducto.DataSource = dt;
                ddlCategoriasProducto.DataTextField = "NombreCategoria";
                ddlCategoriasProducto.DataValueField = "CategoriaProductoID";
                ddlCategoriasProducto.DataBind();
                ddlCategoriasProducto.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione una categoría", "")); // Opción por defecto
            }
        }

        /// <summary>
        /// Carga los datos de subcategorías desde la base de datos y los enlaza al GridView,
        /// opcionalmente filtrando por nombre de subcategoría o nombre de categoría principal.
        /// </summary>
        /// <param name="searchTerm">Término opcional para buscar.</param>
        private void CargarSubcategorias(string searchTerm = null)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"
                    SELECT 
                        S.SubcategoriaID, 
                        S.Nombre, 
                        S.CategoriaProductoID, -- Incluir CategoriaProductoID para DataKeys
                        C.NombreCategoria 
                    FROM Subcategoria S
                    INNER JOIN CategoriasProductos C ON S.CategoriaProductoID = C.CategoriaProductoID";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += " WHERE S.Nombre LIKE '%' + @SearchTerm + '%' " +
                             "OR C.NombreCategoria LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY S.Nombre"; // Ordenar para una visualización consistente

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvSubcategorias.DataSource = dt;
                gvSubcategorias.DataBind();
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Agregar" en el modal.
        /// Agrega un nuevo registro de subcategoría a la base de datos.
        /// </summary>
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "INSERT INTO Subcategoria (Nombre, CategoriaProductoID) VALUES (@Nombre, @CategoriaProductoID)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Nombre", txtNombre.Text.Trim());
                cmd.Parameters.AddWithValue("@CategoriaProductoID", Convert.ToInt32(ddlCategoriasProducto.SelectedValue));

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    MostrarMensaje("Subcategoría agregada correctamente.", true);
                    successOperation = true;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE (ej. Nombre de Subcategoría + CategoriaProductoID ya existen)
                    {
                        MostrarMensaje("Error: Ya existe una subcategoría con el mismo nombre para esta categoría principal. Por favor, verifique.", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al agregar la subcategoría: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al agregar la subcategoría: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarSubcategorias();
            }
            else
            {
                CargarSubcategorias(txtBuscarSubcategoria.Text.Trim()); // Refrescar el grid con el filtro actual
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Actualizar" en el modal.
        /// Actualiza un registro de subcategoría existente en la base de datos.
        /// </summary>
        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            int subcategoriaID;
            if (!int.TryParse(hfSubcategoriaID.Value, out subcategoriaID))
            {
                MostrarMensaje("Error: El ID de la subcategoría no tiene un formato válido para actualizar. Por favor, intente editar de nuevo.", false);
                return;
            }

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "UPDATE Subcategoria SET Nombre=@Nombre, CategoriaProductoID=@CategoriaProductoID WHERE SubcategoriaID=@SubcategoriaID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Nombre", txtNombre.Text.Trim());
                cmd.Parameters.AddWithValue("@CategoriaProductoID", Convert.ToInt32(ddlCategoriasProducto.SelectedValue));
                cmd.Parameters.AddWithValue("@SubcategoriaID", subcategoriaID);

                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MostrarMensaje("Subcategoría actualizada correctamente.", true);
                        successOperation = true;
                    }
                    else
                    {
                        MostrarMensaje("No se encontró la subcategoría para actualizar o no hubo cambios.", false);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE
                    {
                        MostrarMensaje("Error: Ya existe una subcategoría con el mismo nombre para esta categoría principal. Por favor, verifique.", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al actualizar la subcategoría: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al actualizar la subcategoría: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarSubcategorias();
            }
            else
            {
                CargarSubcategorias(txtBuscarSubcategoria.Text.Trim()); // Refrescar el grid con el filtro actual
            }
        }

        /// <summary>
        /// Maneja los comandos de fila del GridView (Editar, Eliminar).
        /// </summary>
        protected void gvSubcategorias_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);

            // Obtener SubcategoriaID y CategoriaProductoID usando DataKeys para robustez
            if (gvSubcategorias.DataKeys == null || index < 0 || index >= gvSubcategorias.DataKeys.Count)
            {
                MostrarMensaje("Error interno: No se pudo obtener el ID de la subcategoría. Por favor, recargue la página.", false);
                return;
            }
            int subcategoriaID = Convert.ToInt32(gvSubcategorias.DataKeys[index]["SubcategoriaID"]);
            int categoriaProductoID = Convert.ToInt32(gvSubcategorias.DataKeys[index]["CategoriaProductoID"]);

            GridViewRow row = gvSubcategorias.Rows[index];

            if (e.CommandName == "Editar")
            {
                // Los índices de celdas corresponden al orden de las BoundFields VISIBLES en el ASPX:
                // [0] = Subcategoría (Nombre), [1] = Categoría Principal (NombreCategoria)
                txtNombre.Text = row.Cells[0].Text.Trim();

                // Seleccionar la categoría principal correcta usando el ID obtenido de DataKeys
                try
                {
                    ddlCategoriasProducto.SelectedValue = categoriaProductoID.ToString();
                }
                catch (Exception)
                {
                    // Si el CategoriaProductoID no se encuentra, selecciona la opción por defecto y muestra una advertencia.
                    ddlCategoriasProducto.ClearSelection();
                    ddlCategoriasProducto.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione una categoría", "")); // Asegura la opción por defecto
                    ddlCategoriasProducto.Items.FindByValue("").Selected = true;
                    MostrarMensaje("Advertencia: La categoría principal asociada a esta subcategoría no se encontró. Por favor, seleccione una nueva.", false);
                }

                hfSubcategoriaID.Value = subcategoriaID.ToString();

                // Cambia la visibilidad de los botones en el modal
                btnAgregar.Style["display"] = "none";
                btnActualizar.Style["display"] = "inline-block";

                // Actualiza el título del modal antes de mostrarlo
                ScriptManager.RegisterStartupScript(this, this.GetType(), "SetSubcategoriaModalTitle", "document.getElementById('subcategoriaModalLabel').innerText = 'Editar Subcategoría';", true);

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
                        // Se eliminó la validación para la tabla 'Productos'
                        // Si no hay dependencias, proceder con la eliminación de la subcategoría
                        SqlCommand cmd = new SqlCommand("DELETE FROM Subcategoria WHERE SubcategoriaID = @SubcategoriaID", con, transaction);
                        cmd.Parameters.AddWithValue("@SubcategoriaID", subcategoriaID);
                        cmd.ExecuteNonQuery();

                        transaction.Commit(); // Confirmar la transacción
                        MostrarMensaje("Subcategoría eliminada correctamente.", true);
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback(); // Revertir en caso de error
                        if (ex.Number == 547) // Error de clave foránea
                        {
                            // Este mensaje es genérico si hay otras tablas que referencian Subcategoria,
                            // como 'Servicios' si SubcategoriaID es una FK en Servicios.
                            MostrarMensaje("No se pudo eliminar la subcategoría porque tiene registros asociados. Elimine los registros asociados primero.", false);
                        }
                        else
                        {
                            MostrarMensaje("Error de base de datos al eliminar subcategoría: " + ex.Message, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        MostrarMensaje("Ocurrió un error inesperado al eliminar la subcategoría: " + ex.Message, false);
                    }
                    finally
                    {
                        con.Close();
                    }
                }
                CargarSubcategorias(); // Refrescar el GridView después de eliminar
            }
        }

        /// <summary>
        /// Limpia los campos del formulario del modal y restablece la UI al modo "Agregar".
        /// </summary>
        private void LimpiarFormulario()
        {
            txtNombre.Text = "";
            ddlCategoriasProducto.ClearSelection();
            if (ddlCategoriasProducto.Items.Count > 0) ddlCategoriasProducto.Items.FindByValue("").Selected = true;
            hfSubcategoriaID.Value = "";
            btnAgregar.Style["display"] = "inline-block";
            btnActualizar.Style["display"] = "none";

            // También restablecer el título del modal y limpiar el mensaje
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetSubcategoriaModalTitle", "document.getElementById('subcategoriaModalLabel').innerText = 'Agregar Nueva Subcategoría';", true);
            // Limpiar mensaje explícitamente sin invocar show/hide modal
            lblMensaje.Text = "";
            lblMensaje.CssClass = "";
        }

        /// <summary>
        /// Valida los campos de entrada del formulario de Subcategoría.
        /// </summary>
        /// <returns>True si el formulario es válido, false en caso contrario.</returns>
        private bool ValidarFormulario()
        {
            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrEmpty(ddlCategoriasProducto.SelectedValue) || ddlCategoriasProducto.SelectedValue == "")
            {
                MostrarMensaje("Por favor, ingrese el nombre de la subcategoría y seleccione una categoría principal.", false);
                return false;
            }

            // Validar Nombre de la Subcategoría (solo letras y espacios, incluyendo tildes y 'ñ')
            if (!Regex.IsMatch(txtNombre.Text.Trim(), @"^[\p{L}\s]+$"))
            {
                MostrarMensaje("El campo 'Nombre' solo puede contener letras y espacios.", false);
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
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideSubcategoriaModalOnSuccess", "hideSubcategoriaModal();", true);
            }
            else // Esto cubre mensajes de error y la lógica de "editar" (donde el mensaje está vacío y exito es false)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowSubcategoriaModalGeneral", "showSubcategoriaModal();", true);
            }

            // Asegurar que el mensaje sea visible dentro del modal
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollToMessageSubcategoria" + Guid.NewGuid().ToString(),
                "var modalBody = document.querySelector('#subcategoriaModal .modal-body'); if(modalBody) modalBody.scrollTop = 0;", true);
        }

        // Asegúrate de que este botón btnImprimirPdf exista en tu archivo .aspx
        protected void btnImprimirPdf_Click(object sender, EventArgs e)
        {
            DataTable dtSubcategorias = new DataTable();

            using (SqlConnection con = new SqlConnection(cadena))
            {
                // Obtener los datos de las subcategorías (aplicando el filtro de búsqueda actual si lo hay)
                string query = @"
            SELECT
                S.Nombre AS NombreSubcategoria,
                C.NombreCategoria
            FROM Subcategoria S
            INNER JOIN CategoriasProductos C ON S.CategoriaProductoID = C.CategoriaProductoID";

                if (!string.IsNullOrEmpty(txtBuscarSubcategoria.Text.Trim()))
                {
                    query += " WHERE S.Nombre LIKE '%' + @SearchTerm + '%' " +
                             "OR C.NombreCategoria LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY S.Nombre";

                SqlCommand cmd = new SqlCommand(query, con);
                if (!string.IsNullOrEmpty(txtBuscarSubcategoria.Text.Trim()))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", txtBuscarSubcategoria.Text.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                try
                {
                    con.Open();
                    da.Fill(dtSubcategorias);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al cargar los datos para el PDF: " + ex.Message, false);
                    return;
                }
            }

            if (dtSubcategorias.Rows.Count == 0)
            {
                MostrarMensaje("No hay datos de subcategorías para generar el PDF con el filtro actual.", false);
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
                    string logoPath = Server.MapPath("~/Assets/Images/logo.png"); // <--- ¡VERIFICA ESTA RUTA!
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
                    companyInfoCell.AddElement(new Paragraph("Villa el Salvador, Lima, Perú", fontCompanyDetails) { Alignment = Element.ALIGN_RIGHT });
                    companyInfoCell.AddElement(new Paragraph("Teléfono: +51 907377938", fontCompanyDetails) { Alignment = Element.ALIGN_RIGHT });
                    companyInfoCell.AddElement(new Paragraph("Email: info@vetweb.com", fontCompanyDetails) { Alignment = Element.ALIGN_RIGHT });

                    headerTable.AddCell(companyInfoCell);
                    doc.Add(headerTable);

                    // Título del Reporte
                    Font reportTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, BaseColor.DARK_GRAY);
                    Paragraph reportTitle = new Paragraph("REPORTE DE SUBCATEGORÍAS", reportTitleFont);
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

                    string filtroAplicado = string.IsNullOrEmpty(txtBuscarSubcategoria.Text.Trim()) ? "Ninguno" : txtBuscarSubcategoria.Text.Trim();
                    docDetailsTable.AddCell(new PdfPCell(new Phrase($"Filtro aplicado: \"{filtroAplicado}\"", fontDocDetails)) { Colspan = 2, Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT });

                    doc.Add(docDetailsTable);

                    // ====================================================================
                    // 2. TABLA DE DATOS DE SUBCATEGORÍAS
                    // ====================================================================

                    // Crear la tabla PDF
                    PdfPTable pdfTable = new PdfPTable(dtSubcategorias.Columns.Count);
                    pdfTable.WidthPercentage = 100; // Ocupar el 100% del ancho disponible
                    pdfTable.SpacingBefore = 10f; // Espacio antes de la tabla
                    pdfTable.DefaultCell.Padding = 5; // Padding de las celdas
                    pdfTable.HeaderRows = 1; // Para que el encabezado se repita en cada página

                    // Configurar anchos de columna (ajusta estos valores según tus datos reales para que no se superpongan)
                    // Las columnas son: NombreSubcategoria, NombreCategoria
                    float[] widths = new float[] { 2f, 2f }; // Anchos ajustados para subcategoría y categoría principal
                    if (dtSubcategorias.Columns.Count == widths.Length)
                    {
                        pdfTable.SetWidths(widths);
                    }
                    else
                    {
                        // Fallback si el número de columnas no coincide (distribuye equitativamente)
                        pdfTable.SetWidths(Enumerable.Repeat(1f, dtSubcategorias.Columns.Count).ToArray());
                    }

                    // Añadir encabezados de columna
                    Font fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);
                    BaseColor headerColor = new BaseColor(54, 80, 106); // Un azul/gris oscuro, similar al de tu GridView
                    string[] headers = { "Nombre de Subcategoría", "Categoría Principal" }; // Nombres amigables para el encabezado

                    for (int i = 0; i < dtSubcategorias.Columns.Count; i++)
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
                    foreach (DataRow row in dtSubcategorias.Rows)
                    {
                        for (int i = 0; i < dtSubcategorias.Columns.Count; i++)
                        {
                            PdfPCell dataCell = new PdfPCell(new Phrase(row[i].ToString(), fontCell));
                            dataCell.HorizontalAlignment = Element.ALIGN_LEFT;
                            dataCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            dataCell.Padding = 5;
                            // Alternar color de fondo para filas para mejor legibilidad
                            if (dtSubcategorias.Rows.IndexOf(row) % 2 == 0)
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
                    Paragraph footerNote = new Paragraph("Este es un reporte interno de subcategorías de VetWeb.", fontFooter);
                    footerNote.Alignment = Element.ALIGN_CENTER;
                    footerNote.SpacingBefore = 20f;
                    doc.Add(footerNote);

                    Paragraph thankYouNote = new Paragraph("Generado por VetWeb - Tu solución para la gestión veterinaria.", FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.LIGHT_GRAY));
                    thankYouNote.Alignment = Element.ALIGN_CENTER;
                    doc.Add(thankYouNote);

                    doc.Close();

                    // Enviar el PDF al navegador
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=ReporteSubcategorias.pdf");
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
        /// <summary>
        /// Maneja el evento de clic del botón de búsqueda.
        /// Filtra la lista de subcategorías basada en el término de búsqueda.
        /// </summary>
        protected void btnBuscarSubcategoria_Click(object sender, EventArgs e)
        {
            CargarSubcategorias(txtBuscarSubcategoria.Text.Trim());
        }

        /// <summary>
        /// Maneja el evento de clic del botón de limpiar búsqueda.
        /// Limpia el término de búsqueda y recarga la lista completa de subcategorías.
        /// </summary>
        protected void btnLimpiarBusquedaSubcategoria_Click(object sender, EventArgs e)
        {
            txtBuscarSubcategoria.Text = ""; // Limpiar el textbox de búsqueda
            CargarSubcategorias(); // Recargar todas las subcategorías sin filtro
        }
        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            DataTable dtSubcategorias = new DataTable();
            string filtroAplicado = string.IsNullOrEmpty(txtBuscarSubcategoria.Text.Trim()) ? "Ninguno" : txtBuscarSubcategoria.Text.Trim(); // <--- AÑADE ESTA LÍNEA

            using (SqlConnection con = new SqlConnection(cadena))
            {
                // Obtener los datos de las subcategorías (aplicando el filtro de búsqueda actual si lo hay)
                string query = @"
                    SELECT
                        S.Nombre AS NombreSubcategoria,
                        C.NombreCategoria
                    FROM Subcategoria S
                    INNER JOIN CategoriasProductos C ON S.CategoriaProductoID = C.CategoriaProductoID";

                if (!string.IsNullOrEmpty(txtBuscarSubcategoria.Text.Trim()))
                {
                    query += " WHERE S.Nombre LIKE '%' + @SearchTerm + '%' " +
                             "OR C.NombreCategoria LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY S.Nombre";

                SqlCommand cmd = new SqlCommand(query, con);
                if (!string.IsNullOrEmpty(txtBuscarSubcategoria.Text.Trim()))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", txtBuscarSubcategoria.Text.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                try
                {
                    con.Open();
                    da.Fill(dtSubcategorias);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al cargar los datos para el Excel: " + ex.Message, false);
                    return;
                }
            }

            if (dtSubcategorias.Rows.Count == 0)
            {
                MostrarMensaje("No hay datos de subcategorías para generar el Excel con el filtro actual.", false);
                return;
            }

            try
            {
                Response.Clear();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.ms-excel";
                Response.AddHeader("Content-Disposition", "attachment;filename=ReporteSubcategorias_" + ".xls");
                Response.Charset = "UTF-8";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());

                StringBuilder sb = new StringBuilder();

                sb.Append("<html xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");
                sb.Append("<head><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet>");
                sb.Append("<x:Name>Subcategorias</x:Name>");
                sb.Append("<x:WorksheetOptions><x:Panes></x:Panes></x:WorksheetOptions>");
                sb.Append("</x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml></head>");
                sb.Append("<body>");

                // Título del reporte en el Excel
                sb.Append("<table border='0' style='font-family: Arial; font-size: 14pt;'><tr><td colspan='" + dtSubcategorias.Columns.Count + "' align='center'><b>REPORTE DE SUBCATEGORÍAS</b></td></tr></table>");
                sb.Append("<table border='0' style='font-family: Arial; font-size: 10pt;'><tr><td colspan='" + dtSubcategorias.Columns.Count + "' align='left'>Fecha de Generación: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "</td></tr>");
                sb.Append("<tr><td colspan='" + dtSubcategorias.Columns.Count + "' align='left'>Filtro Aplicado: \"" + filtroAplicado + "\"</td></tr></table>"); // Usa filtroAplicado aquí
                sb.Append("<br>");

                sb.Append("<table border='1px' cellpadding='0' cellspacing='0' style='border-collapse: collapse; font-family: Arial; font-size: 10pt;'>");

                sb.Append("<tr style='background-color:#36506A; color:#FFFFFF;'>");
                sb.Append("<th>Nombre de Subcategoría</th>");
                sb.Append("<th>Categoría Principal</th>");
                sb.Append("</tr>");

                foreach (DataRow row in dtSubcategorias.Rows)
                {
                    sb.Append("<tr>");
                    sb.Append("<td>" + Server.HtmlEncode(row["NombreSubcategoria"].ToString()) + "</td>");
                    sb.Append("<td>" + Server.HtmlEncode(row["NombreCategoria"].ToString()) + "</td>");
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