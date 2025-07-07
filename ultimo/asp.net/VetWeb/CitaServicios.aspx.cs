using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization; // Necesario para CultureInfo
using System.Web.UI;
using System.Web.UI.WebControls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Linq;

namespace VetWeb
{
    public partial class CitaServicios : System.Web.UI.Page
    {
        string cadena = ConfigurationManager.ConnectionStrings["Conexion"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarCitas();
                CargarServicios();
                LimpiarFormulario();
                gvCitaServicios.Visible = false;
                lblTotalCita.Visible = false;
                lblInfoCitaSeleccionada.Visible = false;
            }
        }

        private void CargarCitas()
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string query = @"
                    SELECT
                        C.CitaID,
                        C.Fecha,
                        Cl.PrimerNombre + ' ' + Cl.ApellidoPaterno AS NombreCliente,
                        M.Nombre AS NombreMascota
                    FROM Citas C
                    INNER JOIN Mascotas M ON C.MascotaID = M.MascotaID
                    INNER JOIN Clientes Cl ON M.ClienteID = Cl.ClienteID
                    ORDER BY C.Fecha DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                dt.Columns.Add("CitaInfo", typeof(string));
                foreach (DataRow row in dt.Rows)
                {
                    row["CitaInfo"] = $"{Convert.ToDateTime(row["Fecha"]).ToString("dd/MM/yyyy")} - {row["NombreCliente"]} - {row["NombreMascota"]}";
                }

