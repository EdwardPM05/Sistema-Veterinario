using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions; // Necesario para expresiones regulares
using System.Web.UI; // Necesario para ScriptManager
using System.Web.UI.WebControls;

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
                    // Buscar en PrimerNombre, ApellidoPaterno o DNI
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
                txtPrimerNombre.Text = row.Cells[0].Text;
                txtApellidoPaterno.Text = row.Cells[1].Text;
                txtApellidoMaterno.Text = row.Cells[2].Text;
                txtDNI.Text = row.Cells[3].Text;
                txtTelefono.Text = row.Cells[4].Text;
                txtDireccion.Text = row.Cells[5].Text;
                txtCorreo.Text = row.Cells[6].Text;
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
    }
}
