using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions; // Necesario para expresiones regulares
using System.Web.UI; // Necesario para ScriptManager
using System.Web.UI.WebControls;

namespace VetWeb
{
    public partial class CategoriasProductos : System.Web.UI.Page
    {
        string cadena = ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarCategoriasProductos();
                // Estado inicial de los botones del modal (modo agregar)
                btnActualizar.Style["display"] = "none";
                btnAgregar.Style["display"] = "inline-block";
            }
        }

        /// <summary>
        /// Carga los datos de categorías de productos desde la base de datos y los enlaza al GridView,
        /// opcionalmente filtrando por nombre de categoría.
        /// </summary>
        /// <param name="searchTerm">Término opcional para buscar en NombreCategoria.</param>
        private void CargarCategoriasProductos(string searchTerm = null)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "SELECT CategoriaProductoID, NombreCategoria FROM CategoriasProductos";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += " WHERE NombreCategoria LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY NombreCategoria"; // Ordenar para una visualización consistente

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvCategoriasProductos.DataSource = dt;
                gvCategoriasProductos.DataBind();
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Agregar" en el modal.
        /// Agrega un nuevo registro de categoría de producto a la base de datos.
        /// </summary>
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "INSERT INTO CategoriasProductos (NombreCategoria) VALUES (@NombreCategoria)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@NombreCategoria", txtNombreCategoria.Text.Trim());

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    MostrarMensaje("Categoría de producto agregada correctamente.", true);
                    successOperation = true;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE (si NombreCategoria es único)
                    {
                        MostrarMensaje("Error: Ya existe una categoría de producto con el mismo nombre. Por favor, ingrese un nombre único.", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al agregar la categoría: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al agregar la categoría: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarCategoriasProductos();
                // El modal se ocultará en MostrarMensaje()
            }
            else
            {
                // Si hay un error, el modal permanece abierto y los datos se conservan.
                CargarCategoriasProductos(txtBuscarNombreCategoria.Text.Trim()); // Refrescar el grid con el filtro actual
                // El modal se mantendrá abierto por MostrarMensaje()
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Actualizar" en el modal.
        /// Actualiza un registro de categoría de producto existente en la base de datos.
        /// </summary>
        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            int categoriaProductoID;
            if (!int.TryParse(hfCategoriaProductoID.Value, out categoriaProductoID))
            {
                MostrarMensaje("Error: El ID de la categoría de producto no tiene un formato válido para actualizar. Por favor, intente editar de nuevo.", false);
                return;
            }

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "UPDATE CategoriasProductos SET NombreCategoria=@NombreCategoria WHERE CategoriaProductoID=@CategoriaProductoID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@NombreCategoria", txtNombreCategoria.Text.Trim());
                cmd.Parameters.AddWithValue("@CategoriaProductoID", categoriaProductoID);

                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MostrarMensaje("Categoría de producto actualizada correctamente.", true);
                        successOperation = true;
                    }
                    else
                    {
                        MostrarMensaje("No se encontró la categoría de producto para actualizar o no hubo cambios.", false);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE (si NombreCategoria es único)
                    {
                        MostrarMensaje("Error: Ya existe una categoría de producto con el mismo nombre. Por favor, ingrese un nombre único.", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al actualizar la categoría: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al actualizar la categoría: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarCategoriasProductos();
                // El modal se ocultará en MostrarMensaje()
            }
            else
            {
                // Si hay un error, el modal permanece abierto y los datos se conservan.
                CargarCategoriasProductos(txtBuscarNombreCategoria.Text.Trim()); // Refrescar el grid con el filtro actual
                // El modal se mantendrá abierto por MostrarMensaje()
            }
        }

        /// <summary>
        /// Maneja los comandos de fila del GridView (Editar, Eliminar).
        /// </summary>
        protected void gvCategoriasProductos_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);

            // Obtener CategoriaProductoID usando DataKeys para robustez
            if (gvCategoriasProductos.DataKeys == null || index < 0 || index >= gvCategoriasProductos.DataKeys.Count)
            {
                MostrarMensaje("Error interno: No se pudo obtener el ID de la categoría de producto. Por favor, recargue la página.", false);
                return;
            }
            int categoriaProductoID = Convert.ToInt32(gvCategoriasProductos.DataKeys[index].Value);

            GridViewRow row = gvCategoriasProductos.Rows[index];

            if (e.CommandName == "Editar")
            {
                // NombreCategoria es la primera columna visible
                txtNombreCategoria.Text = row.Cells[0].Text;
                hfCategoriaProductoID.Value = categoriaProductoID.ToString();

                btnAgregar.Style["display"] = "none";
                btnActualizar.Style["display"] = "inline-block";

                ScriptManager.RegisterStartupScript(this, this.GetType(), "SetCategoriaProductoModalTitle", "document.getElementById('categoriaProductoModalLabel').innerText = 'Editar Categoría de Producto';", true);
                MostrarMensaje("", false); // Limpia mensajes
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCategoriaProductoModalScript", "showCategoriaProductoModal();", true); // Mostrar modal
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
                        // Una categoría de producto no debe eliminarse si hay subcategorías asociadas a ella.
                        SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Subcategoria WHERE CategoriaProductoID = @CategoriaProductoID", con, transaction);
                        checkCmd.Parameters.AddWithValue("@CategoriaProductoID", categoriaProductoID);
                        int dependentSubcategorias = (int)checkCmd.ExecuteScalar();

                        if (dependentSubcategorias > 0)
                        {
                            MostrarMensaje("No se puede eliminar esta categoría porque tiene " + dependentSubcategorias + " subcategoría(s) asociada(s). Elimine o reasigne las subcategorías primero.", false);
                            transaction.Rollback(); // Revertir si hay dependencias
                            return; // Detener el proceso de eliminación
                        }

                        // Si no hay subcategorías asociadas, proceder con la eliminación
                        SqlCommand cmd = new SqlCommand("DELETE FROM CategoriasProductos WHERE CategoriaProductoID = @CategoriaProductoID", con, transaction);
                        cmd.Parameters.AddWithValue("@CategoriaProductoID", categoriaProductoID);
                        cmd.ExecuteNonQuery();

                        transaction.Commit(); // Confirmar la transacción
                        MostrarMensaje("Categoría de producto eliminada correctamente.", true);
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback(); // Revertir en caso de error
                        if (ex.Number == 547) // Error de clave foránea genérico
                        {
                            MostrarMensaje("No se pudo eliminar la categoría porque tiene registros asociados (ej. subcategorías). Elimine los registros asociados primero.", false);
                        }
                        else
                        {
                            MostrarMensaje("Error de base de datos al eliminar categoría: " + ex.Message, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        MostrarMensaje("Ocurrió un error inesperado al eliminar la categoría: " + ex.Message, false);
                    }
                    finally
                    {
                        con.Close();
                    }
                }
                CargarCategoriasProductos(); // Refrescar el GridView después de eliminar
            }
        }

        /// <summary>
        /// Limpia los campos del formulario del modal y restablece la UI al modo "Agregar".
        /// </summary>
        private void LimpiarFormulario()
        {
            txtNombreCategoria.Text = "";
            hfCategoriaProductoID.Value = "";
            btnAgregar.Style["display"] = "inline-block";
            btnActualizar.Style["display"] = "none";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetCategoriaProductoModalTitle", "document.getElementById('categoriaProductoModalLabel').innerText = 'Agregar Nueva Categoría';", true);
            MostrarMensaje("", false); // Limpiar mensajes al limpiar formulario
        }

        /// <summary>
        /// Valida los campos de entrada del formulario de Categoría de Producto.
        /// </summary>
        /// <returns>True si el formulario es válido, false en caso contrario.</returns>
        private bool ValidarFormulario()
        {
            // Validar que el campo no esté vacío
            if (string.IsNullOrWhiteSpace(txtNombreCategoria.Text))
            {
                MostrarMensaje("Por favor, ingrese el nombre de la categoría.", false);
                return false;
            }

            // Validar NombreCategoria (solo letras y espacios, incluyendo tildes y 'ñ')
            // Asegúrate de tener 'using System.Text.RegularExpressions;' al inicio del archivo.
            if (!Regex.IsMatch(txtNombreCategoria.Text.Trim(), @"^[\p{L}\s]+$"))
            {
                MostrarMensaje("El campo 'Nombre de Categoría' solo puede contener letras y espacios.", false);
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
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCategoriaProductoModalOnError", "showCategoriaProductoModal();", true);
                }
                else // Si es un mensaje de éxito
                {
                    // Ocultar el modal en caso de éxito
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideCategoriaProductoModalOnSuccess", "hideCategoriaProductoModal();", true);
                }
            }
            // Asegurar que el mensaje sea visible dentro del modal
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollToMessageCategoriaProducto", "var modalBody = document.querySelector('#categoriaProductoModal .modal-body'); if(modalBody) modalBody.scrollTop = 0;", true);
        }


        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            DataTable dtCategorias = new DataTable();
            string filtroAplicado = string.IsNullOrEmpty(txtBuscarNombreCategoria.Text.Trim()) ? "Ninguno" : txtBuscarNombreCategoria.Text.Trim();

            using (SqlConnection con = new SqlConnection(cadena))
            {
                // Obtener los datos de las categorías de productos (aplicando el filtro de búsqueda actual si lo hay)
                string query = "SELECT NombreCategoria FROM CategoriasProductos";

                if (!string.IsNullOrEmpty(txtBuscarNombreCategoria.Text.Trim()))
                {
                    query += " WHERE NombreCategoria LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY NombreCategoria"; // Ordenar para una visualización consistente

                SqlCommand cmd = new SqlCommand(query, con);
                if (!string.IsNullOrEmpty(txtBuscarNombreCategoria.Text.Trim()))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", txtBuscarNombreCategoria.Text.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                try
                {
                    con.Open();
                    da.Fill(dtCategorias);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al cargar los datos para el Excel: " + ex.Message, false);
                    return;
                }
            }

            if (dtCategorias.Rows.Count == 0)
            {
                MostrarMensaje("No hay datos de categorías de productos para generar el Excel con el filtro actual.", false);
                return;
            }

            try
            {
                // Configurar la respuesta para descargar un archivo Excel
                Response.Clear();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.ms-excel"; // MIME type para Excel 97-2003
                Response.AddHeader("Content-Disposition", "attachment;filename=ReporteCategoriasProductos_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xls");
                Response.Charset = "UTF-8";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble()); // Para UTF-8 con BOM

                // Usar StringBuilder para construir el contenido HTML de la tabla
                StringBuilder sb = new StringBuilder();

                // Cabecera HTML para Excel (opcional pero recomendable para una mejor compatibilidad)
                sb.Append("<html xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");
                sb.Append("<head><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet>");
                sb.Append("<x:Name>CategoriasProductos</x:Name>");
                sb.Append("<x:WorksheetOptions><x:Panes></x:Panes></x:WorksheetOptions>");
                sb.Append("</x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml></head>");
                sb.Append("<body>");

                // Título del reporte en el Excel
                sb.Append("<table border='0' style='font-family: Arial; font-size: 14pt;'><tr><td colspan='1' align='center'><b>REPORTE DE CATEGORÍAS DE PRODUCTOS</b></td></tr></table>");
                sb.Append("<table border='0' style='font-family: Arial; font-size: 10pt;'><tr><td colspan='1' align='left'>Fecha de Generación: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "</td></tr>");
                sb.Append("<tr><td colspan='1' align='left'>Filtro Aplicado: \"" + filtroAplicado + "\"</td></tr></table>");
                sb.Append("<br>"); // Salto de línea para separar el encabezado de la tabla de datos

                // Crear la tabla HTML para los datos
                // Solo tenemos una columna de datos: NombreCategoria
                sb.Append("<table border='1px' cellpadding='0' cellspacing='0' style='border-collapse: collapse; font-family: Arial; font-size: 10pt;'>");

                // Añadir fila de encabezados
                sb.Append("<tr style='background-color:#36506A; color:#FFFFFF;'>");
                sb.Append("<th>Nombre de Categoría</th>"); // Solo un encabezado
                sb.Append("</tr>");

                // Añadir filas de datos
                foreach (DataRow row in dtCategorias.Rows)
                {
                    sb.Append("<tr>");
                    sb.Append("<td>" + Server.HtmlEncode(row["NombreCategoria"].ToString()) + "</td>"); // Solo una columna de datos
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
        /// <summary>
        /// Maneja el evento de clic del botón de búsqueda.
        /// Filtra la lista de categorías de productos basada en el término de búsqueda.
        /// </summary>
        protected void btnBuscarCategoria_Click(object sender, EventArgs e)
        {
            CargarCategoriasProductos(txtBuscarNombreCategoria.Text.Trim());
        }

        /// <summary>
        /// Maneja el evento de clic del botón de limpiar búsqueda.
        /// Limpia el término de búsqueda y recarga la lista completa de categorías de productos.
        /// </summary>
        protected void btnLimpiarBusquedaCategoria_Click(object sender, EventArgs e)
        {
            txtBuscarNombreCategoria.Text = ""; // Limpiar el textbox de búsqueda
            CargarCategoriasProductos(); // Recargar todas las categorías sin filtro
        }

    }
}