                ddlCitas.DataSource = dt;
                ddlCitas.DataTextField = "CitaInfo";
                ddlCitas.DataValueField = "CitaID";
                ddlCitas.DataBind();
                ddlCitas.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione una Cita", ""));
            }
        }

        private void CargarServicios()
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT ServicioID, NombreServicio, Precio FROM Servicios ORDER BY NombreServicio", con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlServicios.DataSource = dt;
                ddlServicios.DataTextField = "NombreServicio";
                ddlServicios.DataValueField = "ServicioID";
                ddlServicios.DataBind();
                ddlServicios.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Seleccione un Servicio", ""));
            }
        }

        protected void ddlCitas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ddlCitas.SelectedValue))
            {
                int selectedCitaID = Convert.ToInt32(ddlCitas.SelectedValue);
                hfSelectedCitaID.Value = selectedCitaID.ToString();
                CargarCitaServicios(selectedCitaID);
                CalcularTotalCita(selectedCitaID); // El cálculo ahora se basa en el precio actual del servicio

                lblInfoCitaSeleccionada.Text = ddlCitas.SelectedItem.Text;
                gvCitaServicios.Visible = true;
                lblTotalCita.Visible = true;
                lblInfoCitaSeleccionada.Visible = true;
                LimpiarFormulario();
            }
            else
            {
                gvCitaServicios.DataSource = null;
                gvCitaServicios.DataBind();
                gvCitaServicios.Visible = false;
                lblTotalCita.Text = "Total de la Cita: S/ 0.00";
                lblTotalCita.Visible = false;
                lblInfoCitaSeleccionada.Text = "";
                lblInfoCitaSeleccionada.Visible = false;
                hfSelectedCitaID.Value = "";
                LimpiarFormulario();
            }
            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModalAfterCitaChange", "hideCitaServicioModal();", true);
        }

        protected void ddlServicios_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalcularValoresModal(); // Se calcula el precio actual del servicio
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCitaServicioModalOnDdlChange", "showCitaServicioModal();", true);
        }

        /// <summary>
        /// Obtiene y muestra el precio actual del servicio seleccionado del catálogo.
        /// </summary>
        private void CalcularValoresModal()
        {
            decimal precioServicio = 0;

            if (!string.IsNullOrEmpty(ddlServicios.SelectedValue))
            {
                int servicioID = Convert.ToInt32(ddlServicios.SelectedValue);
                using (SqlConnection con = new SqlConnection(cadena))
                {
                    SqlCommand cmd = new SqlCommand("SELECT Precio FROM Servicios WHERE ServicioID = @ServicioID", con);
                    cmd.Parameters.AddWithValue("@ServicioID", servicioID);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        precioServicio = Convert.ToDecimal(result);
                    }
                    con.Close();
                }
            }

            CultureInfo culturePE = new CultureInfo("es-PE");
            // Se muestra el precio del servicio directamente como subtotal
            lblSubtotalServicio.Text = precioServicio.ToString("C", culturePE);
        }

        private void CargarCitaServicios(int citaID)
        {
            using (SqlConnection con = new SqlConnection(cadena))
            {
                // La consulta se adapta para obtener el precio actual de la tabla Servicios
                string query = @"
                    SELECT
                        CS.CitaServicioID,
                        CS.CitaID,
                        CS.ServicioID,
                        S.NombreServicio,
                        S.Precio AS PrecioUnitario, -- Obtenemos el precio actual de Servicios
                        S.Precio AS TotalServicio   -- El total de un servicio es su precio unitario si cantidad es 1
                    FROM CitaServicios CS
                    INNER JOIN Servicios S ON CS.ServicioID = S.ServicioID
                    WHERE CS.CitaID = @CitaID
                    ORDER BY S.NombreServicio";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CitaID", citaID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvCitaServicios.DataSource = dt;
                gvCitaServicios.DataBind();
            }
        }

        private void CalcularTotalCita(int citaID)
        {
            decimal totalCita = 0;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                // Se suma el precio actual de cada servicio asociado a la cita
                string query = @"
                    SELECT ISNULL(SUM(S.Precio), 0)
                    FROM CitaServicios CS
                    INNER JOIN Servicios S ON CS.ServicioID = S.ServicioID
                    WHERE CS.CitaID = @CitaID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CitaID", citaID);
                con.Open();
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    totalCita = Convert.ToDecimal(result);
                }
                con.Close();
            }
            CultureInfo culturePE = new CultureInfo("es-PE");
            lblTotalCita.Text = $"Total de la Cita: {totalCita.ToString("C", culturePE)}";
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlServicios.SelectedValue) || ddlServicios.SelectedValue == "")
            {
                MostrarMensaje("Por favor, seleccione un Servicio.", false);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCitaServicioModalOnError", "showCitaServicioModal();", true);
                return;
            }

            if (string.IsNullOrEmpty(hfSelectedCitaID.Value))
            {
                MostrarMensaje("Por favor, seleccione una Cita antes de agregar servicios.", false);
                return;
            }

            int citaID = Convert.ToInt32(hfSelectedCitaID.Value);
            int servicioID = Convert.ToInt32(ddlServicios.SelectedValue);

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                // La inserción ahora solo requiere CitaID y ServicioID
                string query = "INSERT INTO CitaServicios (CitaID, ServicioID) VALUES (@CitaID, @ServicioID)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CitaID", citaID);
                cmd.Parameters.AddWithValue("@ServicioID", servicioID);

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    MostrarMensaje("Servicio agregado a la cita correctamente.", true);
                    successOperation = true;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627 || ex.Number == 2601) // Clave única duplicada
                    {
                        MostrarMensaje("Error: Este servicio ya ha sido agregado a esta cita. No se puede añadir el mismo servicio dos veces.", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al agregar el servicio a la cita: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al agregar el servicio a la cita: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarCitaServicios(citaID);
                CalcularTotalCita(citaID);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideCitaServicioModal", "hideCitaServicioModal();", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCitaServicioModalOnError", "showCitaServicioModal();", true);
            }
        }

        protected void btnActualizar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlServicios.SelectedValue) || ddlServicios.SelectedValue == "")
            {
                MostrarMensaje("Por favor, seleccione un Servicio.", false);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCitaServicioModalOnErrorUpdate", "showCitaServicioModal();", true);
                return;
            }

            int citaServicioID;
            if (!int.TryParse(hfCitaServicioID.Value, out citaServicioID))
            {
                MostrarMensaje("Error: El ID del servicio de cita no tiene un formato válido para actualizar. Por favor, intente editar de nuevo.", false);
                return;
            }

            if (string.IsNullOrEmpty(hfSelectedCitaID.Value))
            {
                MostrarMensaje("Error interno: No se ha seleccionado una cita válida.", false);
                return;
            }

            int citaID = Convert.ToInt32(hfSelectedCitaID.Value);
            int servicioID = Convert.ToInt32(ddlServicios.SelectedValue);

            bool successOperation = false;
            using (SqlConnection con = new SqlConnection(cadena))
            {
                // La actualización ahora solo cambia el ServicioID asociado
                string query = "UPDATE CitaServicios SET ServicioID=@ServicioID WHERE CitaServicioID=@CitaServicioID AND CitaID=@CitaID";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ServicioID", servicioID);
                cmd.Parameters.AddWithValue("@CitaServicioID", citaServicioID);
                cmd.Parameters.AddWithValue("@CitaID", citaID);

                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MostrarMensaje("Servicio de cita actualizado correctamente.", true);
                        successOperation = true;
                    }
                    else
                    {
                        MostrarMensaje("No se encontró el servicio de cita para actualizar o no hubo cambios.", false);
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627 || ex.Number == 2601) // Clave única duplicada
                    {
                        MostrarMensaje("Error: Este servicio ya está asignado a esta cita. No se puede actualizar a un servicio que ya existe.", false);
                    }
                    else
                    {
                        MostrarMensaje("Ocurrió un error en la base de datos al actualizar el servicio de cita: " + ex.Message, false);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Ocurrió un error inesperado al actualizar el servicio de cita: " + ex.Message, false);
                }
                finally
                {
                    con.Close();
                }
            }

            if (successOperation)
            {
                LimpiarFormulario();
                CargarCitaServicios(citaID);
                CalcularTotalCita(citaID);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCitaServicioModalOnErrorUpdate", "showCitaServicioModal();", true);
            }
        }

        protected void gvCitaServicios_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int index = Convert.ToInt32(e.CommandArgument);

            if (gvCitaServicios.DataKeys == null || index < 0 || index >= gvCitaServicios.DataKeys.Count)
            {
                MostrarMensaje("Error interno: No se pudo obtener el ID del servicio de cita. Por favor, recargue la página.", false);
                return;
            }
            int citaServicioID = Convert.ToInt32(gvCitaServicios.DataKeys[index]["CitaServicioID"]);
            int citaID = Convert.ToInt32(gvCitaServicios.DataKeys[index]["CitaID"]);
            int servicioID = Convert.ToInt32(gvCitaServicios.DataKeys[index]["ServicioID"]);

            if (e.CommandName == "Editar")
            {
                if (Convert.ToInt32(ddlCitas.SelectedValue) != citaID)
                {
                    MostrarMensaje("Error: No puede editar un servicio de una cita diferente a la seleccionada.", false);
                    return;
                }

                try
                {
                    ddlServicios.SelectedValue = servicioID.ToString();

                    // Ya no se obtiene PrecioUnitario de CitaServicios, se recalcula del maestro
                    CalcularValoresModal(); // Recalcula el subtotal en el modal basándose en el servicio seleccionado.

                    hfCitaServicioID.Value = citaServicioID.ToString();

                    btnAgregar.Style["display"] = "none";
                    btnActualizar.Style["display"] = "inline-block";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "SetCitaServicioModalTitle", "document.getElementById('citaServicioModalLabel').innerText = 'Editar Servicio de Cita';", true);

                    MostrarMensaje("", false);
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCitaServicioModalScript", "showCitaServicioModal();", true);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al cargar los datos del servicio de cita para edición: " + ex.Message, false);
                }
            }
            else if (e.CommandName == "Eliminar")
            {
                using (SqlConnection con = new SqlConnection(cadena))
                {
                    con.Open();
                    SqlTransaction transaction = con.BeginTransaction();

                    try
                    {
                        SqlCommand cmd = new SqlCommand("DELETE FROM CitaServicios WHERE CitaServicioID = @CitaServicioID", con, transaction);
                        cmd.Parameters.AddWithValue("@CitaServicioID", citaServicioID);
                        cmd.ExecuteNonQuery();

                        transaction.Commit();
                        MostrarMensaje("Servicio de cita eliminado correctamente.", true);
                    }
                    catch (SqlException ex)
                    {
                        transaction.Rollback();
                        MostrarMensaje("Error de base de datos al eliminar servicio de cita: " + ex.Message, false);
                    }
                    catch (Exception ex)
                    {
                        MostrarMensaje("Ocurrió un error inesperado al eliminar el servicio de cita: " + ex.Message, false);
                    }
                    finally
                    {
                        con.Close();
                    }
                }
                CargarCitaServicios(citaID);
                CalcularTotalCita(citaID);
            }
        }

        private void LimpiarFormulario()
        {
            ddlServicios.ClearSelection();
            if (ddlServicios.Items.Count > 0) ddlServicios.Items.FindByValue("").Selected = true;

            CultureInfo culturePE = new CultureInfo("es-PE");
            lblSubtotalServicio.Text = (0M).ToString("C", culturePE);

            hfCitaServicioID.Value = "";
            // hfPrecioUnitarioGuardado ya no es necesario ni existe

            btnAgregar.Style["display"] = "inline-block";
            btnActualizar.Style["display"] = "none";

            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetCitaServicioModalTitle", "document.getElementById('citaServicioModalLabel').innerText = 'Agregar Servicio a Cita';", true);
            lblMensaje.Text = "";
            lblMensaje.CssClass = "";
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrEmpty(ddlServicios.SelectedValue) || ddlServicios.SelectedValue == "")
            {
                MostrarMensaje("Por favor, seleccione un Servicio.", false);
                return false;
            }
            return true;
        }

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
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideCitaServicioModalOnSuccess", "hideCitaServicioModal();", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowCitaServicioModalGeneral", "showCitaServicioModal();", true);
            }

            ScriptManager.RegisterStartupScript(this, this.GetType(), "ScrollToMessageCitaServicio" + Guid.NewGuid().ToString(),
                "var modalBody = document.querySelector('#citaServicioModal .modal-body'); if(modalBody) modalBody.scrollTop = 0;", true);
        }

        protected void btnImprimirPdf_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hfSelectedCitaID.Value))
            {
                MostrarMensaje("Por favor, seleccione una cita para generar el reporte de servicios.", false);
                return;
            }

            int selectedCitaID = Convert.ToInt32(hfSelectedCitaID.Value);
            DataTable dtCitaServicios = new DataTable();
            DataTable dtCitaInfo = new DataTable();
            decimal totalCita = 0;

            // 1. Obtener la información general de la cita
            using (SqlConnection con = new SqlConnection(cadena))
            {
                string queryCitaInfo = @"
            SELECT
                C.CitaID,
                C.Fecha,
                Cl.PrimerNombre + ' ' + Cl.ApellidoPaterno AS NombreCliente,
                M.Nombre AS NombreMascota,
                E.PrimerNombre + ' ' + E.ApellidoPaterno AS NombreEmpleado
            FROM Citas C
            INNER JOIN Mascotas M ON C.MascotaID = M.MascotaID
            INNER JOIN Clientes Cl ON M.ClienteID = Cl.ClienteID
            INNER JOIN Empleados E ON C.EmpleadoID = E.EmpleadoID
            WHERE C.CitaID = @CitaID";

                SqlCommand cmdCitaInfo = new SqlCommand(queryCitaInfo, con);
                cmdCitaInfo.Parameters.AddWithValue("@CitaID", selectedCitaID);
                SqlDataAdapter daCitaInfo = new SqlDataAdapter(cmdCitaInfo);

                try
                {
                    con.Open();
                    daCitaInfo.Fill(dtCitaInfo);
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al obtener la información de la cita para el PDF: " + ex.Message, false);
                    return;
                }
            }

            if (dtCitaInfo.Rows.Count == 0)
            {
                MostrarMensaje("No se encontró información para la cita seleccionada.", false);
                return;
            }

            DataRow citaRow = dtCitaInfo.Rows[0];
            string fechaCita = Convert.ToDateTime(citaRow["Fecha"]).ToString("dd/MM/yyyy");
            string nombreCliente = citaRow["NombreCliente"].ToString();
            string nombreMascota = citaRow["NombreMascota"].ToString();
            string nombreEmpleado = citaRow["NombreEmpleado"].ToString();

            // 2. Obtener los servicios asociados a la cita y calcular el total
            using (SqlConnection con = new SqlConnection(cadena))
            {
                // La consulta ahora obtiene el precio directamente de la tabla 'Servicios'
                // a través de la unión con 'CitaServicios'.
                string queryServicios = @"
        SELECT
            S.NombreServicio,
            S.Precio AS Subtotal 
        FROM CitaServicios CS
        INNER JOIN Servicios S ON CS.ServicioID = S.ServicioID
        WHERE CS.CitaID = @CitaID
        ORDER BY S.NombreServicio";

                SqlCommand cmdServicios = new SqlCommand(queryServicios, con);
                cmdServicios.Parameters.AddWithValue("@CitaID", selectedCitaID);
                SqlDataAdapter daServicios = new SqlDataAdapter(cmdServicios);

                try
                {
                    if (con.State != ConnectionState.Open) con.Open();
                    daServicios.Fill(dtCitaServicios);

                    // Calcular el total de la cita sumando los precios base de los servicios
                    // desde la tabla 'Servicios' a través de 'CitaServicios'.
                    SqlCommand cmdTotal = new SqlCommand(@"
            SELECT SUM(S.Precio)
            FROM CitaServicios CS
            INNER JOIN Servicios S ON CS.ServicioID = S.ServicioID
            WHERE CS.CitaID = @CitaID", con);
                    cmdTotal.Parameters.AddWithValue("@CitaID", selectedCitaID);
                    object resultTotal = cmdTotal.ExecuteScalar();

                    if (resultTotal != null && resultTotal != DBNull.Value)
                    {
                        totalCita = Convert.ToDecimal(resultTotal);
                    }
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al obtener los servicios de la cita para el PDF: " + ex.Message, false);
                    return;
                }
            }

            // Crear el documento PDF
            Document doc = new Document(PageSize.A4, 30f, 30f, 40f, 30f); // Márgenes

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    // Cultura para formato de moneda (Soles Peruanos)
                    CultureInfo culturePE = new CultureInfo("es-PE");

                    // ====================================================================
                    // 1. ENCABEZADO DEL DOCUMENTO (Logo, Info de la Clínica, Título)
                    // ====================================================================

                    PdfPTable headerTable = new PdfPTable(2);
                    headerTable.WidthPercentage = 100;
                    headerTable.SetWidths(new float[] { 1f, 3f });
                    headerTable.DefaultCell.Border = PdfPCell.NO_BORDER;
                    headerTable.SpacingAfter = 20f;

                    string logoPath = Server.MapPath("~/Assets/Images/logo.png"); // <--- ¡VERIFICA ESTA RUTA!
                    if (File.Exists(logoPath))
                    {
                        iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                        logo.ScaleToFit(70f, 70f);
                        PdfPCell logoCell = new PdfPCell(logo);
                        logoCell.Border = PdfPCell.NO_BORDER;
                        logoCell.HorizontalAlignment = Element.ALIGN_LEFT;
                        logoCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        logoCell.Padding = 5;
                        headerTable.AddCell(logoCell);
                    }
                    else
                    {
                        headerTable.AddCell(new PdfPCell(new Phrase("Logo no encontrado", FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8, BaseColor.RED))) { Border = PdfPCell.NO_BORDER });
                    }

                    PdfPCell companyInfoCell = new PdfPCell();
                    companyInfoCell.Border = PdfPCell.NO_BORDER;
                    companyInfoCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    companyInfoCell.VerticalAlignment = Element.ALIGN_TOP;
                    companyInfoCell.Padding = 5;

                    Font fontCompanyName = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(54, 80, 106));
                    Font fontCompanyDetails = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);

                    companyInfoCell.AddElement(new Paragraph("VETWEB", fontCompanyName) { Alignment = Element.ALIGN_RIGHT });
                    companyInfoCell.AddElement(new Paragraph("Villa el Salvador, Lima, Perú", fontCompanyDetails) { Alignment = Element.ALIGN_RIGHT });
                    companyInfoCell.AddElement(new Paragraph("Teléfono: +51 907377938", fontCompanyDetails) { Alignment = Element.ALIGN_RIGHT });
                    companyInfoCell.AddElement(new Paragraph("Email: info@vetweb.com", fontCompanyDetails) { Alignment = Element.ALIGN_RIGHT });

                    headerTable.AddCell(companyInfoCell);
                    doc.Add(headerTable);

                    // Título del Reporte
                    Font reportTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, BaseColor.DARK_GRAY);
                    Paragraph reportTitle = new Paragraph("REPORTE COMPLETO DE CITA", reportTitleFont);
                    reportTitle.Alignment = Element.ALIGN_CENTER;
                    reportTitle.SpacingAfter = 15f;
                    doc.Add(reportTitle);

                    // Información del Documento (Folio, Fecha de Generación)
                    PdfPTable docDetailsTable = new PdfPTable(2);
                    docDetailsTable.WidthPercentage = 100;
                    docDetailsTable.DefaultCell.Border = PdfPCell.NO_BORDER;
                    docDetailsTable.SetWidths(new float[] { 1f, 1f });
                    docDetailsTable.SpacingAfter = 10f;

                    Font fontDocDetails = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);

                    docDetailsTable.AddCell(new PdfPCell(new Phrase($"FOLIO: {new Random().Next(100000, 999999)}", fontDocDetails)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT });
                    docDetailsTable.AddCell(new PdfPCell(new Phrase($"Fecha de Generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", fontDocDetails)) { Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

                    // Mostrar la cita ID como parte del filtro/contexto del reporte
                    docDetailsTable.AddCell(new PdfPCell(new Phrase($"Cita Reportada: Cita #{selectedCitaID}", fontDocDetails)) { Colspan = 2, Border = PdfPCell.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT });

                    doc.Add(docDetailsTable);

                    // ====================================================================
                    // 2. DETALLES DE LA CITA SELECCIONADA (Formato más simple, no tabla de 2 columnas)
                    // ====================================================================
                    // Agregamos la información de la cita como un párrafo o bloques de texto
                    Paragraph citaInfoParagraph = new Paragraph();
                    citaInfoParagraph.SetLeading(0f, 1.2f); // Espaciado entre líneas
                    citaInfoParagraph.SpacingAfter = 15f;

                    Font fontCitaDetailsLabel = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK);
                    Font fontCitaDetailsValue = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.DARK_GRAY);

                    citaInfoParagraph.Add(new Chunk("Fecha de Cita: ", fontCitaDetailsLabel));
                    citaInfoParagraph.Add(new Chunk(fechaCita + "\n", fontCitaDetailsValue));

                    citaInfoParagraph.Add(new Chunk("Cliente: ", fontCitaDetailsLabel));
                    citaInfoParagraph.Add(new Chunk(nombreCliente + "\n", fontCitaDetailsValue));

                    citaInfoParagraph.Add(new Chunk("Mascota: ", fontCitaDetailsLabel));
                    citaInfoParagraph.Add(new Chunk(nombreMascota + "\n", fontCitaDetailsValue));

                    citaInfoParagraph.Add(new Chunk("Empleado Asignado: ", fontCitaDetailsLabel));
                    citaInfoParagraph.Add(new Chunk(nombreEmpleado + "\n", fontCitaDetailsValue));

                    doc.Add(citaInfoParagraph);


                    // ====================================================================
                    // 3. TABLA DE SERVICIOS DE LA CITA (Con estilo de Reporte de Clientes)
                    // ====================================================================

                    if (dtCitaServicios.Rows.Count == 0)
                    {
                        Paragraph noServices = new Paragraph("No se registraron servicios para esta cita.", fontCompanyDetails);
                        noServices.Alignment = Element.ALIGN_CENTER;
                        doc.Add(noServices);
                    }
                    else
                    {
                        PdfPTable pdfTable = new PdfPTable(2); // Only 2 columns now: Servicio, Subtotal
                        pdfTable.WidthPercentage = 100;
                        pdfTable.SpacingBefore = 10f;
                        pdfTable.DefaultCell.Padding = 5;
                        pdfTable.HeaderRows = 1;

                        // The columns are: NombreServicio, Subtotal
                        float[] widths = new float[] { 4f, 2f }; // Adjusted widths for 2 columns
                        pdfTable.SetWidths(widths);

                        // Add column headers
                        Font fontHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);
                        BaseColor headerColor = new BaseColor(54, 80, 106);
                        string[] headers = { "Servicio", "Subtotal" }; // Updated headers

                        foreach (string header in headers)
                        {
                            PdfPCell headerCell = new PdfPCell(new Phrase(header, fontHeader));
                            headerCell.BackgroundColor = headerColor;
                            headerCell.HorizontalAlignment = Element.ALIGN_CENTER;
                            headerCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            headerCell.Padding = 7;
                            headerCell.BorderColor = BaseColor.LIGHT_GRAY;
                            pdfTable.AddCell(headerCell);
                        }

                        // Add data rows
                        Font fontCell = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                        foreach (DataRow row in dtCitaServicios.Rows)
                        {
                            PdfPCell cellServicio = new PdfPCell(new Phrase(row["NombreServicio"].ToString(), fontCell));
                            cellServicio.HorizontalAlignment = Element.ALIGN_LEFT;

                            PdfPCell cellSubtotal = new PdfPCell(new Phrase(Convert.ToDecimal(row["Subtotal"]).ToString("C", culturePE), fontCell));
                            cellSubtotal.HorizontalAlignment = Element.ALIGN_RIGHT;

                            // Alternate row background color for better readability
                            if (dtCitaServicios.Rows.IndexOf(row) % 2 == 0)
                            {
                                BaseColor lightGray = new BaseColor(245, 245, 245); // Very light gray for alternation
                                cellServicio.BackgroundColor = lightGray;
                                cellSubtotal.BackgroundColor = lightGray;
                            }

                            cellServicio.Padding = 5;
                            cellSubtotal.Padding = 5;

                            cellServicio.BorderColor = BaseColor.LIGHT_GRAY;
                            cellSubtotal.BorderColor = BaseColor.LIGHT_GRAY;

                            pdfTable.AddCell(cellServicio);
                            pdfTable.AddCell(cellSubtotal);
                        }

                        doc.Add(pdfTable);

                        // Show the total of the appointment
                        PdfPTable totalTable = new PdfPTable(2);
                        totalTable.WidthPercentage = 100;
                        totalTable.DefaultCell.Border = PdfPCell.NO_BORDER;
                        totalTable.SetWidths(new float[] { 4f, 2f });
                        totalTable.SpacingBefore = 10f;

                        Font fontTotalLabel = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.BLACK);
                        Font fontTotalValue = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(54, 80, 106));

                        PdfPCell totalLabelCell = new PdfPCell(new Phrase("TOTAL DE LA CITA:", fontTotalLabel));
                        totalLabelCell.Border = PdfPCell.NO_BORDER;
                        totalLabelCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        totalLabelCell.Padding = 5;
                        totalTable.AddCell(totalLabelCell);

                        PdfPCell totalValueCell = new PdfPCell(new Phrase(totalCita.ToString("C", culturePE), fontTotalValue));
                        totalValueCell.Border = PdfPCell.NO_BORDER;
                        totalValueCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        totalValueCell.Padding = 5;
                        totalTable.AddCell(totalValueCell);

                        doc.Add(totalTable);
                    }

                    // ====================================================================
                    // 4. PIE DE PÁGINA DEL DOCUMENTO
                    // ====================================================================
                    Font fontFooter = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9, BaseColor.GRAY);
                    Paragraph footerNote = new Paragraph($"Este es un reporte de servicios generado para la cita #{selectedCitaID} de VetWeb.", fontFooter);
                    footerNote.Alignment = Element.ALIGN_CENTER;
                    footerNote.SpacingBefore = 20f;
                    doc.Add(footerNote);

                    Paragraph thankYouNote = new Paragraph("Generado por VetWeb - Tu solución para la gestión veterinaria.", FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.LIGHT_GRAY));
                    thankYouNote.Alignment = Element.ALIGN_CENTER;
                    doc.Add(thankYouNote);

                    doc.Close();

                    // Send the PDF to the browser
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", $"attachment;filename=ReporteServiciosCita_#{selectedCitaID}.pdf");
                    Response.Buffer = true;
                    Response.Clear();
                    Response.BinaryWrite(ms.ToArray());
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al generar el PDF de servicios de la cita: " + ex.Message, false);
            }
            finally
            {
                if (doc.IsOpen())
                {
                    doc.Close();
                }
            }
        }
    }
}