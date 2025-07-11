using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Reflection;
using System.Web.Caching;

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
                CargarEspecies(); // <--- NUEVO: Cargar las especies al inicio
                // CargarRazas(); // <--- COMENTADO: Las razas ahora se cargarán por especie
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
                ddlClientes.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione un cliente", ""));
            }
        }

        /// <summary>
        /// <summary>
        /// Carga las especies desde la base de datos y las llena en el DropDownList de Especies.
        /// </summary>
        private void CargarEspecies()
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT EspecieID, NombreEspecie FROM Especies ORDER BY NombreEspecie", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlEspecies.DataSource = dt;
                ddlEspecies.DataTextField = "NombreEspecie";
                ddlEspecies.DataValueField = "EspecieID";
                ddlEspecies.DataBind();
                ddlEspecies.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione una especie", ""));
            }
        }

        /// <summary>
        /// Carga las razas desde la base de datos y las llena en el DropDownList de Razas,
        /// opcionalmente filtrando por EspecieID.
        /// </summary>
        /// <param name="especieID">El ID de la especie para filtrar las razas. Si es 0 o nulo, carga todas.</param>
        private void CargarRazas(int especieID = 0)
        {
            ddlRazas.Items.Clear(); // Limpiar antes de cargar
            ddlRazas.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione una raza", ""));

            if (especieID <= 0) // Si no se selecciona una especie válida, solo dejamos la opción por defecto
            {
                ddlRazas.Enabled = false; // Deshabilitar hasta que se seleccione una especie
                return;
            }

            ddlRazas.Enabled = true; // Habilitar el ddlRazas

            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "SELECT RazaID, NombreRaza FROM Razas WHERE EspecieID = @EspecieID ORDER BY NombreRaza";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.SelectCommand.Parameters.AddWithValue("@EspecieID", especieID);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlRazas.DataSource = dt;
                ddlRazas.DataTextField = "NombreRaza";
                ddlRazas.DataValueField = "RazaID";
                ddlRazas.DataBind();

                if (dt.Rows.Count == 0)
                {
                    ddlRazas.Items.Add(new System.Web.UI.WebControls.ListItem("No hay razas para esta especie", ""));
                    ddlRazas.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Evento que se dispara cuando se selecciona una nueva especie.
        /// Carga las razas asociadas a la especie seleccionada.
        /// </summary>
        protected void ddlEspecies_SelectedIndexChanged(object sender, EventArgs e)
        {
            int especieId;
            if (int.TryParse(ddlEspecies.SelectedValue, out especieId))
            {
                CargarRazas(especieId);
            }
            else
            {
                // Si no se selecciona una especie válida, limpiar y deshabilitar razas
                CargarRazas(0);
            }
            // Mantiene el modal abierto después del postback
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowMascotaModalAfterPostBack", "showMascotaModal();", true);
        }


        /// <summary>
        /// Carga las opciones de Sexo en el DropDownList de Sexo.
        /// </summary>
        private void CargarSexo()
        {
            ddlSexo.Items.Clear();
            ddlSexo.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione el sexo", ""));
            ddlSexo.Items.Add(new System.Web.UI.WebControls.ListItem("Macho", "M"));
            ddlSexo.Items.Add(new System.Web.UI.WebControls.ListItem("Hembra", "H"));
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
                        M.ClienteID, 
                        C.PrimerNombre + ' ' + C.ApellidoPaterno AS NombreCliente, 
                        M.RazaID, 
                        R.NombreRaza,
                        R.EspecieID, -- <--- NUEVO: Incluir EspecieID para la edición
                        E.NombreEspecie -- <--- NUEVO: Incluir NombreEspecie para la edición y visualización
                    FROM Mascotas M
                    INNER JOIN Clientes C ON M.ClienteID = C.ClienteID
                    INNER JOIN Razas R ON M.RazaID = R.RazaID
                    INNER JOIN Especies E ON R.EspecieID = E.EspecieID"; // <--- NUEVO: JOIN con Especies

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += " WHERE M.Nombre LIKE '%' + @SearchTerm + '%' " +
                             "OR C.PrimerNombre LIKE '%' + @SearchTerm + '%' " +
                             "OR C.ApellidoPaterno LIKE '%' + @SearchTerm + '%' " +
                             "OR R.NombreRaza LIKE '%' + @SearchTerm + '%' " +
                             "OR E.NombreEspecie LIKE '%' + @SearchTerm + '%'"; // <--- NUEVO: Buscar también por especie
                }
                query += " ORDER BY M.Nombre";

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
            if (!ValidarFormulario())
            {
                // Si la validación falla, asegurar que el modal permanezca abierto
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalOnError", "showMascotaModal();", true);
                return;
            }

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "INSERT INTO Mascotas (Nombre, Edad, Sexo, ClienteID, RazaID) VALUES (@Nombre, @Edad, @Sexo, @ClienteID, @RazaID)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Nombre", txtNombreMascota.Text.Trim());
                cmd.Parameters.AddWithValue("@Edad", Convert.ToInt32(txtEdad.Text.Trim()));
                cmd.Parameters.AddWithValue("@Sexo", ddlSexo.SelectedValue);
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
                    if (ex.Number == 2627)
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
            }
            else
            {
                CargarMascotas(txtBuscarNombreMascota.Text.Trim());
                // Importante: Si hubo un error en el servidor (ej. duplicado),
                // y el formulario permanece abierto, necesitamos recargar las razas
                // si se había seleccionado una especie.
                int especieIdSelected;
                if (int.TryParse(ddlEspecies.SelectedValue, out especieIdSelected))
                {
                    CargarRazas(especieIdSelected);
                }
            }
        }

        /// <summary>
        /// Maneja el evento de clic del botón "Actualizar" en el modal.
        /// Actualiza un registro de mascota existente en la base de datos.
        /// </summary>
        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            if (!ValidarFormulario())
            {
                // Si la validación falla, asegurar que el modal permanezca abierto
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalOnError", "showMascotaModal();", true);
                return;
            }

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
                cmd.Parameters.AddWithValue("@Sexo", ddlSexo.SelectedValue);
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
                    if (ex.Number == 2627)
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
            }
            else
            {
                CargarMascotas(txtBuscarNombreMascota.Text.Trim());
                // Importante: Si hubo un error en el servidor,
                // y el formulario permanece abierto, necesitamos recargar las razas
                // si se había seleccionado una especie.
                int especieIdSelected;
                if (int.TryParse(ddlEspecies.SelectedValue, out especieIdSelected))
                {
                    CargarRazas(especieIdSelected);
                }
            }
        }

        /// <summary>
        /// Maneja los comandos de fila del GridView (Editar, Eliminar).
        /// </summary>
        protected void gvMascotas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);

            if (gvMascotas.DataKeys == null || index < 0 || index >= gvMascotas.DataKeys.Count)
            {
                MostrarMensaje("Error interno: No se pudo obtener el ID de la mascota. Por favor, recargue la página.", false);
                return;
            }

            int mascotaID = Convert.ToInt32(gvMascotas.DataKeys[index]["MascotaID"]);
            int clienteID = Convert.ToInt32(gvMascotas.DataKeys[index]["ClienteID"]);
            int razaID = Convert.ToInt32(gvMascotas.DataKeys[index]["RazaID"]);
            string sexoValue = gvMascotas.DataKeys[index]["Sexo"].ToString();
            // <--- NUEVO: Obtener EspecieID desde DataKeys
            int especieID = Convert.ToInt32(gvMascotas.DataKeys[index]["EspecieID"]);

            GridViewRow row = gvMascotas.Rows[index];

            if (e.CommandName == "Editar")
            {
                txtNombreMascota.Text = row.Cells[0].Text.Trim();
                txtEdad.Text = row.Cells[1].Text.Trim();

                ddlSexo.ClearSelection();
                try
                {
                    System.Web.UI.WebControls.ListItem sexoItem = ddlSexo.Items.FindByValue(sexoValue);
                    if (sexoItem != null)
                    {
                        sexoItem.Selected = true;
                    }
                    else
                    {
                        ddlSexo.Items.FindByValue("").Selected = true;
                        MostrarMensaje("Advertencia: El valor del sexo no se pudo cargar correctamente. Seleccione uno nuevo.", false);
                    }
                }
                catch (Exception)
                {
                    ddlSexo.Items.FindByValue("").Selected = true;
                    MostrarMensaje("Advertencia: Error al cargar el sexo. Seleccione uno nuevo.", false);
                }

                try
                {
                    ddlClientes.SelectedValue = clienteID.ToString();
                }
                catch (Exception)
                {
                    ddlClientes.ClearSelection();
                    ddlClientes.Items.FindByValue("").Selected = true;
                    MostrarMensaje("Advertencia: El cliente asociado a esta mascota no se encontró. Por favor, seleccione uno nuevo.", false);
                }

                // <--- NUEVO: Seleccionar la especie primero, y luego cargar las razas
                try
                {
                    ddlEspecies.SelectedValue = especieID.ToString();
                    CargarRazas(especieID); // Cargar las razas de la especie seleccionada
                    ddlRazas.SelectedValue = razaID.ToString(); // Luego seleccionar la raza específica
                }
                catch (Exception)
                {
                    ddlEspecies.ClearSelection();
                    ddlEspecies.Items.FindByValue("").Selected = true;
                    CargarRazas(0); // Limpiar y deshabilitar ddlRazas si la especie no se puede cargar
                    ddlRazas.ClearSelection(); // Asegurar que raza también esté limpia
                    ddlRazas.Items.FindByValue("").Selected = true;
                    MostrarMensaje("Advertencia: La especie o raza asociada a esta mascota no se encontró. Por favor, seleccione una nueva.", false);
                }


                hfMascotaID.Value = mascotaID.ToString();

                btnAgregar.Style["display"] = "none";
                btnActualizar.Style["display"] = "inline-block";

                ScriptManager.RegisterStartupScript(this, this.GetType(), "SetMascotaModalTitle", "document.getElementById('mascotaModalLabel').innerText = 'Editar Mascota';", true);

                MostrarMensaje("", false); // Esto mostrará el modal
            }
            else if (e.CommandName == "Eliminar")
            {
                using (SqlConnection con = new SqlConnection(cadena))
                {
                    con.Open();
                    SqlTransaction transaction = con.BeginTransaction();

                    try
                    {
                        SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Citas WHERE MascotaID = @MascotaID", con, transaction);
                        checkCmd.Parameters.AddWithValue("@MascotaID", mascotaID);
                        int dependentCitas = (int)checkCmd.ExecuteScalar();

                        if (dependentCitas > 0)
                        {
                            MostrarMensaje("No se puede eliminar la mascota porque tiene " + dependentCitas + " cita(s) programada(s). Reasigne o elimine las citas primero.", false);
                            transaction.Rollback();
                            return;
                        }

                        SqlCommand cmd = new SqlCommand("DELETE FROM Mascotas WHERE MascotaID = @MascotaID", con, transaction);
                        cmd.Parameters.AddWithValue("@MascotaID", mascotaID);
                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                        MostrarMensaje("Mascota eliminada correctamente.", true);
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        if (ex.Number == 547)
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
                CargarMascotas();
            }
        }

        /// <summary>
        /// Limpia los campos del formulario del modal y restablece la UI al modo "Agregar".
        /// </summary>
        private void LimpiarFormulario()
        {
            txtNombreMascota.Text = "";
            txtEdad.Text = "";

            ddlSexo.ClearSelection();
            if (ddlSexo.Items.Count > 0) ddlSexo.Items.FindByValue("").Selected = true;

            ddlClientes.ClearSelection();
            if (ddlClientes.Items.Count > 0) ddlClientes.Items.FindByValue("").Selected = true;

            // <--- NUEVO: Limpiar y resetear el dropdown de Especies
            ddlEspecies.ClearSelection();
            if (ddlEspecies.Items.Count > 0) ddlEspecies.Items.FindByValue("").Selected = true;

            // <--- NUEVO: Limpiar y deshabilitar el dropdown de Razas
            ddlRazas.ClearSelection();
            if (ddlRazas.Items.Count > 0) ddlRazas.Items.FindByValue("").Selected = true;
            ddlRazas.Enabled = false; // Deshabilitar hasta que se seleccione una especie

            hfMascotaID.Value = "";
            btnAgregar.Style["display"] = "inline-block";
            btnActualizar.Style["display"] = "none";

            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetMascotaModalTitle", "document.getElementById('mascotaModalLabel').innerText = 'Agregar Nueva Mascota';", true);
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
                string.IsNullOrEmpty(ddlEspecies.SelectedValue) || ddlEspecies.SelectedValue == "" || // <--- NUEVO: Validar Especie
                string.IsNullOrEmpty(ddlRazas.SelectedValue) || ddlRazas.SelectedValue == "")
            {
                MostrarMensaje("Por favor, complete todos los campos obligatorios: Nombre, Edad, Sexo, Cliente, Especie y Raza.", false);
                return false;
            }

            // Validar Nombre de la Mascota (solo letras y espacios, incluyendo tildes y 'ñ')
            if (!Regex.IsMatch(txtNombreMascota.Text.Trim(), @"^[\p{L}\s]+$"))
            {
                MostrarMensaje("El campo 'Nombre' solo puede contener letras y espacios.", false);
                return false;
            }

            int edad;
            if (!int.TryParse(txtEdad.Text.Trim(), out edad) || edad < 0 || edad > 180)
            {
                MostrarMensaje("La edad debe ser un número entero válido entre 0 y 180.", false);
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
            }

            if (exito && !string.IsNullOrEmpty(mensaje))
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideMascotaModalOnSuccess", "hideMascotaModal();", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowMascotaModalGeneral", "showMascotaModal();", true);
            }

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
            txtBuscarNombreMascota.Text = "";
            CargarMascotas();
        }

