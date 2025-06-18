using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions; // Important: You need this line for Regex!
using System.Web.UI; // Required for ScriptManager to call client-side scripts
using System.Web.UI.WebControls;

namespace VetWeb
{
    public partial class Razas : System.Web.UI.Page
    {
        string cadena = ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarEspecies(); // Load species for the dropdown
                CargarRazas();    // Load breeds (possibly filtered)
                // Initial state for modal buttons (add mode)
                btnActualizar.Style["display"] = "none";
                btnAgregar.Style["display"] = "inline-block";
            }
        }

        /// <summary>
        /// Loads all species from the database and populates the Especies DropDownList.
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
                ddlEspecies.Items.Insert(0, new ListItem("Seleccione una especie", "")); // Add default item
            }
        }

        /// <summary>
        /// Loads breed data from the database and binds it to the GridView,
        /// optionally filtering by breed name. Includes species name.
        /// </summary>
        /// <param name="searchTerm">Optional: The term to search for in NombreRaza.</param>
        private void CargarRazas(string searchTerm = null)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "SELECT R.RazaID, R.NombreRaza, E.NombreEspecie FROM Razas R INNER JOIN Especies E ON R.EspecieID = E.EspecieID";

                // If a search term is provided, add a WHERE clause
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += " WHERE R.NombreRaza LIKE '%' + @SearchTerm + '%'";
                }

                query += " ORDER BY E.NombreEspecie, R.NombreRaza"; // Always order for consistent display

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvRazas.DataSource = dt;
                gvRazas.DataBind();
            }
        }

        /// <summary>
        /// Handles the click event for the "Agregar" (Add) button in the modal.
        /// Adds a new breed record to the database.
        /// </summary>
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            // If validation fails, MostrarMensaje will display the error and prevent further execution.
            if (!ValidarFormulario()) return;

            bool successOperation = false; // Flag to track operation success
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "INSERT INTO Razas (NombreRaza, EspecieID) VALUES (@NombreRaza, @EspecieID)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@NombreRaza", txtNombreRaza.Text.Trim());
                cmd.Parameters.AddWithValue("@EspecieID", Convert.ToInt32(ddlEspecies.SelectedValue));

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    MostrarMensaje("Raza '" + txtNombreRaza.Text.Trim() + "' agregada correctamente.", true);
                    successOperation = true; // Mark as successful
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Unique constraint violation (NombreRaza, EspecieID)
                    {
                        MostrarMensaje("Error: La raza '" + txtNombreRaza.Text.Trim() + "' ya existe para la especie seleccionada. Por favor, verifique.", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al agregar la raza: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al agregar la raza: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            // After database operation, update UI based on success or failure
            if (successOperation)
            {
                LimpiarFormulario(); // Clear form fields only on successful add
                CargarRazas();       // Refresh the GridView (without search filter)
            }
            else
            {
                // If there was an error, the modal remains open (due to MostrarMensaje)
                // and the form data is preserved for correction. Just refresh the grid.
                CargarRazas(txtBuscarNombreRaza.Text.Trim()); // Refresh grid with current search term if any
            }
        }

        /// <summary>
        /// Handles the click event for the "Actualizar" (Update) button in the modal.
        /// Updates an existing breed record in the database.
        /// </summary>
        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            // If validation fails, MostrarMensaje will display the error and prevent further execution.
            if (!ValidarFormulario()) return;

            int razaID;
            // Ensure RazaID from hidden field is valid integer
            if (!int.TryParse(hfRazaID.Value, out razaID))
            {
                MostrarMensaje("Error: El ID de la raza no tiene un formato válido para actualizar. Por favor, intente editar de nuevo.", false);
                return;
            }

            bool successOperation = false; // Flag to track operation success
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "UPDATE Razas SET NombreRaza=@NombreRaza, EspecieID=@EspecieID WHERE RazaID=@RazaID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@NombreRaza", txtNombreRaza.Text.Trim());
                cmd.Parameters.AddWithValue("@EspecieID", Convert.ToInt32(ddlEspecies.SelectedValue));
                cmd.Parameters.AddWithValue("@RazaID", razaID);

                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MostrarMensaje("Raza actualizada correctamente.", true);
                        successOperation = true; // Mark as successful
                    }
                    else
                    {
                        MostrarMensaje("No se encontró la raza para actualizar o no hubo cambios.", false);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Unique constraint violation (NombreRaza, EspecieID)
                    {
                        MostrarMensaje("Error: La raza '" + txtNombreRaza.Text.Trim() + "' ya existe para la especie seleccionada. Por favor, verifique.", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al actualizar la raza: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al actualizar la raza: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            // After database operation, update UI based on success or failure
            if (successOperation)
            {
                LimpiarFormulario(); // Clear form fields and reset UI for add mode only on successful update
                CargarRazas();       // Refresh the GridView (without search filter)
            }
            else
            {
                // If there was an error, the modal remains open (due to MostrarMensaje)
                // and the form data is preserved for correction. Just refresh the grid.
                CargarRazas(txtBuscarNombreRaza.Text.Trim()); // Refresh grid with current search term if any
            }
        }

        /// <summary>
        /// Handles commands issued from within the GridView rows (e.g., Edit, Delete).
        /// </summary>
        protected void gvRazas_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);

            // Important: Retrieve RazaID using DataKeys as the column is not displayed.
            if (gvRazas.DataKeys == null || index < 0 || index >= gvRazas.DataKeys.Count)
            {
                MostrarMensaje("Error interno: No se pudo obtener el ID de la raza. Por favor, recargue la página.", false);
                return;
            }
            int razaID = Convert.ToInt32(gvRazas.DataKeys[index].Value);

            // Get the current row for other visible data
            GridViewRow row = gvRazas.Rows[index];

            if (e.CommandName == "Editar")
            {
                // Populate the text box and hidden field with data from the selected row.
                // NombreRaza is in Cell[0] as it's the first visible BoundField.
                txtNombreRaza.Text = row.Cells[0].Text;
                hfRazaID.Value = razaID.ToString(); // Store the integer ID as a string in the hidden field

                // Populate the DropDownList for Especie
                if (row.Cells.Count > 1) // NombreEspecie should be in Cell[1]
                {
                    string nombreEspecie = row.Cells[1].Text;
                    ListItem item = ddlEspecies.Items.FindByText(nombreEspecie);
                    if (item != null)
                    {
                        ddlEspecies.SelectedValue = item.Value;
                    }
                    else
                    {
                        // Handle case where species might not be in the dropdown (e.g., deleted)
                        ddlEspecies.ClearSelection();
                        ddlEspecies.Items.Insert(0, new ListItem("Seleccione una especie (No encontrada)", "")); // Add placeholder
                        ddlEspecies.Items.FindByValue("").Selected = true;
                        MostrarMensaje("Advertencia: La especie asociada a esta raza no se encontró. Por favor, seleccione una nueva.", false);
                    }
                }

                // Change button visibility for "Edit" mode
                btnAgregar.Style["display"] = "none";     // Hide Add button
                btnActualizar.Style["display"] = "inline-block"; // Show Update button

                // Set modal title for editing
                ScriptManager.RegisterStartupScript(this, this.GetType(), "SetRazaModalTitle", "document.getElementById('razaModalLabel').innerText = 'Editar Raza';", true);

                MostrarMensaje("", false); // Clear any previous messages

                // Show the modal
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowRazaModalScript", "showRazaModal();", true);
            }
            else if (e.CommandName == "Eliminar")
            {
                // razaID is already retrieved from DataKeys

                using (SqlConnection con = new SqlConnection(cadena))
                {
                    con.Open();
                    SqlTransaction transaction = con.BeginTransaction(); // Start a transaction for atomicity

                    try
                    {
                        // First, check if there are any associated "Mascotas" for this raza
                        SqlCommand checkMascotasCmd = new SqlCommand("SELECT COUNT(*) FROM Mascotas WHERE RazaID = @RazaID", con, transaction);
                        checkMascotasCmd.Parameters.Add("@RazaID", SqlDbType.Int).Value = razaID;
                        int numMascotas = (int)checkMascotasCmd.ExecuteScalar();

                        if (numMascotas > 0)
                        {
                            MostrarMensaje("No se puede eliminar esta raza porque tiene " + numMascotas + " mascota(s) asociada(s). Elimine las mascotas o reasígnelas primero.", false);
                            transaction.Rollback(); // Rollback if dependent records exist
                            return; // Stop the deletion process
                        }

                        // If no dependent mascotas, proceed with deleting the species
                        SqlCommand cmd = new SqlCommand("DELETE FROM Razas WHERE RazaID = @RazaID", con, transaction);
                        cmd.Parameters.Add("@RazaID", SqlDbType.Int).Value = razaID;
                        cmd.ExecuteNonQuery();

                        transaction.Commit(); // Commit the transaction if successful
                        MostrarMensaje("Raza eliminada correctamente.", true);
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback(); // Rollback on any error
                        if (ex.Number == 547) // Foreign key constraint violation
                        {
                            MostrarMensaje("No se pudo eliminar la raza porque tiene registros asociados (ej. mascotas). Elimine los registros asociados primero.", false);
                        }
                        else
                        {
                            MostrarMensaje("Error de base de datos al eliminar raza: " + ex.Message, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        MostrarMensaje("Ocurrió un error inesperado al eliminar raza: " + ex.Message, false);
                    }
                    finally
                    {
                        con.Close();
                    }
                }
                CargarRazas(); // Refresh the GridView after deletion
            }
        }

        /// <summary>
        /// Clears the form fields and resets the UI to "Add" mode.
        /// This is called when adding a new item, or after edit/delete.
        /// </summary>
        private void LimpiarFormulario()
        {
            txtNombreRaza.Text = "";
            ddlEspecies.ClearSelection();
            // Re-select the default item if it exists
            ListItem defaultItem = ddlEspecies.Items.FindByValue("");
            if (defaultItem != null)
            {
                defaultItem.Selected = true;
            }
            else if (ddlEspecies.Items.Count > 0)
            {
                ddlEspecies.Items[0].Selected = true; // Select first if no empty value
            }
            hfRazaID.Value = "";

            // Reset button visibility for "Add" mode
            btnAgregar.Style["display"] = "inline-block";
            btnActualizar.Style["display"] = "none";

            // Set modal title back to "Add New"
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetRazaModalTitle", "document.getElementById('razaModalLabel').innerText = 'Agregar Nueva Raza';", true);

            MostrarMensaje("", false); // Clear any displayed messages
        }

        /// <summary>
        /// Validates the input form fields for Raza.
        /// </summary>
        /// <returns>True if the form is valid, false otherwise.</returns>
        private bool ValidarFormulario()
        {
            // Validaciones de campos obligatorios
            if (string.IsNullOrWhiteSpace(txtNombreRaza.Text))
            {
                MostrarMensaje("Por favor, ingrese el nombre de la raza.", false);
                return false;
            }

            if (string.IsNullOrEmpty(ddlEspecies.SelectedValue) || ddlEspecies.SelectedValue == "") // Check if no species is selected or default empty value
            {
                MostrarMensaje("Por favor, seleccione una especie válida para la raza.", false);
                return false;
            }

            // --- VALIDACIÓN DE FORMATO para NombreRaza (letras, tildes, ñ, espacios, apóstrofes, guiones) ---
            // Requires 'using System.Text.RegularExpressions;' at the top of the file
            // \p{L} matches any kind of letter from any language
            if (!Regex.IsMatch(txtNombreRaza.Text.Trim(), @"^[\p{L}\s\.'-]+$"))
            {
                MostrarMensaje("El campo 'Nombre' solo puede contener letras ", false);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Displays a message to the user, either success or error, using Bootstrap alert styles.
        /// </summary>
        /// <param name="mensaje">The message to display.</param>
        /// <param name="exito">True for a success message (green alert), false for an error message (red alert).</param>
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

                // Control modal visibility based on success/error
                if (!exito) // If it's an error message
                {
                    // Re-open the modal to show the error
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowRazaModalOnError", "showRazaModal();", true);
                }
                else // If it's a success message
                {
                    // Hide the modal on success
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideRazaModalOnSuccess", "hideRazaModal();", true);
                }
            }

            // Scroll to message, applicable whether it's shown or hidden
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollToMessageRaza", "var modalBody = document.querySelector('#razaModal .modal-body'); if(modalBody) modalBody.scrollTop = 0;", true);
        }

        /// <summary>
        /// Handles the click event for the search button.
        /// Filters the breed list based on the entered search term.
        /// </summary>
        protected void btnBuscarRaza_Click(object sender, EventArgs e)
        {
            CargarRazas(txtBuscarNombreRaza.Text.Trim());
        }

        /// <summary>
        /// Handles the click event for the clear search button.
        /// Clears the search term and reloads the full breed list.
        /// </summary>
        protected void btnLimpiarBusquedaRaza_Click(object sender, EventArgs e)
        {
            txtBuscarNombreRaza.Text = ""; // Clear the search textbox
            CargarRazas(); // Reload all breeds without a filter
        }
    }
}
