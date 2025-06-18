using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions; // Necesario para expresiones regulares
using System.Web.UI; // Necesario para ScriptManager
using System.Web.UI.WebControls;

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
                ddlCategoriasProducto.Items.Insert(0, new ListItem("Seleccione una categoría", "")); // Opción por defecto
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
                    ddlCategoriasProducto.Items.Insert(0, new ListItem("Seleccione una categoría", "")); // Asegura la opción por defecto
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
                        // **VALIDACIÓN DE DEPENDENCIAS ANTES DE ELIMINAR**
                        // Verificar si hay productos asociados a esta subcategoría
                        SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Productos WHERE SubcategoriaID = @SubcategoriaID", con, transaction);
                        checkCmd.Parameters.AddWithValue("@SubcategoriaID", subcategoriaID);
                        int dependentProductos = (int)checkCmd.ExecuteScalar();

                        if (dependentProductos > 0)
                        {
                            MostrarMensaje("No se puede eliminar esta subcategoría porque tiene " + dependentProductos + " producto(s) asociado(s). Elimine o reasigne los productos primero.", false);
                            transaction.Rollback(); // Revertir si hay dependencias
                            return; // Detener el proceso de eliminación
                        }

                        // Si no hay productos asociados, proceder con la eliminación de la subcategoría
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
                            MostrarMensaje("No se pudo eliminar la subcategoría porque tiene registros asociados (ej. productos). Elimine los registros asociados primero.", false);
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
    }
}