protected void btnImprimirPdf_Click(object sender, EventArgs e)
        {
            DataTable dtMascotas = new DataTable();

            using (SqlConnection con = new SqlConnection(cadena))
            {
                // Obtener los datos de las mascotas (aplicando el filtro de búsqueda actual si lo hay)
                string query = @"
                    SELECT
                        M.Nombre,
                        M.Edad,
                        CASE M.Sexo
                            WHEN 'M' THEN 'Macho'
                            WHEN 'H' THEN 'Hembra'
                            ELSE 'Desconocido'
                        END AS SexoMascota, -- Renombramos para mostrar texto legible
                        C.PrimerNombre + ' ' + C.ApellidoPaterno AS NombreCliente,
                        R.NombreRaza
                    FROM Mascotas M
                    INNER JOIN Clientes C ON M.ClienteID = C.ClienteID
                    INNER JOIN Razas R ON M.RazaID = R.RazaID";

                // Si hay un término de búsqueda aplicado, también aplicarlo al PDF
                if (!string.IsNullOrEmpty(txtBuscarNombreMascota.Text.Trim()))
                {
                    query += " WHERE M.Nombre LIKE '%' + @SearchTerm + '%' " +
                             "OR C.PrimerNombre LIKE '%' + @SearchTerm + '%' " +
                             "OR C.ApellidoPaterno LIKE '%' + @SearchTerm + '%' " +
                             "OR R.NombreRaza LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY M.Nombre";

                SqlCommand cmd = new SqlCommand(query, con);
                if (!string.IsNullOrEmpty(txtBuscarNombreMascota.Text.Trim()))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", txtBuscarNombreMascota.Text.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                try
                {
                    con.Open();
                    da.Fill(dtMascotas);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al obtener datos para el PDF: " + ex.Message, false);
                    return;
                }
            }

            if (dtMascotas.Rows.Count == 0)
            {
                MostrarMensaje("No hay mascotas para exportar a PDF con el filtro actual.", false);
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
                    Paragraph reportTitle = new Paragraph("REPORTE DE MASCOTAS", reportTitleFont);
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

                    string filtroAplicado = string.IsNullOrEmpty(txtBuscarNombreMascota.Text.Trim()) ? "Ninguno" : txtBuscarNombreMascota.Text.Trim();
                    docDetailsTable.AddCell(new PdfPCell(new Phrase($"Filtro aplicado: \"{filtroAplicado}\"", fontDocDetails)) { Colspan = 2, Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT });

                    doc.Add(docDetailsTable);

                    // ====================================================================
                    // 2. TABLA DE DATOS DE MASCOTAS
                    // ====================================================================

                    // Crear la tabla PDF
                    PdfPTable pdfTable = new PdfPTable(dtMascotas.Columns.Count);
                    pdfTable.WidthPercentage = 100; // Ocupar el 100% del ancho disponible
                    pdfTable.SpacingBefore = 10f; // Espacio antes de la tabla
                    pdfTable.DefaultCell.Padding = 5; // Padding de las celdas
                    pdfTable.HeaderRows = 1; // Para que el encabezado se repita en cada página

                    // Configurar anchos de columna (ajusta estos valores según tus datos reales para que no se superpongan)
                    // Las columnas son: Nombre, Edad, SexoMascota, NombreCliente, NombreRaza
                    float[] widths = new float[] { 1.5f, 0.8f, 1.2f, 2f, 1.5f }; // Anchos reajustados para mejor legibilidad
                    if (dtMascotas.Columns.Count == widths.Length)
                    {
                        pdfTable.SetWidths(widths);
                    }
                    else
                    {
                        // Fallback si el número de columnas no coincide (distribuye equitativamente)
                        pdfTable.SetWidths(Enumerable.Repeat(1f, dtMascotas.Columns.Count).ToArray());
                    }

                    // Añadir encabezados de columna
                    Font fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);
                    BaseColor headerColor = new BaseColor(54, 80, 106); // Un azul/gris oscuro, similar al de tu GridView
                    string[] headers = { "Nombre", "Edad", "Sexo", "Cliente", "Raza" }; // Nombres amigables para el encabezado

                    for (int i = 0; i < dtMascotas.Columns.Count; i++)
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
                    foreach (DataRow row in dtMascotas.Rows)
                    {
                        for (int i = 0; i < dtMascotas.Columns.Count; i++)
                        {
                            PdfPCell dataCell = new PdfPCell(new Phrase(row[i].ToString(), fontCell));
                            dataCell.HorizontalAlignment = Element.ALIGN_LEFT;
                            dataCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            dataCell.Padding = 5;
                            // Alternar color de fondo para filas para mejor legibilidad
                            if (dtMascotas.Rows.IndexOf(row) % 2 == 0)
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
                    Paragraph footerNote = new Paragraph("Este es un reporte interno de mascotas de VetWeb.", fontFooter);
                    footerNote.Alignment = Element.ALIGN_CENTER;
                    footerNote.SpacingBefore = 20f;
                    doc.Add(footerNote);

                    Paragraph thankYouNote = new Paragraph("Generado por VetWeb - Tu solución para la gestión veterinaria.", FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.LIGHT_GRAY));
                    thankYouNote.Alignment = Element.ALIGN_CENTER;
                    doc.Add(thankYouNote);

                    doc.Close();

                    // Enviar el PDF al navegador
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=ReporteMascotas.pdf");
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
            DataTable dtMascotas = new DataTable();
            string filtroAplicado = string.IsNullOrEmpty(txtBuscarNombreMascota.Text.Trim()) ? "Ninguno" : txtBuscarNombreMascota.Text.Trim();

            using (SqlConnection con = new SqlConnection(cadena))
            {
                // Obtener los datos de las mascotas (aplicando el filtro de búsqueda actual si lo hay)
                string query = @"
                    SELECT
                        M.Nombre,
                        M.Edad,
                        CASE M.Sexo
                            WHEN 'M' THEN 'Macho'
                            WHEN 'H' THEN 'Hembra'
                            ELSE 'Desconocido'
                        END AS SexoMascota,         
                        C.PrimerNombre + ' ' + C.ApellidoPaterno AS NombreCliente,
                        R.NombreRaza
                    FROM Mascotas M
                    INNER JOIN Clientes C ON M.ClienteID = C.ClienteID
                    INNER JOIN Razas R ON M.RazaID = R.RazaID" ; 

                if (!string.IsNullOrEmpty(txtBuscarNombreMascota.Text.Trim()))
                {
                    query += " WHERE M.Nombre LIKE '%' + @SearchTerm + '%' " +
                             "OR C.PrimerNombre LIKE '%' + @SearchTerm + '%' " +
                             "OR C.ApellidoPaterno LIKE '%' + @SearchTerm + '%' " +
                             "OR R.NombreRaza LIKE '%' + @SearchTerm + '%'";
                }
                query += " ORDER BY M.Nombre"; // Ordenar para una visualización consistente

                SqlCommand cmd = new SqlCommand(query, con);
                if (!string.IsNullOrEmpty(txtBuscarNombreMascota.Text.Trim()))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", txtBuscarNombreMascota.Text.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                try
                {
                    con.Open();
                    da.Fill(dtMascotas);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al cargar los datos para el Excel: " + ex.Message, false);
                    return;
                }
            }

            if (dtMascotas.Rows.Count == 0)
            {
                MostrarMensaje("No hay datos de mascotas para generar el Excel con el filtro actual.", false);
                return;
            }

            try
            {
                // Configurar la respuesta para descargar un archivo Excel
                Response.Clear();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.ms-excel"; // MIME type para Excel 97-2003
                Response.AddHeader("Content-Disposition", "attachment;filename=ReporteMascotas_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xls");
                Response.Charset = "UTF-8";
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble()); // Para UTF-8 con BOM

                // Usar StringBuilder para construir el contenido HTML de la tabla
                StringBuilder sb = new StringBuilder();

                // Cabecera HTML para Excel (opcional pero recomendable para una mejor compatibilidad)
                sb.Append("<html xmlns:x=\"urn:schemas-microsoft-com:office:excel\">");
                sb.Append("<head><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet>");
                sb.Append("<x:Name>Mascotas</x:Name>");
                sb.Append("<x:WorksheetOptions><x:Panes></x:Panes></x:WorksheetOptions>");
                sb.Append("</x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml></head>");
                sb.Append("<body>");

                // Título del reporte en el Excel
                // Son 5 columnas (Nombre, Edad, Sexo, Cliente, Raza)
                sb.Append("<table border='0' style='font-family: Arial; font-size: 14pt;'><tr><td colspan='5' align='center'><b>REPORTE DE MASCOTAS</b></td></tr></table>");
                sb.Append("<table border='0' style='font-family: Arial; font-size: 10pt;'><tr><td colspan='5' align='left'>Fecha de Generación: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "</td></tr>");
                sb.Append("<tr><td colspan='5' align='left'>Filtro Aplicado: \"" + filtroAplicado + "\"</td></tr></table>");
                sb.Append("<br>"); // Salto de línea para separar el encabezado de la tabla de datos

                // Crear la tabla HTML para los datos
                sb.Append("<table border='1px' cellpadding='0' cellspacing='0' style='border-collapse: collapse; font-family: Arial; font-size: 10pt;'>");

                // Añadir fila de encabezados
                sb.Append("<tr style='background-color:#36506A; color:#FFFFFF;'>");
                sb.Append("<th>Nombre Mascota</th>");
                sb.Append("<th>Edad</th>");
                sb.Append("<th>Sexo</th>");
                sb.Append("<th>Cliente</th>");
                sb.Append("<th>Raza</th>");
                sb.Append("</tr>");

                // Añadir filas de datos
                foreach (DataRow row in dtMascotas.Rows)
                {
                    sb.Append("<tr>");
                    sb.Append("<td>" + Server.HtmlEncode(row["Nombre"].ToString()) + "</td>");
                    sb.Append("<td>" + Server.HtmlEncode(row["Edad"].ToString()) + "</td>");
                    sb.Append("<td>" + Server.HtmlEncode(row["SexoMascota"].ToString()) + "</td>"); // Usar SexoMascota
                    sb.Append("<td>" + Server.HtmlEncode(row["NombreCliente"].ToString()) + "</td>");
                    sb.Append("<td>" + Server.HtmlEncode(row["NombreRaza"].ToString()) + "</td>");
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
