using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;          // Necesario para MemoryStream
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;     // Necesario para la clase Document
using iTextSharp.text.pdf; // Necesario para PdfWriter, PdfPTable, etc.
using System.Text;


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
                    ddlRoles.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione un rol", "")); // Opción por defecto
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
                    System.Web.UI.WebControls.ListItem rolItem = ddlRoles.Items.FindByValue(rolID.ToString());
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
                ddlRoles.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione un rol", ""));
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

        protected void btnImprimirPdf_Click(object sender, EventArgs e)
        {
            DataTable dtEmpleados = new DataTable();

            using (SqlConnection con = new SqlConnection(cadena))
            {
                // Obtener los datos de los empleados (aplicando el filtro de búsqueda actual si lo hay)
                string query = @"
                    SELECT 
                        E.PrimerNombre, 
                        E.ApellidoPaterno, 
                        E.ApellidoMaterno, 
                        E.DNI, 
                        E.Telefono, 
                        E.Correo, 
                        R.NombreRol 
                    FROM Empleados E
                    INNER JOIN Roles R ON E.RolID = R.RolID";

                if (!string.IsNullOrEmpty(txtBuscarNombreEmpleado.Text.Trim()))
                {
                    query += " WHERE E.PrimerNombre LIKE '%' + @SearchTerm + '%' " +
                             "OR E.ApellidoPaterno LIKE '%' + @SearchTerm + '%' " +
                             "OR E.ApellidoMaterno LIKE '%' + @SearchTerm + '%' " +
                             "OR E.DNI LIKE '%' + @SearchTerm + '%' " +
                             "OR R.NombreRol LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY E.PrimerNombre, E.ApellidoPaterno";

                SqlCommand cmd = new SqlCommand(query, con);
                if (!string.IsNullOrEmpty(txtBuscarNombreEmpleado.Text.Trim()))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", txtBuscarNombreEmpleado.Text.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                try
                {
                    con.Open();
                    da.Fill(dtEmpleados);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al cargar los datos para el PDF: " + ex.Message, false);
                    return;
                }
            }

            if (dtEmpleados.Rows.Count == 0)
            {
                MostrarMensaje("No hay datos de empleados para generar el PDF con el filtro actual.", false);
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
                    string logoPath = Server.MapPath("~/Assets/Images/logo.png"); // <--- ¡AJUSTA ESTA RUTA SI ES DIFERENTE!
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
                    Paragraph reportTitle = new Paragraph("REPORTE DE EMPLEADOS", reportTitleFont);
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

                    string filtroAplicado = string.IsNullOrEmpty(txtBuscarNombreEmpleado.Text.Trim()) ? "Ninguno" : txtBuscarNombreEmpleado.Text.Trim();
                    docDetailsTable.AddCell(new PdfPCell(new Phrase($"Filtro aplicado: \"{filtroAplicado}\"", fontDocDetails)) { Colspan = 2, Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT });

                    doc.Add(docDetailsTable);

                    // ====================================================================
                    // 2. TABLA DE DATOS DE EMPLEADOS
                    // ====================================================================

                    // Crear la tabla PDF
                    PdfPTable pdfTable = new PdfPTable(dtEmpleados.Columns.Count);
                    pdfTable.WidthPercentage = 100; // Ocupar el 100% del ancho disponible
                    pdfTable.SpacingBefore = 10f; // Espacio antes de la tabla
                    pdfTable.DefaultCell.Padding = 5; // Padding de las celdas
                    pdfTable.HeaderRows = 1; // Para que el encabezado se repita en cada página

                    // Configurar anchos de columna (ajusta estos valores según tus datos reales para que no se superpongan)
                    // Las columnas son: PrimerNombre, ApellidoPaterno, ApellidoMaterno, DNI, Telefono, Correo, NombreRol
                    float[] widths = new float[] { 1.3f, 1.3f, 1.3f, 0.9f, 1.1f, 2f, 1.5f }; // Anchos reajustados para mejor legibilidad
                    if (dtEmpleados.Columns.Count == widths.Length)
                    {
                        pdfTable.SetWidths(widths);
                    }
                    else
                    {
                        // Fallback si el número de columnas no coincide (distribuye equitativamente)
                        pdfTable.SetWidths(Enumerable.Repeat(1f, dtEmpleados.Columns.Count).ToArray());
                    }

                    // Añadir encabezados de columna
                    Font fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);
                    BaseColor headerColor = new BaseColor(54, 80, 106); // Un azul/gris oscuro, similar al de tu GridView
                    string[] headers = { "Nombre", "A. Paterno", "A. Materno", "DNI", "Teléfono", "Correo", "Rol" }; // Nombres amigables para el encabezado

                    for (int i = 0; i < dtEmpleados.Columns.Count; i++)
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
                    foreach (DataRow row in dtEmpleados.Rows)
                    {
                        for (int i = 0; i < dtEmpleados.Columns.Count; i++)
                        {
                            PdfPCell dataCell = new PdfPCell(new Phrase(row[i].ToString(), fontCell));
                            dataCell.HorizontalAlignment = Element.ALIGN_LEFT;
                            dataCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            dataCell.Padding = 5;
                            // Alternar color de fondo para filas para mejor legibilidad
                            if (dtEmpleados.Rows.IndexOf(row) % 2 == 0)
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
                    Paragraph footerNote = new Paragraph("Este es un reporte interno de empleados de VetWeb.", fontFooter);
                    footerNote.Alignment = Element.ALIGN_CENTER;
                    footerNote.SpacingBefore = 20f;
                    doc.Add(footerNote);

                    Paragraph thankYouNote = new Paragraph("Generado por VetWeb - Tu solución para la gestión veterinaria.", FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.LIGHT_GRAY));
                    thankYouNote.Alignment = Element.ALIGN_CENTER;
                    doc.Add(thankYouNote);

                    doc.Close();

                    // Enviar el PDF al navegador
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=ReporteEmpleados.pdf");
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

        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            DataTable dtEmpleados = new DataTable();
            // Utiliza txtBuscarNombreEmpleado para el filtro de búsqueda de empleados
            string filtroAplicado = string.IsNullOrEmpty(txtBuscarNombreEmpleado.Text.Trim()) ? "Ninguno" : txtBuscarNombreEmpleado.Text.Trim();

            using (SqlConnection con = new SqlConnection(cadena))
            {
                // Obtener los datos de los empleados (aplicando el filtro de búsqueda actual si lo hay)
                // Selecciona las columnas que quieres en tu Excel
                string query = @"
                    SELECT
                        E.PrimerNombre,
                        E.ApellidoPaterno,
                        E.ApellidoMaterno,
                        E.DNI,
                        E.Correo,
                        E.Telefono,
                        R.NombreRol
                    FROM Empleados E
                    INNER JOIN Roles R ON E.RolID = R.RolID";

                if (!string.IsNullOrEmpty(txtBuscarNombreEmpleado.Text.Trim()))
                {
                    query += " WHERE E.PrimerNombre LIKE '%' + @SearchTerm + '%' " +
                                  "OR E.ApellidoPaterno LIKE '%' + @SearchTerm + '%' " +
                                  "OR E.ApellidoMaterno LIKE '%' + @SearchTerm + '%' " +
                                  "OR E.DNI LIKE '%' + @SearchTerm + '%' " +
                                  "OR R.NombreRol LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY E.PrimerNombre, E.ApellidoPaterno"; // Ordenar para una visualización consistente

                SqlCommand cmd = new SqlCommand(query, con);
                if (!string.IsNullOrEmpty(txtBuscarNombreEmpleado.Text.Trim()))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", txtBuscarNombreEmpleado.Text.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                try
                {
                    con.Open();
                    da.Fill(dtEmpleados);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al cargar los datos para el Excel: " + ex.Message, false);
                    return;
                }
            }

            if (dtEmpleados.Rows.Count == 0)
            {
                MostrarMensaje("No hay datos de empleados para generar el Excel con el filtro actual.", false);
                return;
            }

            try
            {
                // Configurar la respuesta para descargar un archivo Excel (formato HTML/XLS)
                Response.Clear();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.ms-excel"; // MIME type para Excel 97-2003
                // Nombre del archivo Excel: ReporteEmpleados_FechaHoraActual.xls
                Response.AddHeader("Content-Disposition", "attachment;filename=ReporteEmpleados_" + ".xls");
                Response.Charset = "UTF-8";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble()); // Para UTF-8 con BOM

                // Usar StringBuilder para construir el contenido HTML de la tabla
                StringBuilder sb = new StringBuilder();

                // Cabecera HTML para Excel (opcional pero recomendable para una mejor compatibilidad)
                sb.Append("<html xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");
                sb.Append("<head><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet>");
                sb.Append("<x:Name>Empleados</x:Name>"); // Nombre de la hoja en Excel
                sb.Append("<x:WorksheetOptions><x:Panes></x:Panes></x:WorksheetOptions>");
                sb.Append("</x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml></head>");
                sb.Append("<body>");

                // Título del reporte en el Excel
                // Colspan será el número de columnas que vas a exportar (7 en este caso)
                int numColumnas = dtEmpleados.Columns.Count;
                sb.Append("<table border='0' style='font-family: Arial; font-size: 14pt;'><tr><td colspan='" + numColumnas + "' align='center'><b>REPORTE DE EMPLEADOS</b></td></tr></table>");
                sb.Append("<table border='0' style='font-family: Arial; font-size: 10pt;'><tr><td colspan='" + numColumnas + "' align='left'>Fecha de Generación: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "</td></tr>");
                sb.Append("<tr><td colspan='" + numColumnas + "' align='left'>Filtro Aplicado: \"" + filtroAplicado + "\"</td></tr></table>");
                sb.Append("<br>"); // Salto de línea para separar el encabezado de la tabla de datos

                // Crear la tabla HTML para los datos de los empleados
                sb.Append("<table border='1px' cellpadding='0' cellspacing='0' style='border-collapse: collapse; font-family: Arial; font-size: 10pt;'>");

                // Añadir fila de encabezados para la tabla de datos
                sb.Append("<tr style='background-color:#36506A; color:#FFFFFF;'>");
                // Recorrer las columnas del DataTable para generar los encabezados
                foreach (DataColumn column in dtEmpleados.Columns)
                {
                    // Nombres más amigables para las columnas en el Excel
                    string headerText = column.ColumnName;
                    switch (column.ColumnName)
                    {
                        case "PrimerNombre": headerText = "Primer Nombre"; break;
                        case "ApellidoPaterno": headerText = "Apellido Paterno"; break;
                        case "ApellidoMaterno": headerText = "Apellido Materno"; break;
                        case "DNI": headerText = "DNI"; break;
                        case "Telefono": headerText = "Teléfono"; break;
                        case "Correo": headerText = "Correo Electrónico"; break;
                        case "NombreRol": headerText = "Rol"; break;
                            // Puedes añadir más casos si cambias los nombres de las columnas en la consulta SQL
                    }
                    sb.Append("<th>" + headerText + "</th>");
                }
                sb.Append("</tr>");

                // Añadir filas de datos
                foreach (DataRow row in dtEmpleados.Rows)
                {
                    sb.Append("<tr>");
                    foreach (object cellValue in row.ItemArray)
                    {
                        // Server.HtmlEncode para evitar problemas con caracteres especiales
                        sb.Append("<td>" + Server.HtmlEncode(cellValue?.ToString() ?? "") + "</td>");
                    }
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
