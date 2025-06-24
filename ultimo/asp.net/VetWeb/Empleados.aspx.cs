using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions; // Necesario para expresiones regulares
using System.Web.UI; // Necesario para ScriptManager
using System.Web.UI.WebControls;

namespace VetWeb
{
    public partial class Empleados : System.Web.UI.Page
    {
        string cadena = ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarRoles(); // Primero cargar los roles para el DropDownList
                CargarEmpleados();
                // Estado inicial de los botones del modal (modo agregar)
                btnActualizar.Style["display"] = "none";
                btnAgregar.Style["display"] = "inline-block";
            }
        }

        /// <summary>
        /// Carga los roles desde la base de datos y los llena en el DropDownList de Roles.
        /// </summary>
        private void CargarRoles()
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT RolID, NombreRol FROM Roles ORDER BY NombreRol", con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Limpiar para asegurar que no haya duplicados si se llama inesperadamente
                ddlRoles.ClearSelection(); // Borra cualquier selección previa
                ddlRoles.Items.Clear();    // Borra todos los items existentes

                ddlRoles.DataSource = dt;
                ddlRoles.DataTextField = "NombreRol";
                ddlRoles.DataValueField = "RolID";
                ddlRoles.DataBind();

                // Solo inserta la opción "Seleccione un rol" si no está ya presente
                if (ddlRoles.Items.FindByValue("") == null) // Buscar por el valor vacío
                {
                    ddlRoles.Items.Insert(0, new ListItem("Seleccione un rol", "")); // Opción por defecto
                }
                // Asegurarse de que la opción por defecto esté seleccionada al inicio
                ddlRoles.Items.FindByValue("").Selected = true;
            }
        }

        /// <summary>
        /// Carga los datos de empleados desde la base de datos y los enlaza al GridView,
        /// opcionalmente filtrando por nombre, DNI o nombre de rol.
        /// </summary>
        /// <param name="searchTerm">Término opcional para buscar en PrimerNombre, ApellidoPaterno, DNI o NombreRol.</param>
        private void CargarEmpleados(string searchTerm = null)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"
                    SELECT 
                        E.EmpleadoID, 
                        E.PrimerNombre, 
                        E.ApellidoPaterno, 
                        E.ApellidoMaterno, 
                        E.DNI, 
                        E.Correo, 
                        E.Telefono, 
                        R.RolID, 
                        R.NombreRol 
                    FROM Empleados E
                    INNER JOIN Roles R ON E.RolID = R.RolID";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += " WHERE E.PrimerNombre LIKE '%' + @SearchTerm + '%' " +
                             "OR E.ApellidoPaterno LIKE '%' + @SearchTerm + '%' " +
                             "OR E.ApellidoMaterno LIKE '%' + @SearchTerm + '%' " +
                             "OR E.DNI LIKE '%' + @SearchTerm + '%' " +
                             "OR R.NombreRol LIKE '%' + @SearchTerm + '%'"; // Se añadió la búsqueda por NombreRol
                }
                query += " ORDER BY E.PrimerNombre"; // Ordenar para una visualización consistente

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvEmpleados.DataSource = dt;
                gvEmpleados.DataBind();
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Agregar" en el modal.
        /// Agrega un nuevo registro de empleado a la base de datos.
        /// </summary>
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "INSERT INTO Empleados (PrimerNombre, ApellidoPaterno, ApellidoMaterno, DNI, Correo, Telefono, RolID) " +
                               "VALUES (@PrimerNombre, @ApellidoPaterno, @ApellidoMaterno, @DNI, @Correo, @Telefono, @RolID)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PrimerNombre", txtPrimerNombre.Text.Trim());
                cmd.Parameters.AddWithValue("@ApellidoPaterno", txtApellidoPaterno.Text.Trim());
                cmd.Parameters.AddWithValue("@ApellidoMaterno", string.IsNullOrWhiteSpace(txtApellidoMaterno.Text) ? (object)DBNull.Value : txtApellidoMaterno.Text.Trim());
                cmd.Parameters.AddWithValue("@DNI", txtDNI.Text.Trim());
                cmd.Parameters.AddWithValue("@Correo", string.IsNullOrWhiteSpace(txtCorreo.Text) ? (object)DBNull.Value : txtCorreo.Text.Trim());
                cmd.Parameters.AddWithValue("@Telefono", string.IsNullOrWhiteSpace(txtTelefono.Text) ? (object)DBNull.Value : txtTelefono.Text.Trim());
                cmd.Parameters.AddWithValue("@RolID", Convert.ToInt32(ddlRoles.SelectedValue));

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    MostrarMensaje("Empleado agregado correctamente.", true);
                    successOperation = true;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE (ej. DNI o Correo ya existen si tienen unique constraint)
                    {
                        if (ex.Message.Contains("IX_Empleados_DNI")) // Asumiendo un índice único en DNI
                        {
                            MostrarMensaje("Error: Ya existe un empleado con el DNI ingresado. Por favor, verifique.", false);
                        }
                        else if (ex.Message.Contains("IX_Empleados_Correo")) // Asumiendo un índice único en Correo
                        {
                            MostrarMensaje("Error: Ya existe un empleado con el correo electrónico ingresado. Por favor, verifique.", false);
                        }
                        else
                        {
                            MostrarMensaje("Error de base de datos al agregar empleado: Ya existe un registro con datos duplicados. " + ex.Message, false);
                        }
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al agregar el empleado: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al agregar el empleado: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarEmpleados();
                // El modal se ocultará en MostrarMensaje()
            }
            else
            {
                // Si hay un error, el modal permanece abierto y los datos se conservan.
                CargarEmpleados(txtBuscarNombreEmpleado.Text.Trim()); // Refrescar el grid con el filtro actual
                // El modal se mantendrá abierto por MostrarMensaje()
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Actualizar" en el modal.
        /// Actualiza un registro de empleado existente en la base de datos.
        /// </summary>
        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            int empleadoID;
            if (!int.TryParse(hfEmpleadoID.Value, out empleadoID))
            {
                MostrarMensaje("Error: El ID del empleado no tiene un formato válido para actualizar. Por favor, intente editar de nuevo.", false);
                return;
            }

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "UPDATE Empleados SET PrimerNombre=@PrimerNombre, ApellidoPaterno=@ApellidoPaterno, ApellidoMaterno=@ApellidoMaterno, " +
                               "DNI=@DNI, Correo=@Correo, Telefono=@Telefono, RolID=@RolID WHERE EmpleadoID=@EmpleadoID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PrimerNombre", txtPrimerNombre.Text.Trim());
                cmd.Parameters.AddWithValue("@ApellidoPaterno", txtApellidoPaterno.Text.Trim());
                cmd.Parameters.AddWithValue("@ApellidoMaterno", string.IsNullOrWhiteSpace(txtApellidoMaterno.Text) ? (object)DBNull.Value : txtApellidoMaterno.Text.Trim());
                cmd.Parameters.AddWithValue("@DNI", txtDNI.Text.Trim());
                cmd.Parameters.AddWithValue("@Correo", string.IsNullOrWhiteSpace(txtCorreo.Text) ? (object)DBNull.Value : txtCorreo.Text.Trim());
                cmd.Parameters.AddWithValue("@Telefono", string.IsNullOrWhiteSpace(txtTelefono.Text) ? (object)DBNull.Value : txtTelefono.Text.Trim());
                cmd.Parameters.AddWithValue("@RolID", Convert.ToInt32(ddlRoles.SelectedValue));
                cmd.Parameters.AddWithValue("@EmpleadoID", empleadoID);

                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MostrarMensaje("Empleado actualizado correctamente.", true);
                        successOperation = true;
                    }
                    else
                    {
                        MostrarMensaje("No se encontró el empleado para actualizar o no hubo cambios.", false);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE
                    {
                        if (ex.Message.Contains("IX_Empleados_DNI"))
                        {
                            MostrarMensaje("Error: Ya existe un empleado con el DNI ingresado. Por favor, verifique.", false);
                        }
                        else if (ex.Message.Contains("IX_Empleados_Correo"))
                        {
                            MostrarMensaje("Error: Ya existe un empleado con el correo electrónico ingresado. Por favor, verifique.", false);
                        }
                        else
                        {
                            MostrarMensaje("Error de base de datos al actualizar empleado: Ya existe un registro con datos duplicados. " + ex.Message, false);
                        }
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al actualizar el empleado: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al actualizar el empleado: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarEmpleados();
                // El modal se ocultará en MostrarMensaje()
            }
            else
            {
                // Si hay un error, el modal permanece abierto y los datos se conservan.
                CargarEmpleados(txtBuscarNombreEmpleado.Text.Trim()); // Refrescar el grid con el filtro actual
                // El modal se mantendrá abierto por MostrarMensaje()
            }
        }

        /// <summary>
        /// Maneja los comandos de fila del GridView (Editar, Eliminar).
        /// </summary>
        protected void gvEmpleados_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);

            // Obtener EmpleadoID usando DataKeys para robustez
            if (gvEmpleados.DataKeys == null || index < 0 || index >= gvEmpleados.DataKeys.Count)
            {
                MostrarMensaje("Error interno: No se pudo obtener el ID del empleado. Por favor, recargue la página.", false);
                return;
            }
            int empleadoID = Convert.ToInt32(gvEmpleados.DataKeys[index].Value);

            GridViewRow row = gvEmpleados.Rows[index];

            if (e.CommandName == "Editar")
            {
                // Los índices de celdas deben coincidir con el orden de las BoundFields en el ASPX
                // Si añadiste RolID como columna oculta, ajusta los índices.
                // Ejemplo: Si RolID es la 7ma columna (índice 6) y NombreRol la 8va (índice 7)
                txtPrimerNombre.Text = row.Cells[0].Text;
                txtApellidoPaterno.Text = row.Cells[1].Text;
                txtApellidoMaterno.Text = row.Cells[2].Text == "&nbsp;" ? "" : row.Cells[2].Text;
                txtDNI.Text = row.Cells[3].Text;
                txtCorreo.Text = row.Cells[4].Text == "&nbsp;" ? "" : row.Cells[4].Text;
                txtTelefono.Text = row.Cells[5].Text == "&nbsp;" ? "" : row.Cells[5].Text;

                // ***** CAMBIO CLAVE AQUÍ *****
                // Obtener el RolID de la celda oculta del GridView
                // AHORA el RolID debe estar en una celda si lo añadiste como BoundField
                // Asegúrate de que el índice sea el correcto según tu GridView.
                // Si RolID es la 7ma columna (index 6) y NombreRol la 8va (index 7)
                string rolIDString = row.Cells[7].Text; // <-- Ajusta este índice si es diferente en tu ASPX

                int rolID;
                if (int.TryParse(rolIDString, out rolID))
                {
                    // Importante: No llamar CargarRoles() aquí de nuevo, ya se cargó en Page_Load
                    // Solo intentar seleccionar el valor.
                    ListItem rolItem = ddlRoles.Items.FindByValue(rolID.ToString());
                    if (rolItem != null)
                    {
                        ddlRoles.SelectedValue = rolItem.Value;
                    }
                    else
                    {
                        // Si el RolID del empleado no se encuentra en la lista de ddlRoles.
                        // Esto indicaría un problema de datos (un RolID en Empleados que no existe en Roles)
                        ddlRoles.ClearSelection();
                        // Selecciona la opción por defecto "Seleccione un rol" (valor vacío)
                        if (ddlRoles.Items.FindByValue("") != null)
                        {
                            ddlRoles.Items.FindByValue("").Selected = true;
                        }
                        MostrarMensaje("Advertencia: El rol asociado a este empleado no se encontró en la lista de roles disponibles. Por favor, seleccione uno nuevo.", false);
                    }
                }
                else
                {
                    // Esto sucede si row.Cells[X].Text no es un número válido.
                    ddlRoles.ClearSelection();
                    if (ddlRoles.Items.FindByValue("") != null)
                    {
                        ddlRoles.Items.FindByValue("").Selected = true;
                    }
                    MostrarMensaje("Advertencia: El ID del rol del empleado no es válido. Por favor, seleccione uno nuevo.", false);
                }

                hfEmpleadoID.Value = empleadoID.ToString();

                btnAgregar.Style["display"] = "none";
                btnActualizar.Style["display"] = "inline-block";

                ScriptManager.RegisterStartupScript(this, this.GetType(), "SetEmpleadoModalTitle", "document.getElementById('empleadoModalLabel').innerText = 'Editar Empleado';", true);
                MostrarMensaje("", false); // Limpia mensajes antes de mostrar el modal
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowEmpleadoModalScript", "showEmpleadoModal();", true); // Mostrar modal
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
                        // Un empleado no debe eliminarse si tiene citas asociadas.
                        SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Citas WHERE EmpleadoID = @EmpleadoID", con, transaction);
                        checkCmd.Parameters.AddWithValue("@EmpleadoID", empleadoID);
                        int dependentCitas = (int)checkCmd.ExecuteScalar();

                        if (dependentCitas > 0)
                        {
                            MostrarMensaje("No se puede eliminar este empleado porque tiene " + dependentCitas + " cita(s) programada(s). Reasigne o elimine las citas primero.", false);
                            transaction.Rollback(); // Revertir si hay dependencias
                            return; // Detener el proceso de eliminación
                        }

                        // Si no hay citas asociadas, proceder con la eliminación
                        SqlCommand cmd = new SqlCommand("DELETE FROM Empleados WHERE EmpleadoID = @EmpleadoID", con, transaction);
                        cmd.Parameters.AddWithValue("@EmpleadoID", empleadoID);
                        cmd.ExecuteNonQuery();

                        transaction.Commit(); // Confirmar la transacción
                        MostrarMensaje("Empleado eliminado correctamente.", true);
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback(); // Revertir en caso de error
                        if (ex.Number == 547) // Error de clave foránea genérico
                        {
                            MostrarMensaje("No se pudo eliminar el empleado porque tiene registros asociados (ej. citas). Elimine los registros asociados primero.", false);
                        }
                        else
                        {
                            MostrarMensaje("Error de base de datos al eliminar empleado: " + ex.Message, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        MostrarMensaje("Ocurrió un error inesperado al eliminar el empleado: " + ex.Message, false);
                    }
                    finally
                    {
                        con.Close();
                    }
                }
                CargarEmpleados(); // Refrescar el GridView después de eliminar
            }
        }

        /// <summary>
        /// Limpia los campos del formulario del modal y restablece la UI al modo "Agregar".
        /// </summary>
        private void LimpiarFormulario()
        {
            txtPrimerNombre.Text = "";
            txtApellidoPaterno.Text = "";
            txtApellidoMaterno.Text = "";
            txtDNI.Text = "";
            txtCorreo.Text = "";
            txtTelefono.Text = "";
            ddlRoles.ClearSelection();
            if (ddlRoles.Items.FindByValue("") != null)
            {
                ddlRoles.Items.FindByValue("").Selected = true;
            }
            else
            {
                // En un caso extremo, si no existe, re-insertarlo y seleccionarlo.
                ddlRoles.Items.Insert(0, new ListItem("Seleccione un rol", ""));
                ddlRoles.Items.FindByValue("").Selected = true;
            }

            hfEmpleadoID.Value = "";
            btnAgregar.Style["display"] = "inline-block";
            btnActualizar.Style["display"] = "none";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetEmpleadoModalTitle", "document.getElementById('empleadoModalLabel').innerText = 'Agregar Nuevo Empleado';", true);
            MostrarMensaje("", false); // Limpiar mensajes al limpiar formulario
        }

        /// <summary>
        /// Valida los campos de entrada del formulario de Empleado.
        /// </summary>
        /// <returns>True si el formulario es válido, false en caso contrario.</returns>
        private bool ValidarFormulario()
        {
            // Validar campos obligatorios y DropDownList
            if (string.IsNullOrWhiteSpace(txtPrimerNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellidoPaterno.Text) ||
                string.IsNullOrWhiteSpace(txtDNI.Text) ||
                string.IsNullOrEmpty(ddlRoles.SelectedValue) || ddlRoles.SelectedValue == "") // El rol es obligatorio
            {
                MostrarMensaje("Por favor, complete los campos obligatorios: Primer Nombre, Apellido Paterno, DNI y Rol.", false);
                return false;
            }

            // Validar Primer Nombre (solo letras y espacios)
            if (!Regex.IsMatch(txtPrimerNombre.Text.Trim(), @"^[\p{L}\s]+$"))
            {
                MostrarMensaje("El 'Primer Nombre' solo puede contener letras y espacios.", false);
                return false;
            }

            // Validar Apellido Paterno (solo letras y espacios)
            if (!Regex.IsMatch(txtApellidoPaterno.Text.Trim(), @"^[\p{L}\s]+$"))
            {
                MostrarMensaje("El 'Apellido Paterno' solo puede contener letras y espacios.", false);
                return false;
            }

            // Validar Apellido Materno (opcional, pero si tiene valor, solo letras y espacios)
            if (!string.IsNullOrWhiteSpace(txtApellidoMaterno.Text) && !Regex.IsMatch(txtApellidoMaterno.Text.Trim(), @"^[\p{L}\s]+$"))
            {
                MostrarMensaje("El 'Apellido Materno' solo puede contener letras y espacios.", false);
                return false;
            }

            // Validar DNI (8 dígitos numéricos)
            if (!Regex.IsMatch(txtDNI.Text.Trim(), @"^\d{8}$"))
            {
                MostrarMensaje("El DNI debe contener exactamente 8 dígitos numéricos.", false);
                return false;
            }

            // Validar Teléfono (9 dígitos numéricos, si no está vacío)
            if (!string.IsNullOrWhiteSpace(txtTelefono.Text) && !Regex.IsMatch(txtTelefono.Text.Trim(), @"^\d{9}$"))
            {
                MostrarMensaje("El teléfono debe contener exactamente 9 dígitos numéricos.", false);
                return false;
            }

            // Validar Correo Electrónico (formato básico de email, si no está vacío)
            if (!string.IsNullOrWhiteSpace(txtCorreo.Text) && !Regex.IsMatch(txtCorreo.Text.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MostrarMensaje("Por favor, ingrese un formato de correo electrónico válido.", false);
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
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowEmpleadoModalOnError", "showEmpleadoModal();", true);
                }
                else // Si es un mensaje de éxito
                {
                    // Ocultar el modal en caso de éxito
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideEmpleadoModalOnSuccess", "hideEmpleadoModal();", true);
                }
            }
            // Asegurar que el mensaje sea visible dentro del modal
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollToMessageEmpleado", "var modalBody = document.querySelector('#empleadoModal .modal-body'); if(modalBody) modalBody.scrollTop = 0;", true);
        }

        /// <summary>
        /// Maneja el evento de clic del botón de búsqueda.
        /// Filtra la lista de empleados basada en el término de búsqueda.
        /// </summary>
        protected void btnBuscarEmpleado_Click(object sender, EventArgs e)
        {
            CargarEmpleados(txtBuscarNombreEmpleado.Text.Trim());
        }

        /// <summary>
        /// Maneja el evento de clic del botón de limpiar búsqueda.
        /// Limpia el término de búsqueda y recarga la lista completa de empleados.
        /// </summary>
        protected void btnLimpiarBusquedaEmpleado_Click(object sender, EventArgs e)
        {
            txtBuscarNombreEmpleado.Text = ""; // Limpiar el textbox de búsqueda
            CargarEmpleados(); // Recargar todos los empleados sin filtro
        }
    }
}
