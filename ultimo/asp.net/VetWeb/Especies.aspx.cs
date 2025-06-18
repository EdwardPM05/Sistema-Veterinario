using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web.UI; // Required for ScriptManager
using System.Web.UI.WebControls;

namespace VetWeb
{
    public partial class Especies : System.Web.UI.Page
    {
        // Connection string to the database, retrieved from Web.config
        string cadena = ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Only load data and set initial UI state on the first page load, not on postbacks
            if (!IsPostBack)
            {
                CargarEspecies();
                // Initially, the "Update" button should be hidden, "Add" button visible
                btnActualizar.Style["display"] = "none";
                btnAgregar.Style["display"] = "inline-block";
            }
        }

        /// <summary>
        /// Loads all species data from the database and binds it to the GridView,
        /// optionally filtering by species name.
        /// </summary>
        /// <param name="searchTerm">Optional: The term to search for in NombreEspecie.</param>
        private void CargarEspecies(string searchTerm = null)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = "SELECT EspecieID, NombreEspecie FROM Especies";

                // If a search term is provided, add a WHERE clause
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    // Use LIKE for partial matching and concatenate wildcards in C# for clarity
                    query += " WHERE NombreEspecie LIKE '%' + @SearchTerm + '%'";
                }

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    // Add the parameter for the search term
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt); // Fill the DataTable with data
                gvEspecies.DataSource = dt; // Set the DataTable as the GridView's data source
                gvEspecies.DataBind(); // Bind the data to the GridView

                // Removed the lines that set lblResultadosBusqueda.Text
                // The lblResultadosBusqueda control was removed from the ASPX.
            }
        }

        /// <summary>
        /// Handles the click event for the "Agregar" (Add) button.
        /// Adds a new species record to the database.
        /// </summary>
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            // Validate the form input before proceeding
            if (!ValidarFormulario()) return;

            bool successOperation = false; // Flag to track operation success
            using (SqlConnection con = new SqlConnection(cadena))
            {
                // SQL INSERT statement. EspecieID is IDENTITY, so it's not included in INSERT
                string query = "INSERT INTO Especies (NombreEspecie) VALUES (@NombreEspecie)";
                SqlCommand cmd = new SqlCommand(query, con);
                // Explicitly define SqlDbType for robustness
                cmd.Parameters.Add("@NombreEspecie", SqlDbType.NVarChar, 100).Value = txtNombreEspecie.Text.Trim();

                try
                {
                    con.Open(); // Open the database connection
                    cmd.ExecuteNonQuery(); // Execute the INSERT command
                    MostrarMensaje("Especie agregada correctamente.", true); // Show success message
                    successOperation = true; // Mark as successful
                }
                catch (SqlException ex)
                {
                    // Handle potential database errors (e.g., unique constraint violation)
                    if (ex.Number == 2627) // Error number for unique constraint violation
                    {
                        MostrarMensaje("El nombre de la especie ya existe. Por favor, ingrese un nombre único.", false);
                    }
                    else
                    {
                        MostrarMensaje("Error de base de datos al agregar especie: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    // Catch any other unexpected errors
                    MostrarMensaje("Ocurrió un error inesperado al agregar especie: " + ex.Message, false);
                }
                finally
                {
                    con.Close(); // Ensure the connection is closed
                }
            }

            // After database operation, update UI based on success or failure
            if (successOperation)
            {
                LimpiarFormulario(); // Clear form fields only on successful add
                CargarEspecies(); // Refresh the GridView (without search term)
                // Modal will be hidden by MostrarMensaje on success
            }
            else
            {
                // If there was an error, the modal remains open (due to MostrarMensaje)
                // and the form data is preserved for correction. Just refresh the grid.
                CargarEspecies(txtBuscarNombreEspecie.Text.Trim()); // Refresh grid with current search term if any
            }
        }

        /// <summary>
        /// Handles the click event for the "Actualizar" (Update) button.
        /// Updates an existing species record in the database.
        /// </summary>
        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            // Validate the form input
            if (!ValidarFormulario())
            {
                // Message will be shown by ValidarFormulario
                return;
            }

            int especieID;
            // Attempt to parse the EspecieID from the hidden field to an integer
            if (!int.TryParse(hfEspecieID.Value, out especieID))
            {
                MostrarMensaje("Error: El ID de la especie no tiene un formato válido para actualizar. Por favor, intente editar de nuevo o verifique los datos.", false);
                return;
            }

            bool successOperation = false; // Flag to track operation success
            using (SqlConnection con = new SqlConnection(cadena))
            {
                // SQL UPDATE statement with parameterized query
                string query = "UPDATE Especies SET NombreEspecie=@NombreEspecie WHERE EspecieID=@EspecieID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.Add("@NombreEspecie", SqlDbType.NVarChar, 100).Value = txtNombreEspecie.Text.Trim();
                cmd.Parameters.Add("@EspecieID", SqlDbType.Int).Value = especieID; // Use the parsed int ID

                try
                {
                    con.Open(); // Open the database connection
                    int rowsAffected = cmd.ExecuteNonQuery(); // Execute the UPDATE command
                    if (rowsAffected > 0)
                    {
                        MostrarMensaje("Especie actualizada correctamente.", true); // Show success message
                        successOperation = true; // Mark as successful
                    }
                    else
                    {
                        MostrarMensaje("No se encontró la especie para actualizar o no hubo cambios.", false);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627) // Error number for unique constraint violation
                    {
                        MostrarMensaje("El nombre de la especie ya existe. Por favor, ingrese un nombre único.", false);
                    }
                    else
                    {
                        MostrarMensaje("Error de base de datos al actualizar especie: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    // Catch any other unexpected errors
                    MostrarMensaje("Ocurrió un error inesperado al actualizar especie: " + ex.Message, false);
                }
                finally
                {
                    con.Close(); // Ensure the connection is closed
                }
            }

            // After database operation, update UI based on success or failure
            if (successOperation)
            {
                LimpiarFormulario(); // Clear form fields and reset UI for add mode only on successful update
                CargarEspecies(); // Refresh the GridView (without search term)
                // Modal will be hidden by MostrarMensaje on success
            }
            else
            {
                // If there was an error, the modal remains open (due to MostrarMensaje)
                // and the form data is preserved for correction. Just refresh the grid.
                CargarEspecies(txtBuscarNombreEspecie.Text.Trim()); // Refresh grid with current search term if any
            }
        }

        /// <summary>
        /// Handles commands issued from within the GridView rows (e.g., Edit, Delete).
        /// </summary>
        protected void gvEspecies_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument); // Get the row index

            // Retrieve the EspecieID using DataKeys. This is crucial because EspecieID is no longer a visible column.
            int especieID = Convert.ToInt32(gvEspecies.DataKeys[index].Value);

            // Get the current row (still useful for NombreEspecie, which is visible)
            GridViewRow row = gvEspecies.Rows[index];

            if (e.CommandName == "Editar")
            {
                // Populate the text box and hidden field with data from the selected row.
                // NombreEspecie is in Cell[0] because EspecieID column was removed from display.
                txtNombreEspecie.Text = row.Cells[0].Text;
                hfEspecieID.Value = especieID.ToString(); // Store the integer ID as a string in the hidden field

                // Change button visibility for "Edit" mode
                btnAgregar.Style["display"] = "none";     // Hide Add button
                btnActualizar.Style["display"] = "inline-block"; // Show Update button

                // Set modal title for editing
                ScriptManager.RegisterStartupScript(this, this.GetType(), "SetModalTitle", "document.getElementById('especieModalLabel').innerText = 'Editar Especie';", true);

                MostrarMensaje("", false); // Clear any previous messages

                // Show the modal
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalScript", "showEspecieModal();", true);
            }
            else if (e.CommandName == "Eliminar")
            {
                // especieID is already retrieved from DataKeys

                using (SqlConnection con = new SqlConnection(cadena))
                {
                    con.Open();
                    SqlTransaction transaction = con.BeginTransaction(); // Start a transaction for atomicity

                    try
                    {
                        // First, check if there are any associated "Razas" (Breeds) for this species
                        SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Razas WHERE EspecieID = @EspecieID", con, transaction);
                        checkCmd.Parameters.Add("@EspecieID", SqlDbType.Int).Value = especieID;
                        int dependentRazas = (int)checkCmd.ExecuteScalar();

                        if (dependentRazas > 0)
                        {
                            MostrarMensaje("No se puede eliminar la especie porque tiene razas asociadas. Elimine las razas primero.", false);
                            transaction.Rollback(); // Rollback if dependent records exist
                            return; // Stop the deletion process
                        }

                        // If no dependent razas, proceed with deleting the species
                        SqlCommand cmd = new SqlCommand("DELETE FROM Especies WHERE EspecieID = @EspecieID", con, transaction);
                        cmd.Parameters.Add("@EspecieID", SqlDbType.Int).Value = especieID;
                        cmd.ExecuteNonQuery();

                        transaction.Commit(); // Commit the transaction if successful
                        MostrarMensaje("Especie eliminada correctamente.", true);
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback(); // Rollback on any error
                        MostrarMensaje("Error de base de datos al eliminar especie: " + ex.Message, false);
                    }
                    catch (Exception ex)
                    {
                        // Catch any other unexpected errors
                        MostrarMensaje("Ocurrió un error inesperado al eliminar especie: " + ex.Message, false);
                    }
                    finally
                    {
                        con.Close();
                    }
                }
                CargarEspecies(); // Refresh the GridView after deletion
            }
        }

        /// <summary>
        /// Clears the form fields and resets the UI to "Add" mode.
        /// </summary>
        private void LimpiarFormulario()
        {
            txtNombreEspecie.Text = ""; // Clear the species name textbox
            hfEspecieID.Value = ""; // Clear the hidden ID field

            // Reset button visibility to "Add" mode
            btnAgregar.Style["display"] = "inline-block";
            btnActualizar.Style["display"] = "none";

            // Reset modal title
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitle", "document.getElementById('especieModalLabel').innerText = 'Agregar Nueva Especie';", true);

            MostrarMensaje("", false); // Clear any displayed messages
        }

        /// <summary>
        /// Validates the input form fields.
        /// </summary>
        /// <returns>True if the form is valid, false otherwise.</returns>
        private bool ValidarFormulario()
        {
            // Validate that the species name is not empty or just whitespace
            if (string.IsNullOrWhiteSpace(txtNombreEspecie.Text))
            {
                MostrarMensaje("Por favor, ingrese el nombre de la especie.", false);
                return false;
            }

            // Validate that NombreEspecie only contains letters and spaces
            // Requires 'using System.Text.RegularExpressions;' at the top of the file
            if (!Regex.IsMatch(txtNombreEspecie.Text.Trim(), @"^[\p{L}\s]+$")) // Used \p{L} for any unicode letter, including accented characters
            {
                MostrarMensaje("El campo 'Nombre' solo puede contener letras.", false);
                return false;
            }

            return true; // Form is valid
        }

        /// <summary>
        /// Displays a message to the user, either success or error.
        /// </summary>
        /// <param name="mensaje">The message to display.</param>
        /// <param name="exito">True for a success message (green), false for an error message (red).</param>
        private void MostrarMensaje(string mensaje, bool exito)
        {
            // If the message is empty, clear the label and its styling
            if (string.IsNullOrEmpty(mensaje))
            {
                lblMensaje.Text = "";
                lblMensaje.CssClass = "";
            }
            else
            {
                lblMensaje.Text = mensaje; // Set the message text
                // Apply appropriate Bootstrap alert class based on success or error
                lblMensaje.CssClass = exito ? "alert alert-success" : "alert alert-danger";

                // Control modal visibility based on success/error
                if (!exito) // If it's an error message
                {
                    // Re-open the modal to show the error
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowEspecieModalOnError", "showEspecieModal();", true);
                }
                else // If it's a success message
                {
                    // Hide the modal on success
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideEspecieModalOnSuccess", "hideEspecieModal();", true);
                }
            }

            // Ensure the message is visible within the modal
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollToMessage", "var modalBody = document.querySelector('#especieModal .modal-body'); if(modalBody) modalBody.scrollTop = 0;", true);
        }

        /// <summary>
        /// Handles the click event for the search button.
        /// Filters the species list based on the entered search term.
        /// </summary>
        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            CargarEspecies(txtBuscarNombreEspecie.Text.Trim());
        }

        /// <summary>
        /// Handles the click event for the clear search button.
        /// Clears the search term and reloads the full species list.
        /// </summary>
        protected void btnLimpiarBusqueda_Click(object sender, EventArgs e)
        {
            txtBuscarNombreEspecie.Text = ""; // Clear the search textbox
            CargarEspecies(); // Reload all species without a filter
            // lblResultadosBusqueda.Text = ""; // This line was removed as the control is no longer in ASPX
        }
    }
}
