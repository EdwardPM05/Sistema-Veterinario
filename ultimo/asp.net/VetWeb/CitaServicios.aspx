<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CitaServicios.aspx.cs" Inherits="VetWeb.CitaServicios" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Gestión de Servicios de Citas - VetWeb</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        /* Estilos generales y de la barra lateral (consistentes con otras páginas) */
        body {
            min-height: 100vh;
            overflow-x: hidden;
            font-family: 'Inter', sans-serif; /* Usando fuente Inter */
            background-color: #f8f9fa; /* Fondo claro */
        }
        .sidebar {
            position: fixed;
            top: 0;
            left: 0;
            height: 100vh;
            width: 220px;
            background-color: #343a40;
            padding-top: 1rem;
            box-shadow: 2px 0 5px rgba(0,0,0,0.1); /* Sombra sutil */
        }
        .sidebar .nav-link {
            color: #adb5bd;
            font-weight: 500;
            padding: 12px 20px;
            transition: background-color 0.3s, color 0.3s; /* Transiciones suaves */
            border-radius: 8px; /* Esquinas redondeadas para enlaces de navegación */
            margin: 0 10px 5px 10px; /* Espaciado */
        }
        .sidebar .nav-link:hover, .sidebar .nav-link.active {
            background-color: #495057;
            color: #fff;
        }
        .sidebar-brand {
            color: #fff;
            font-size: 1.8rem; /* Fuente ligeramente más grande */
            font-weight: 700;
            padding: 0 20px 1rem;
            border-bottom: 1px solid #495057;
            margin-bottom: 1rem;
            display: block;
            text-decoration: none;
            text-align: center;
        }
        .content {
            margin-left: 220px;
            padding: 2rem;
        }
        .btn-custom {
            background-color: #6f42c1; /* Púrpura personalizado */
            color: white;
            border-radius: 8px;
            padding: 10px 20px;
            font-size: 1.1rem;
            transition: background-color 0.3s ease;
        }
        .btn-custom:hover {
            background-color: #59359a;
            color: white;
        }
        /* Mejoras en la tabla */
        .table-striped tbody tr:nth-of-type(odd) {
            background-color: rgba(0, 0, 0, 0.03);
        }
        .table-hover tbody tr:hover {
            background-color: rgba(0, 0, 0, 0.075);
        }
        .table-primary th {
            background-color: #0d6efd; /* Azul primario de Bootstrap */
            color: white;
            border-color: #0d6efd;
        }
        /* Estilos del modal */
        .modal-header {
            background-color: #0d6efd; /* Encabezado azul primario */
            color: white;
            border-top-left-radius: 10px;
            border-top-right-radius: 10px;
        }
        .modal-content {
            border-radius: 10px; /* Esquinas redondeadas para el modal */
            box-shadow: 0 0 20px rgba(0,0,0,0.2);
        }
        .form-control {
            border-radius: 8px;
        }
        .btn {
            border-radius: 8px;
        }
        /* Estilos de alertas */
        .alert {
            border-radius: 8px;
            padding: 10px 15px;
            margin-bottom: 15px;
            font-size: 0.95rem;
        }
        .alert-success {
            background-color: #d4edda;
            color: #155724;
            border-color: #badbcc;
        }
        .alert-danger {
            background-color: #f8d7da;
            color: #721c24;
            border-color: #f5c6cb;
        }

        /* Estilos específicos de la barra de búsqueda */
        .search-input-group .form-control {
            border-top-left-radius: 8px;
            border-bottom-left-radius: 8px;
            border-top-right-radius: 0;
            border-bottom-right-radius: 0;
            border-color: #ced4da;
            box-shadow: none;
        }
        .search-input-group .form-control:focus {
            border-color: #80bdff;
            box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25);
        }
        .search-input-group .btn {
            border-radius: 0;
            font-weight: 600;
            padding-left: 1rem;
            padding-right: 1rem;
        }
        .search-input-group #btnBuscarCitaServicio { 
            background-color: #0d6efd;
            color: white;
            border-color: #0d6efd;
            border-top-right-radius: 0;
            border-bottom-right-radius: 0;
        }
        .search-input-group #btnBuscarCitaServicio:hover { 
            background-color: #0b5ed7;
            border-color: #0b5ed7;
        }
        .search-input-group #btnLimpiarBusquedaCitaServicio { 
            background-color: #6c757d;
            color: white;
            border-color: #6c757d;
            border-top-right-radius: 8px;
            border-bottom-right-radius: 8px;
        }
        .search-input-group #btnLimpiarBusquedaCitaServicio:hover { 
            background-color: #5c636a;
            border-color: #565e64;
        }
        .search-input-group .btn:not(:last-child) {
            border-right: 1px solid rgba(0,0,0,.125);
        }

        /* Estilos personalizados para mensajes de alerta dentro del modal */
        #citaServicioModal .alert { 
            font-size: 1rem; 
            padding: 0.75rem 1.25rem; 
            margin-top: 1rem; 
            margin-bottom: 1.5rem; 
            text-align: center; 
            font-weight: 600; 
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); 
            word-wrap: break-word; 
            white-space: normal; 
        }

        /* Ícono solo si el mensaje no está vacío */
        #citaServicioModal .alert-danger:not(:empty)::before { 
            content: "\2716"; /* Símbolo de "x" grande */
            font-size: 1.2rem;
            margin-right: 0.5rem;
            vertical-align: middle;
            display: inline-block;
            line-height: 1; 
            color: #dc3545; /* Rojo de Bootstrap */
        }

        /* Ícono solo si el mensaje no está vacío */
        #citaServicioModal .alert-success:not(:empty)::before { 
            content: "\2714"; /* Símbolo de "check" grande */
            font-size: 1.2rem;
            margin-right: 0.5rem;
            vertical-align: middle;
            display: inline-block;
            line-height: 1;
            color: #28a745; /* Verde de Bootstrap */
        }

        /* Ajustes de ancho del modal para mejor visualización */
        #citaServicioModal .modal-dialog { 
            max-width: 600px; 
            width: 90%; 
        }

        @media (min-width: 576px) {
            #citaServicioModal .modal-dialog {
                max-width: 650px; 
            }
        }
        @media (min-width: 768px) {
            #citaServicioModal .modal-dialog {
                max-width: 700px; 
            }
        }
        
        /* Estilos para etiquetas de cálculo (Precio actual, Subtotal, Total de la Cita) */
        .calculation-label {
            font-weight: 600;
            font-size: 1.1rem;
            margin-top: 0.5rem;
            display: block; /* Para que cada label ocupe su propia línea */
        }
        .total-cita-container {
            border-top: 2px solid #0d6efd;
            padding-top: 1rem;
            margin-top: 1.5rem;
            text-align: right; /* Alinear el total a la derecha */
        }
        .total-cita-label {
            font-weight: 700;
            font-size: 1.4rem;
            color: #0d6efd;
            display: block;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <!-- Barra de Navegación Lateral -->
        <nav class="sidebar">
            <a href="Default.aspx" class="sidebar-brand">VetWeb</a>
            <ul class="nav flex-column">
                <li class="nav-item"><a class="nav-link" href="Default.aspx">Dashboard</a></li>
                <li class="nav-item"><a class="nav-link" href="Clientes.aspx">Clientes</a></li>
                <li class="nav-item"><a class="nav-link" href="Razas.aspx">Razas</a></li>
                <li class="nav-item"><a class="nav-link" href="Especies.aspx">Especies</a></li>
                <li class="nav-item"><a class="nav-link" href="Mascotas.aspx">Mascotas</a></li>
                <li class="nav-item"><a class="nav-link" href="Roles.aspx">Roles</a></li>
                <li class="nav-item"><a class="nav-link" href="Empleados.aspx">Empleados</a></li>
                <li class="nav-item"><a class="nav-link" href="CategoriasProductos.aspx">Cat. Productos</a></li>
                <li class="nav-item"><a class="nav-link" href="Subcategoria.aspx">Subcategorías</a></li>
                <li class="nav-item"><a class="nav-link" href="Servicios.aspx">Servicios</a></li>
                <li class="nav-item"><a class="nav-link" href="Citas.aspx">Citas</a></li>
                <li class="nav-item"><a class="nav-link active" href="CitaServicios.aspx">Cita-Servicios</a></li>
                <li class="nav-item"><a class="nav-link" href="VentaServicios.aspx">Venta Servicios</a></li>
            </ul>
        </nav>

        <!-- Área de Contenido Principal -->
        <div class="content">
            <h2 class="mb-4">Gestión de Servicios de Citas</h2>

            <!-- Selector de Cita y Botón para Agregar Nuevo Servicio a la Cita Seleccionada -->
            <div class="row align-items-center mb-4">
                <div class="col-md-8">
                    <label for="<%= ddlCitas.ClientID %>" class="form-label">Seleccione una Cita para gestionar sus servicios:</label>
                    <asp:DropDownList ID="ddlCitas" runat="server" CssClass="form-select" AutoPostBack="True" OnSelectedIndexChanged="ddlCitas_SelectedIndexChanged">
                        <%-- Las citas se cargarán desde el code-behind --%>
                    </asp:DropDownList>
                    <asp:HiddenField ID="hfSelectedCitaID" runat="server" Value="" />
                </div>
                <div class="col-md-4 text-end">
                    <button type="button" class="btn btn-custom mt-md-4" data-bs-toggle="modal" data-bs-target="#citaServicioModal" data-mode="add">
                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-plus-circle me-2" viewBox="0 0 16 16">
                            <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14m0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16"/>
                            <path d="M8 4a.5.5 0 0 1 .5.5v3h3a.5.5 0 0 1 0 1h-3v3a.5.5 0 0 1-1 0v-3h-3a.5.5 0 0 1 0-1h3v-3A.5.5 0 0 1 8 4"/>
                        </svg>
                        Agregar Servicio a Cita
                    </button>
                </div>
            </div>

            <hr />

            <!-- GridView para mostrar los servicios de la cita seleccionada -->
            <h3>Servicios para la Cita seleccionada: <asp:Label ID="lblInfoCitaSeleccionada" runat="server" Text=""></asp:Label></h3>
            <asp:GridView ID="gvCitaServicios" runat="server" AutoGenerateColumns="False" OnRowCommand="gvCitaServicios_RowCommand"
                CssClass="table table-striped table-bordered table-hover"
                HeaderStyle-CssClass="table-primary"
                DataKeyNames="CitaServicioID, CitaID, ServicioID"> <%-- Claves de datos para una edición robusta --%>
                <Columns>
                    <%-- Las columnas se muestran en el orden que se definen aquí --%>
                    <asp:BoundField DataField="NombreServicio" HeaderText="Servicio" />
                    <asp:BoundField DataField="Cantidad" HeaderText="Cantidad" />
                    <asp:BoundField DataField="PrecioUnitario" HeaderText="Precio Unitario" DataFormatString="{0:C}" />
                    <asp:BoundField DataField="TotalServicio" HeaderText="Subtotal" DataFormatString="{0:C}" />
                    <asp:TemplateField HeaderText="Acciones" ItemStyle-Width="180px">
                        <ItemTemplate>
                            <asp:Button ID="btnEditarCitaServicio" runat="server" CommandName="Editar" Text="Editar" CssClass="btn btn-warning btn-sm me-2" CommandArgument="<%# Container.DataItemIndex %>" />
                            <asp:Button ID="btnEliminarCitaServicio" runat="server" CommandName="Eliminar" Text="Eliminar" CssClass="btn btn-danger btn-sm" CommandArgument="<%# Container.DataItemIndex %>" OnClientClick="return confirm('¿Está seguro de que desea eliminar este servicio de la cita?');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>

            <!-- Total de la Cita -->
            <div class="total-cita-container">
                <asp:Label ID="lblTotalCita" runat="server" CssClass="total-cita-label" Text="Total de la Cita: S/ 0.00"></asp:Label>
            </div>
        </div>

        <!-- Modal de Bootstrap para Añadir/Editar Servicio de Cita -->
        <div class="modal fade" id="citaServicioModal" tabindex="-1" aria-labelledby="citaServicioModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="citaServicioModalLabel">Gestión de Servicio de Cita</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <!-- Etiqueta para mensajes de validación/éxito/error -->
                        <asp:Label ID="lblMensaje" runat="server" EnableViewState="false"></asp:Label><br />
                        
                        <!-- Campos de entrada del formulario -->
                        <div class="mb-3">
                            <label for="ddlServicios" class="form-label">Servicio</label>
                            <asp:DropDownList ID="ddlServicios" runat="server" CssClass="form-select" AutoPostBack="True" OnSelectedIndexChanged="ddlServicios_SelectedIndexChanged">
                                <%-- Los servicios se cargarán desde el code-behind --%>
                            </asp:DropDownList>
                        </div>
                        <div class="mb-3">
                            <label for="txtCantidad" class="form-label">Cantidad</label>
                            <asp:TextBox ID="txtCantidad" runat="server" CssClass="form-control" TextMode="Number" OnTextChanged="txtCantidad_TextChanged" AutoPostBack="True" />
                        </div>
                        
                        <!-- Información de Precio y Subtotal -->
                        <div class="mb-3">
                            <label class="calculation-label">Precio Unitario Actual: <asp:Label ID="lblPrecioUnitarioActual" runat="server" Text="S/ 0.00"></asp:Label></label>
                            <label class="calculation-label">Subtotal para este Servicio: <asp:Label ID="lblSubtotalServicio" runat="server" Text="S/ 0.00"></asp:Label></label>
                        </div>

                        <!-- Campo oculto para almacenar el ID de CitaServicio al editar -->
                        <asp:HiddenField ID="hfCitaServicioID" runat="server" />
                        <!-- Campo oculto para almacenar el PrecioUnitario original del servicio al editar/agregar -->
                        <asp:HiddenField ID="hfPrecioUnitarioGuardado" runat="server" />
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>
                        <!-- Botones de Guardar (Agregar) y Actualizar -->
                        <asp:Button ID="btnAgregar" runat="server" CssClass="btn btn-success" Text="Agregar Servicio" OnClick="btnAgregar_Click" />
                        <asp:Button ID="btnActualizar" runat="server" CssClass="btn btn-primary" Text="Actualizar Servicio" OnClick="btnActualizar_Click" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Archivos JavaScript de Bootstrap -->
        <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
        <script>
            // Función JavaScript para limpiar los campos del formulario del modal y establecer el modo "Agregar"
            function clearModalFormAndSetAddModeCitaServicio() {
                var ddlServicios = document.getElementById('<%= ddlServicios.ClientID %>');
                var txtCantidad = document.getElementById('<%= txtCantidad.ClientID %>');
                var lblPrecioUnitarioActual = document.getElementById('<%= lblPrecioUnitarioActual.ClientID %>');
                var lblSubtotalServicio = document.getElementById('<%= lblSubtotalServicio.ClientID %>');
                var hfCitaServicioID = document.getElementById('<%= hfCitaServicioID.ClientID %>');
                var hfPrecioUnitarioGuardado = document.getElementById('<%= hfPrecioUnitarioGuardado.ClientID %>');
                var lblMensaje = document.getElementById('<%= lblMensaje.ClientID %>');
                var btnAgregar = document.getElementById('<%= btnAgregar.ClientID %>');
                var btnActualizar = document.getElementById('<%= btnActualizar.ClientID %>');
                var modalTitle = document.getElementById('citaServicioModalLabel');

                // Restablecer los valores
                if (ddlServicios) ddlServicios.selectedIndex = 0; 
                if (txtCantidad) txtCantidad.value = '';
                if (lblPrecioUnitarioActual) lblPrecioUnitarioActual.innerText = 'S/ 0.00';
                if (lblSubtotalServicio) lblSubtotalServicio.innerText = 'S/ 0.00';
                if (hfCitaServicioID) hfCitaServicioID.value = '';
                if (hfPrecioUnitarioGuardado) hfPrecioUnitarioGuardado.value = '';
                
                // Limpiar el mensaje y sus clases para que no ocupe espacio
                if (lblMensaje) {
                    lblMensaje.innerHTML = '';
                    lblMensaje.className = ''; 
                }

                // Restablecer la visibilidad de los botones
                if (btnAgregar) btnAgregar.style.display = 'inline-block';
                if (btnActualizar) btnActualizar.style.display = 'none';

                if (modalTitle) modalTitle.innerText = 'Agregar Servicio a Cita';
            }

            // Función JavaScript para mostrar el modal (llamada desde C#)
            function showCitaServicioModal() {
                var myModal = new bootstrap.Modal(document.getElementById('citaServicioModal'));
                myModal.show();
            }

            // Función JavaScript para ocultar el modal (llamada desde C#)
            function hideCitaServicioModal() {
                var myModal = bootstrap.Modal.getInstance(document.getElementById('citaServicioModal'));
                if (myModal) {
                    myModal.hide();
                }
            }

            // Listener de evento para cuando el modal se muestra (para manejar el reseteo en modo 'agregar')
            document.addEventListener('DOMContentLoaded', function () {
                var citaServicioModal = document.getElementById('citaServicioModal');
                if (citaServicioModal) {
                    citaServicioModal.addEventListener('shown.bs.modal', function (event) {
                        var button = event.relatedTarget; 
                        // Solo limpiar si el modal fue activado por el botón 'Add New' (que tiene data-mode="add")
                        if (button && button.getAttribute('data-mode') === 'add') {
                            clearModalFormAndSetAddModeCitaServicio();
                        }
                    });
                    // Listener de evento para cuando el modal se oculta completamente
                    citaServicioModal.addEventListener('hidden.bs.modal', function () {
                        // Resetear la visibilidad de los botones para la próxima vez que se abra el modal en modo 'agregar'
                        clearModalFormAndSetAddModeCitaServicio(); 
                    });
                }
            });
        </script>
    </form>
</body>
</html>
