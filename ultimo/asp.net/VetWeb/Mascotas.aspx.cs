using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions; // Necesario para expresiones regulares
using System.Web.UI; // Necesario para ScriptManager
using System.Web.UI.WebControls;

namespace VetWeb
{
    public partial class Mascotas : System.Web.UI.Page
    {
        string cadena = ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Cargar los DropDownLists solo una vez al inicio
                CargarClientes();
                CargarRazas();
                CargarSexo();
                CargarMascotas();
                // Estado inicial de los botones del modal (modo agregar)
                btnActualizar.Style["display"] = "none";
                btnAgregar.Style["display"] = "inline-block";
                // Limpiar el formulario y el mensaje al cargar la página por primera vez
                LimpiarFormulario();
            }
        }

        /// <summary>
        /// Carga los clientes desde la base de datos y los llena en el DropDownList de Clientes.
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
                ddlClientes.Items.Insert(0, new ListItem("Seleccione un cliente", ""));
            }
        }

        /// <summary>
        /// Carga las razas desde la base de datos y las llena en el DropDownList de Razas.
        /// </summary>
        private void CargarRazas()
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT RazaID, NombreRaza FROM Razas ORDER BY NombreRaza", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlRazas.DataSource = dt;
                ddlRazas.DataTextField = "NombreRaza";
                ddlRazas.DataValueField = "RazaID";
                ddlRazas.DataBind();
                ddlRazas.Items.Insert(0, new ListItem("Seleccione una raza", ""));
            }
        }

        /// <summary>
        /// Carga las opciones de Sexo en el DropDownList de Sexo.
        /// CAMBIO CLAVE AQUÍ: 'Hembra' ahora tiene el valor 'H'.
        /// </summary>
        private void CargarSexo()
        {
            ddlSexo.Items.Clear(); // Limpiar antes de añadir para evitar duplicados si se llama más de una vez por error
            ddlSexo.Items.Insert(0, new ListItem("Seleccione el sexo", ""));
            ddlSexo.Items.Add(new ListItem("Macho", "M"));
            ddlSexo.Items.Add(new ListItem("Hembra", "H")); // <-- CAMBIO DE 'F' A 'H'
        }

        /// <summary>
        /// Carga los datos de mascotas desde la base de datos y los enlaza al GridView,
        /// opcionalmente filtrando por nombre de mascota, nombre de cliente o nombre de raza.
        /// </summary>
        /// <param name="searchTerm">Término opcional para buscar.</param>
        private void CargarMascotas(string searchTerm = null)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"
                    SELECT 
                        M.MascotaID, 
                        M.Nombre, 
                        M.Edad, 
                        M.Sexo, 
                        M.ClienteID,  -- Incluir ClienteID para DataKeys
                        C.PrimerNombre + ' ' + C.ApellidoPaterno AS NombreCliente, 
                        M.RazaID,     -- Incluir RazaID para DataKeys
                        R.NombreRaza 
                    FROM Mascotas M
                    INNER JOIN Clientes C ON M.ClienteID = C.ClienteID
                    INNER JOIN Razas R ON M.RazaID = R.RazaID";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += " WHERE M.Nombre LIKE '%' + @SearchTerm + '%' " +
                             "OR C.PrimerNombre LIKE '%' + @SearchTerm + '%' " +
                             "OR C.ApellidoPaterno LIKE '%' + @SearchTerm + '%' " +
                             "OR R.NombreRaza LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY M.Nombre"; // Ordenar para una visualización consistente

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvMascotas.DataSource = dt;
                gvMascotas.DataBind();
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Agregar" en el modal.
        /// Agrega un nuevo registro de mascota a la base de datos.
        /// </summary>
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "INSERT INTO Mascotas (Nombre, Edad, Sexo, ClienteID, RazaID) VALUES (@Nombre, @Edad, @Sexo, @ClienteID, @RazaID)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Nombre", txtNombreMascota.Text.Trim());
                cmd.Parameters.AddWithValue("@Edad", Convert.ToInt32(txtEdad.Text.Trim()));
                cmd.Parameters.AddWithValue("@Sexo", ddlSexo.SelectedValue); // Aquí se usa 'M' o 'H'
                cmd.Parameters.AddWithValue("@ClienteID", Convert.ToInt32(ddlClientes.SelectedValue));
                cmd.Parameters.AddWithValue("@RazaID", Convert.ToInt32(ddlRazas.SelectedValue));

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    MostrarMensaje("Mascota agregada correctamente.", true);
                    successOperation = true;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE (si la hubiera, por ejemplo, Nombre + ClienteID)
                    {
                        MostrarMensaje("Error: Ya existe una mascota con el mismo nombre para este cliente. Por favor, verifique.", false);
                    }
                    else if (ex.Message.Contains("conflicto con la restricción CHECK") && ex.Message.Contains("column 'Sexo'"))
                    {
                        MostrarMensaje("Error: El valor seleccionado para el Sexo no es válido según las reglas de la base de datos (debe ser 'M' o 'H').", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al agregar la mascota: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al agregar la mascota: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarMascotas();
                // El modal se ocultará automáticamente a través de MostrarMensaje()
            }
            else
            {
                // Si hay un error, el modal permanece abierto y los datos se conservan.
                CargarMascotas(txtBuscarNombreMascota.Text.Trim()); // Refrescar el grid con el filtro actual
                // El modal se mantendrá abierto a través de MostrarMensaje()
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Actualizar" en el modal.
        /// Actualiza un registro de mascota existente en la base de datos.
        /// </summary>
        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario()) return;

            int mascotaID;
            if (!int.TryParse(hfMascotaID.Value, out mascotaID))
            {
                MostrarMensaje("Error: El ID de la mascota no tiene un formato válido para actualizar. Por favor, intente editar de nuevo.", false);
                return;
            }

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "UPDATE Mascotas SET Nombre=@Nombre, Edad=@Edad, Sexo=@Sexo, ClienteID=@ClienteID, RazaID=@RazaID WHERE MascotaID=@MascotaID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Nombre", txtNombreMascota.Text.Trim());
                cmd.Parameters.AddWithValue("@Edad", Convert.ToInt32(txtEdad.Text.Trim()));
                cmd.Parameters.AddWithValue("@Sexo", ddlSexo.SelectedValue); // Aquí se usa 'M' o 'H'
                cmd.Parameters.AddWithValue("@ClienteID", Convert.ToInt32(ddlClientes.SelectedValue));
                cmd.Parameters.AddWithValue("@RazaID", Convert.ToInt32(ddlRazas.SelectedValue));
                cmd.Parameters.AddWithValue("@MascotaID", mascotaID);

                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MostrarMensaje("Mascota actualizada correctamente.", true);
                        successOperation = true;
                    }
                    else
                    {
                        MostrarMensaje("No se encontró la mascota para actualizar o no hubo cambios.", false);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Violación de restricción UNIQUE (si la hubiera)
                    {
                        MostrarMensaje("Error: Ya existe una mascota con el mismo nombre para este cliente. Por favor, verifique.", false);
                    }
                    else if (ex.Message.Contains("conflicto con la restricción CHECK") && ex.Message.Contains("column 'Sexo'"))
                    {
                        MostrarMensaje("Error: El valor seleccionado para el Sexo no es válido según las reglas de la base de datos (debe ser 'M' o 'H').", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al actualizar la mascota: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al actualizar la mascota: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarMascotas();
                // El modal se ocultará automáticamente a través de MostrarMensaje()
            }
            else
            {
                // Si hay un error, el modal permanece abierto y los datos se conservan.
                CargarMascotas(txtBuscarNombreMascota.Text.Trim()); // Refrescar el grid con el filtro actual
                // El modal se mantendrá abierto a través de MostrarMensaje()
            }
        }

        /// <summary>
        /// Maneja los comandos de fila del GridView (Editar, Eliminar).
        /// </summary>
        protected void gvMascotas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);

            // Obtener MascotaID, ClienteID, RazaID y Sexo usando DataKeys para robustez
            if (gvMascotas.DataKeys == null || index < 0 || index >= gvMascotas.DataKeys.Count)
            {
                MostrarMensaje("Error interno: No se pudo obtener el ID de la mascota. Por favor, recargue la página.", false);
                return;
            }
            // Accede a los IDs directamente desde DataKeys por su nombre de campo
            int mascotaID = Convert.ToInt32(gvMascotas.DataKeys[index]["MascotaID"]);
            int clienteID = Convert.ToInt32(gvMascotas.DataKeys[index]["ClienteID"]);
            int razaID = Convert.ToInt32(gvMascotas.DataKeys[index]["RazaID"]);
            string sexoValue = gvMascotas.DataKeys[index]["Sexo"].ToString(); // OBTENER SEXO DIRECTAMENTE DE DATAKEYS

            GridViewRow row = gvMascotas.Rows[index];

            if (e.CommandName == "Editar")
            {
                // Los índices de celdas corresponden al orden de las BoundFields VISIBLES en el ASPX:
                // [0] = Nombre, [1] = Edad, [2] = Sexo (Display Text), [3] = NombreCliente, [4] = NombreRaza
                txtNombreMascota.Text = row.Cells[0].Text.Trim();
                txtEdad.Text = row.Cells[1].Text.Trim();

                // Seleccionar el Sexo correcto usando el valor de DataKeys (más robusto que la celda de texto)
                ddlSexo.ClearSelection(); // Limpiar antes de seleccionar
                try
                {
                    ListItem sexoItem = ddlSexo.Items.FindByValue(sexoValue);
                    if (sexoItem != null)
                    {
                        sexoItem.Selected = true;
                    }
                    else
                    {
                        // Si el valor no se encuentra (dato inconsistente en DB), selecciona la opción por defecto.
                        ddlSexo.Items.FindByValue("").Selected = true;
                        MostrarMensaje("Advertencia: El valor del sexo no se pudo cargar correctamente. Seleccione uno nuevo.", false);
                    }
                }
                catch (Exception)
                {
                    ddlSexo.Items.FindByValue("").Selected = true;
                    MostrarMensaje("Advertencia: Error al cargar el sexo. Seleccione uno nuevo.", false);
                }

                // Seleccionar el Cliente y Raza correctos usando los IDs obtenidos de DataKeys (más robusto)
                try
                {
                    ddlClientes.SelectedValue = clienteID.ToString();
                }
                catch (Exception)
                {
                    // Si el ClienteID no se encuentra (posiblemente cliente eliminado o un problema de datos),
                    // selecciona la opción por defecto y muestra una advertencia.
                    ddlClientes.ClearSelection();
                    ddlClientes.Items.FindByValue("").Selected = true;
                    MostrarMensaje("Advertencia: El cliente asociado a esta mascota no se encontró. Por favor, seleccione uno nuevo.", false);
                }

                try
                {
                    ddlRazas.SelectedValue = razaID.ToString();
                }
                catch (Exception)
                {
                    // Si el RazaID no se encuentra (posiblemente raza eliminada o un problema de datos),
                    // selecciona la opción por defecto y muestra una advertencia.
                    ddlRazas.ClearSelection();
                    ddlRazas.Items.FindByValue("").Selected = true;
                    MostrarMensaje("Advertencia: La raza asociada a esta mascota no se encontró. Por favor, seleccione una nueva.", false);
                }

                hfMascotaID.Value = mascotaID.ToString();

                // Cambia la visibilidad de los botones en el modal
                btnAgregar.Style["display"] = "none";
                btnActualizar.Style["display"] = "inline-block";

                // Actualiza el título del modal antes de mostrarlo
                ScriptManager.RegisterStartupScript(this, this.GetType(), "SetMascotaModalTitle", "document.getElementById('mascotaModalLabel').innerText = 'Editar Mascota';", true);

                // Limpia el mensaje si lo hubiera y luego muestra el modal.
                // Esta llamada es crucial para que el modal se muestre.
                MostrarMensaje("", false); // Llama a esta función que a su vez llama showMascotaModal()
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
                        // Verificar si hay citas asociadas a esta mascota
                        SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Citas WHERE MascotaID = @MascotaID", con, transaction);
                        checkCmd.Parameters.AddWithValue("@MascotaID", mascotaID);
                        int dependentCitas = (int)checkCmd.ExecuteScalar();

                        if (dependentCitas > 0)
                        {
                            MostrarMensaje("No se puede eliminar la mascota porque tiene " + dependentCitas + " cita(s) programada(s). Reasigne o elimine las citas primero.", false);
                            transaction.Rollback(); // Revertir si hay dependencias
                            return; // Detener el proceso de eliminación
                        }

                        // Si no hay citas asociadas, proceder con la eliminación de la mascota
                        SqlCommand cmd = new SqlCommand("DELETE FROM Mascotas WHERE MascotaID = @MascotaID", con, transaction);
                        cmd.Parameters.AddWithValue("@MascotaID", mascotaID);
                        cmd.ExecuteNonQuery();

                        transaction.Commit(); // Confirmar la transacción
                        MostrarMensaje("Mascota eliminada correctamente.", true);
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback(); // Revertir en caso de error
                        if (ex.Number == 547) // Error de clave foránea
                        {
                            MostrarMensaje("No se pudo eliminar la mascota porque tiene registros asociados (ej. citas). Elimine los registros asociados primero.", false);
                        }
                        else
                        {
                            MostrarMensaje("Error de base de datos al eliminar mascota: " + ex.Message, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        MostrarMensaje("Ocurrió un error inesperado al eliminar la mascota: " + ex.Message, false);
                    }
                    finally
                    {
                        con.Close();
                    }
                }
                CargarMascotas(); // Refrescar el GridView después de eliminar
            }
        }

        /// <summary>
        /// Limpia los campos del formulario del modal y restablece la UI al modo "Agregar".
        /// </summary>
        private void LimpiarFormulario()
        {
            txtNombreMascota.Text = "";
            txtEdad.Text = "";

            // Resetear DropDownLists a su primera opción ("Seleccione...")
            ddlSexo.ClearSelection();
            if (ddlSexo.Items.Count > 0) ddlSexo.Items.FindByValue("").Selected = true;

            ddlClientes.ClearSelection();
            if (ddlClientes.Items.Count > 0) ddlClientes.Items.FindByValue("").Selected = true;

            ddlRazas.ClearSelection();
            if (ddlRazas.Items.Count > 0) ddlRazas.Items.FindByValue("").Selected = true;

            hfMascotaID.Value = "";
            btnAgregar.Style["display"] = "inline-block";
            btnActualizar.Style["display"] = "none";

            // También restablecer el título del modal y limpiar el mensaje
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetMascotaModalTitle", "document.getElementById('mascotaModalLabel').innerText = 'Agregar Nueva Mascota';", true);
            // Limpiar mensaje explícitamente sin invocar show/hide modal
            lblMensaje.Text = "";
            lblMensaje.CssClass = "";
        }

        /// <summary>
        /// Valida los campos de entrada del formulario de Mascota.
        /// </summary>
        /// <returns>True si el formulario es válido, false en caso contrario.</returns>
        private bool ValidarFormulario()
        {
            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(txtNombreMascota.Text) ||
                string.IsNullOrWhiteSpace(txtEdad.Text) ||
                string.IsNullOrEmpty(ddlSexo.SelectedValue) || ddlSexo.SelectedValue == "" ||
                string.IsNullOrEmpty(ddlClientes.SelectedValue) || ddlClientes.SelectedValue == "" ||
                string.IsNullOrEmpty(ddlRazas.SelectedValue) || ddlRazas.SelectedValue == "")
            {
                MostrarMensaje("Por favor, complete todos los campos obligatorios: Nombre, Edad, Sexo, Cliente y Raza.", false);
                return false;
            }

            // Validar Nombre de la Mascota (solo letras y espacios, incluyendo tildes y 'ñ')
            if (!Regex.IsMatch(txtNombreMascota.Text.Trim(), @"^[\p{L}\s]+$"))
            {
                MostrarMensaje("El campo 'Nombre' solo puede contener letras y espacios.", false);
                return false;
            }

            // Validar Edad (número entero válido y positivo)
            int edad;
            if (!int.TryParse(txtEdad.Text.Trim(), out edad) || edad < 0)
            {
                MostrarMensaje("La edad debe ser un número entero válido y positivo.", false);
                return false;
            }
            // Puedes añadir un límite de edad si es necesario (ej: edad < 200)

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
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideMascotaModalOnSuccess", "hideMascotaModal();", true);
            }
            else // Esto cubre mensajes de error y la lógica de "editar" (donde el mensaje está vacío y exito es false)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowMascotaModalGeneral", "showMascotaModal();", true);
            }

            // Asegurar que el mensaje sea visible dentro del modal
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollToMessageMascota" + Guid.NewGuid().ToString(),
                "var modalBody = document.querySelector('#mascotaModal .modal-body'); if(modalBody) modalBody.scrollTop = 0;", true);
        }

        /// <summary>
        /// Maneja el evento de clic del botón de búsqueda.
        /// Filtra la lista de mascotas basada en el término de búsqueda.
        /// </summary>
        protected void btnBuscarMascota_Click(object sender, EventArgs e)
        {
            CargarMascotas(txtBuscarNombreMascota.Text.Trim());
        }

        /// <summary>
        /// Maneja el evento de clic del botón de limpiar búsqueda.
        /// Limpia el término de búsqueda y recarga la lista completa de mascotas.
        /// </summary>
        protected void btnLimpiarBusquedaMascota_Click(object sender, EventArgs e)
        {
            txtBuscarNombreMascota.Text = ""; // Limpiar el textbox de búsqueda
            CargarMascotas(); // Recargar todas las mascotas sin filtro
        }
    }
}
