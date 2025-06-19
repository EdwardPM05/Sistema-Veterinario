<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Clientes.aspx.cs" Inherits="VetWeb.Clientes" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Gestión de Clientes - VetWeb</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" />
    <style>
        /* Global box-sizing for consistent layout */
        html, body {
            box-sizing: border-box;
        }
        *, *::before, *::after {
            box-sizing: inherit;
        }

        /* Definición de variables CSS para colores (solo modo claro) */
        :root {
            --body-bg: #f8f9fa;
            --text-color: #212529;
            --sidebar-bg: #343a40; /* Fondo del sidebar */
            --sidebar-text: #adb5bd; /* Color de texto del sidebar */
            --sidebar-hover-bg: #495057; /* Fondo al pasar el mouse por el sidebar */
            --sidebar-hover-text: #fff; /* Texto al pasar el mouse por el sidebar */
            --card-bg: #fff;
            --card-shadow: rgba(0,0,0,0.08);
            --card-header-bg: #0d6efd;
            --card-number-color: #343a40;
            --cita-card-bg-gradient-start: #e0f2f7;
            --cita-card-bg-gradient-end: #f0f8ff;
            --cita-card-border: #b3e0ff;
            --cita-card-shadow: rgba(0, 0, 0, 0.12);
            --cita-client-color: #212529;
            --cita-icon-client: #007bff;
            --cita-date-bubble-bg: #007bff;
            --cita-text-color: #495057;
            --cita-icon-mascota: #28a745;
            --cita-footer-border: #cceeff;
            --cita-footer-text: #6c757d;
            --cita-icon-empleado: #6f42c1;
            --no-citas-bg: #f0f2f5;
            --no-citas-text: #6c757d;
        }

        body {
            min-height: 100vh;
            overflow-x: hidden; /* Prevent horizontal scroll for the entire body */
            font-family: 'Inter', sans-serif;
            background-color: var(--body-bg);
            color: var(--text-color);
            transition: background-color 0.3s, color 0.3s;
            padding-left: 220px; /* Space for fixed sidebar on desktop */
        }
        
        /* Sidebar styling for desktop/tablet */
        .sidebar {
            position: fixed;
            top: 0;
            width: 220px;
            height: 100vh;
            background-color: var(--sidebar-bg);
            padding-top: 1rem;
            box-shadow: 2px 0 5px var(--card-shadow);
            overflow-y: auto; /* Scrollbar appears only when content overflows vertically */
            overflow-x: hidden; /* **CRITICAL: Ensure no horizontal scrollbar within sidebar** */
            z-index: 1030; /* Higher than content */
            transition: left 0.3s ease; /* Smooth transition for sliding */
            left: 0; /* Default position for large screens */
        }
        .sidebar .nav-link {
            color: var(--sidebar-text);
            font-weight: 500;
            padding: 12px 20px; /* Consistent padding-left for all nav links */
            transition: background-color 0.3s, color 0.3s;
            border-radius: 8px;
            margin: 0 10px 5px 10px; /* Margin around the link item */
            display: flex; /* Make it a flex container */
            align-items: center; /* Vertically align items */
            justify-content: space-between; /* Pushes caret to the right, allows space for text */
            white-space: nowrap; /* Keep content on one line */
            overflow: hidden; /* Hide overflow of content within the link */
            text-overflow: ellipsis; /* Show ellipsis for overflowing text */
        }
        .sidebar .nav-link i { /* Style for Bootstrap Icons (main and sub-menu icons) */
            margin-right: 10px; /* Space between icon and text */
            font-size: 1.1rem;
            width: 20px; /* Fixed width for icons to align text */
            text-align: center;
            flex-shrink: 0; /* **IMPORTANT: Prevent icon from shrinking** */
        }
        .sidebar .nav-link span { /* For text within nav-link */
            flex-grow: 1; /* Allow text to grow and take available space */
            flex-shrink: 1; /* **IMPORTANT: Allow text to shrink if necessary** */
            min-width: 0; /* **CRITICAL: Allows flex item to shrink properly with text-overflow** */
        }
        .sidebar .nav-link:hover, .sidebar .nav-link.active {
            background-color: var(--sidebar-hover-bg);
            color: var(--sidebar-hover-text);
        }
        .sidebar-brand {
            color: var(--sidebar-hover-text);
            font-size: 1.8rem;
            font-weight: 700;
            padding: 0 20px 1rem;
            border-bottom: 1px solid var(--sidebar-hover-bg);
            margin-bottom: 1rem;
            display: flex; /* Para alinear el icono */
            align-items: center; /* Para alinear el icono */
            justify-content: center; /* Centrar el contenido de la marca */
            text-decoration: none;
        }
        .sidebar-brand svg {
            margin-right: 10px;
            font-size: 2.2rem;
        }

        /* Content area positioning for desktop/tablet */
        .content {
            margin-left: 0; /* Content starts after sidebar's padding-left */
            padding: 2rem;
            position: relative;
        }
        h2 {
            color: var(--text-color);
            transition: color 0.3s;
        }
        .card {
            border-radius: 10px;
            box-shadow: 0 4px 10px var(--card-shadow);
            transition: transform 0.2s ease-in-out, background-color 0.3s, box-shadow 0.3s;
            background-color: var(--card-bg);
        }
        .card:hover {
            transform: translateY(-5px);
        }
        .card-header {
            background-color: var(--card-header-bg);
            color: white;
            font-weight: 600;
            border-top-left-radius: 10px;
            border-top-right-radius: 10px;
            transition: background-color 0.3s;
        }
        .card-body h3 {
            font-size: 2.5rem;
            font-weight: 700;
            color: var(--card-number-color);
            transition: color 0.3s;
        }
        .card-text.text-muted {
            color: var(--text-color) !important;
        }

        /* Styles for dropdown caret */
        .dropdown-caret {
            transition: transform 0.3s ease;
            margin-left: 8px;
            flex-shrink: 0;
            font-size: 0.9rem;
        }
        /* Rotate caret when collapse is open */
        .nav-link[aria-expanded="true"] .dropdown-caret {
            transform: rotate(-180deg);
        }

        /* Styles for collapsible sub-menu items */
        .sidebar .nav-item .collapse .nav-item {
            margin-left: 20px;
            margin-bottom: 2px;
            margin-right: 0;
        }
        .sidebar .nav-item .collapse .nav-link {
            justify-content: flex-start;
        }
        .sidebar .nav-item .collapse .nav-link i {
            font-size: 0.95rem;
            width: 18px;
            flex-shrink: 0;
            margin-right: 8px;
            text-align: center;
        }

        /* --- RESPONSIVE STYLES FOR MOBILE --- */
        @media (max-width: 767.98px) {
            .sidebar {
                left: -220px; /* Hide sidebar off-screen on small devices by default */
                box-shadow: none; /* Remove shadow when hidden initially */
                z-index: 1040; /* Higher z-index for overlay effect when open */
            }
            .sidebar.show {
                left: 0; /* Bring sidebar into view when toggled */
                box-shadow: 2px 0 5px var(--card-shadow); /* Re-add shadow when shown */
            }

            body {
                padding-left: 0; /* No left padding on small screens */
                padding-top: 5rem; /* Space for the top toggle button */
            }

            .content {
                padding: 1rem; /* Adjust padding for content on small screens */
            }

            /* Responsive button to toggle sidebar */
            #sidebarToggle {
                display: flex !important; /* Make sure it's visible on small screens */
                position: fixed;
                top: 15px;
                left: 15px;
                z-index: 1050;
                border-radius: 50%;
                width: 45px;
                height: 45px;
                align-items: center;
                justify-content: center;
                background-color: var(--sidebar-bg);
                border-color: var(--sidebar-hover-bg);
                color: var(--sidebar-text);
                transition: background-color 0.3s, color 0.3s, border-color 0.3s;
            }
            #sidebarToggle:hover {
                background-color: var(--sidebar-hover-bg);
                color: var(--sidebar-hover-text);
            }

            /* Backdrop for when sidebar is open on mobile */
            .sidebar-backdrop {
                position: fixed;
                top: 0;
                left: 0;
                width: 100%;
                height: 100%;
                background-color: rgba(0, 0, 0, 0.5); /* Semi-transparent black overlay */
                z-index: 1039; /* Just below sidebar */
                display: none; /* Hidden by default */
                transition: opacity 0.3s ease; /* Smooth fade for backdrop */
                opacity: 0;
            }
            .sidebar-backdrop.show {
                display: block; /* Show backdrop when sidebar is open */
                opacity: 1;
            }

            /* Prevent body scrolling when sidebar is open */
            body.overflow-hidden {
                overflow: hidden;
            }
        }
        
        /* Specific styles for Clientes.aspx content */
        .btn-custom {
            background-color: #6f42c1; /* Custom purple */
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
        /* Table enhancements */
        .table-striped tbody tr:nth-of-type(odd) {
            background-color: #fff; /* Force white background for odd rows */
        }
        .table-hover tbody tr:hover {
            background-color: rgba(0, 0, 0, 0.075);
        }
        .table-primary th {
            background-color: #0d6efd; /* Bootstrap primary blue */
            color: white;
            border-color: #0d6efd;
        }
        .modal-header {
            background-color: #0d6efd; /* Primary blue header */
            color: white;
            border-top-left-radius: 10px;
            border-top-right-radius: 10px;
        }
        .modal-content {
            border-radius: 10px; /* Rounded corners for modal */
            box-shadow: 0 0 20px rgba(0,0,0,0.2);
        }
        .form-control {
            border-radius: 8px;
        }
        .btn {
            border-radius: 8px;
        }
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

        /* Search Bar Specific Styles */
        .search-input-group .form-control {
            border-top-left-radius: 8px;
            border-bottom-left-radius: 8px;
            border-top-right-radius: 0;
            border-bottom-right-radius: 0;
            border-color: #ced4da; /* Default Bootstrap border */
            box-shadow: none; /* Remove default focus shadow */
        }
        .search-input-group .form-control:focus {
            border-color: #80bdff; /* Bootstrap blue focus */
            box-shadow: 0 0 0 0.25rem rgba(13, 110, 253, 0.25); /* Bootstrap focus shadow */
        }

        .search-input-group .btn {
            /* Common styles for search buttons */
            border-radius: 0; /* Remove default radius inherited from .btn */
            font-weight: 600; /* Slightly bolder text */
            padding-left: 1rem;
            padding-right: 1rem;
        }

        /* Specific styles for "Buscar" button */
        .search-input-group #btnBuscarCliente { /* Updated ID */
            background-color: #0d6efd; /* Primary blue */
            color: white;
            border-color: #0d6efd;
            border-top-right-radius: 0;
            border-bottom-right-radius: 0;
        }
        .search-input-group #btnBuscarCliente:hover { /* Updated ID */
            background-color: #0b5ed7; /* Darker blue on hover */
            border-color: #0b5ed7;
        }

        /* Specific styles for "Limpiar" button */
        .search-input-group #btnLimpiarBusquedaCliente { /* Updated ID */
            background-color: #6c757d; /* Secondary gray */
            color: white;
            border-color: #6c757d;
            border-top-right-radius: 8px; /* Rounded corner on the far right */
            border-bottom-right-radius: 8px;
        }
        .search-input-group #btnLimpiarBusquedaCliente:hover { /* Updated ID */
            background-color: #5c636a; /* Darker gray on hover */
            border-color: #565e64;
        }

        /* Ensure correct border-radius for input-group buttons */
        .search-input-group .btn:not(:last-child) {
            border-right: 1px solid rgba(0,0,0,.125); /* Small separator between buttons */
        }

        /* Custom styles for alert messages within the modal */
        #clienteModal .alert { /* Changed to clienteModal */
            font-size: 1rem; /* Slightly larger text for readability */
            padding: 0.75rem 1.25rem; /* More comfortable padding */
            margin-top: 1rem; /* Space above the message */
            margin-bottom: 1.5rem; /* Space below the message */
            text-align: center; /* Center the text */
            font-weight: 600; /* Bolder text for emphasis */
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); /* Subtle shadow for depth */
            word-wrap: break-word; /* Ensures long words break to fit */
            white-space: normal; /* Allows text to wrap naturally */
        }

        #clienteModal .alert-danger { /* Changed to clienteModal */
            background-color: #f8d7da; /* Light red */
            color: #721c24; /* Dark red text */
            border-color: #f5c6cb; /* Red border */
            position: relative; /* Needed for pseudo-elements */
        }

        #clienteModal .alert-danger::before { /* Changed to clienteModal */
            content: "\2716"; /* Unicode 'heavy multiplication x' for a cross */
            font-size: 1.2rem;
            margin-right: 0.5rem;
            vertical-align: middle;
            display: inline-block;
            line-height: 1; /* Align with text */
            color: #dc3545; /* Bootstrap red */
        }

        #clienteModal .alert-success::before { /* Changed to clienteModal */
            content: "\2714"; /* Unicode 'heavy check mark' */
            font-size: 1.2rem;
            margin-right: 0.5rem;
            vertical-align: middle;
            display: inline-block;
            line-height: 1;
            color: #28a745; /* Bootstrap green */
        }

        /* Increase modal width for better display of long messages */
        #clienteModal .modal-dialog { /* Changed to clienteModal */
            max-width: 650px; /* Slightly larger for client form */
            width: 90%; /* Responsive width */
        }

        @media (min-width: 576px) {
            #clienteModal .modal-dialog {
                max-width: 700px; /* Increase max-width for small screens and up */
            }
        }
        @media (min-width: 768px) {
            #clienteModal .modal-dialog {
                max-width: 800px; /* Increase max-width for medium screens and up */
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager runat="server"></asp:ScriptManager>

        <button type="button" id="sidebarToggle" class="btn btn-primary d-md-none">
            <i class="bi bi-list fs-5"></i>
        </button>

        <nav class="sidebar" id="sidebarMenu">
            <a href="Default.aspx" class="sidebar-brand">
                VetWeb
            </a>
            <ul class="nav flex-column">
                <li class="nav-item">
                    <a class="nav-link" href="Default.aspx">
                        <i class="bi bi-speedometer2"></i><span>Dashboard</span>
                    </a>
                </li>
                <li class="nav-item">
                    <a class="nav-link active" href="Clientes.aspx">
                        <i class="bi bi-people-fill"></i><span>Clientes</span>
                    </a>
                </li>
                
                <li class="nav-item">
                    <a class="nav-link" data-bs-toggle="collapse" href="#mascotasSubmenu" role="button" aria-expanded="false" aria-controls="mascotasSubmenu">
                        <i class="bi bi-heart-fill"></i><span>Mascotas</span> 
                        <i class="bi bi-chevron-down dropdown-caret"></i>
                    </a>
                    <div class="collapse" id="mascotasSubmenu">
                        <ul class="nav flex-column">
                            <li class="nav-item"><a class="nav-link" href="Mascotas.aspx"><i class="bi bi-heart-fill"></i><span>Ver Mascotas</span></a></li>
                            <li class="nav-item"><a class="nav-link" href="Razas.aspx"><i class="bi bi-gem"></i><span>Razas</span></a></li>
                            <li class="nav-item"><a class="nav-link" href="Especies.aspx"><i class="bi bi-tags-fill"></i><span>Especies</span></a></li>
                        </ul>
                    </div>
                </li>

                <li class="nav-item">
                    <a class="nav-link" data-bs-toggle="collapse" href="#empleadosSubmenu" role="button" aria-expanded="false" aria-controls="empleadosSubmenu">
                        <i class="bi bi-briefcase-fill"></i><span>Empleados</span>
                        <i class="bi bi-chevron-down dropdown-caret"></i>
                    </a>
                    <div class="collapse" id="empleadosSubmenu">
                        <ul class="nav flex-column">
                            <li class="nav-item"><a class="nav-link" href="Empleados.aspx"><i class="bi bi-briefcase-fill"></i><span>Ver Empleados</span></a></li>
                            <li class="nav-item"><a class="nav-link" href="Roles.aspx"><i class="bi bi-person-badge-fill"></i><span>Roles</span></a></li>
                        </ul>
                    </div>
                </li>

                <li class="nav-item">
                    <a class="nav-link" data-bs-toggle="collapse" href="#serviciosSubmenu" role="button" aria-expanded="false" aria-controls="serviciosSubmenu">
                        <i class="bi bi-tools"></i><span>Servicios</span>
                        <i class="bi bi-chevron-down dropdown-caret"></i>
                    </a>
                    <div class="collapse" id="serviciosSubmenu">
                        <ul class="nav flex-column">
                            <li class="nav-item"><a class="nav-link" href="Servicios.aspx"><i class="bi bi-tools"></i><span>Ver Servicios</span></a></li>
                            <li class="nav-item"><a class="nav-link" href="CategoriasProductos.aspx"><i class="bi bi-boxes"></i><span>Cat. Productos</span></a></li>
                            <li class="nav-item"><a class="nav-link" href="Subcategoria.aspx"><i class="bi bi-box-seam-fill"></i><span>Subcategorías</span></a></li>
                        </ul>
                    </div>
                </li>
                
                <li class="nav-item">
                    <a class="nav-link" href="Citas.aspx">
                        <i class="bi bi-calendar-check-fill"></i><span>Citas</span>
                    </a>
                </li>
                <li class="nav-item">
                    <a class="nav-link" href="CitaServicios.aspx">
                        <i class="bi bi-clipboard-check-fill"></i><span>Cita-Servicios</span>
                    </a>
                </li>
                <li class="nav-item">
                    <a class="nav-link" href="VentaServicios.aspx">
                        <i class="bi bi-currency-dollar"></i><span>Venta Servicios</span>
                    </a>
                </li>
            </ul>
        </nav>

        <div class="content">
            <h2 class="mb-4">Gestión de Clientes</h2>

            <div class="input-group mb-3 search-input-group">
                <asp:TextBox ID="txtBuscarNombreCliente" runat="server" CssClass="form-control" Placeholder="Buscar por nombre o DNI" />
                <asp:Button ID="btnBuscarCliente" runat="server" CssClass="btn btn-outline-secondary" Text="Buscar" OnClick="btnBuscarCliente_Click" />
                <asp:Button ID="btnLimpiarBusquedaCliente" runat="server" CssClass="btn btn-outline-secondary" Text="Limpiar" OnClick="btnLimpiarBusquedaCliente_Click" />
            </div>

            <button type="button" class="btn btn-custom mb-4" data-bs-toggle="modal" data-bs-target="#clienteModal" data-mode="add">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-person-plus me-2" viewBox="0 0 16 16">
                    <path d="M6 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6m2-3a2 2 0 1 1-4 0 2 2 0 0 1 4 0m4 8c0 1-1 1-1 1H1s-1 0-1-1 1-4 6-4 6 3 6 4m-1-.004c-.001-.246-.154-.986-.832-1.664C11.516 10.68 10.289 10 8 10s-3.516.68-4.168 1.332c-.678.678-.83 1.418-.832 1.664z"/>
                    <path fill-rule="evenodd" d="M13.5 5a.5.5 0 0 1 .5.5V7h1.5a.5.5 0 0 1 0 1H14v1.5a.5.5 0 0 1-1 0V8h-1.5a.5.5 0 0 1 0-1H13V5.5a.5.5 0 0 1 .5-.5"/>
                </svg>
                Agregar Nuevo Cliente
            </button>

            <hr />

            <asp:GridView ID="gvClientes" runat="server" AutoGenerateColumns="False" OnRowCommand="gvClientes_RowCommand"
                CssClass="table table-bordered table-hover"
                HeaderStyle-CssClass="table-primary"
                DataKeyNames="ClienteID">
                <Columns>
                    <%-- Removed ClienteID column from display --%>
                    <asp:BoundField DataField="PrimerNombre" HeaderText="Nombre" />
                    <asp:BoundField DataField="ApellidoPaterno" HeaderText="Paterno" />
                    <asp:BoundField DataField="ApellidoMaterno" HeaderText="Materno" />
                    <asp:BoundField DataField="DNI" HeaderText="DNI" />
                    <asp:BoundField DataField="Telefono" HeaderText="Teléfono" />
                    <asp:BoundField DataField="Direccion" HeaderText="Dirección" />
                    <asp:BoundField DataField="Correo" HeaderText="Correo" />
                    <asp:TemplateField HeaderText="Acciones" ItemStyle-Width="180px">
                        <ItemTemplate>
                            <asp:Button ID="btnEditarCliente" runat="server" CommandName="Editar" Text="Editar" CssClass="btn btn-warning btn-sm me-2" CommandArgument="<%# Container.DataItemIndex %>" />
                            <asp:Button ID="btnEliminarCliente" runat="server" CommandName="Eliminar" Text="Eliminar" CssClass="btn btn-danger btn-sm" CommandArgument="<%# Container.DataItemIndex %>" OnClientClick="return confirm('¿Está seguro de que desea eliminar este cliente?');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <div class="modal fade" id="clienteModal" tabindex="-1" aria-labelledby="clienteModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="clienteModalLabel">Gestión de Cliente</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <asp:Label ID="lblMensaje" runat="server" EnableViewState="false"></asp:Label><br />
                       
                        <div class="mb-3">
                            <label for="txtPrimerNombre" class="form-label">Primer Nombre</label>
                            <asp:TextBox ID="txtPrimerNombre" runat="server" CssClass="form-control" Placeholder="Primer Nombre" />
                        </div>
                        <div class="mb-3">
                            <label for="txtApellidoPaterno" class="form-label">Apellido Paterno</label>
                            <asp:TextBox ID="txtApellidoPaterno" runat="server" CssClass="form-control" Placeholder="Apellido Paterno" />
                        </div>
                        <div class="mb-3">
                            <label for="txtApellidoMaterno" class="form-label">Apellido Materno</label>
                            <asp:TextBox ID="txtApellidoMaterno" runat="server" CssClass="form-control" Placeholder="Apellido Materno (Opcional)" />
                        </div>
                        <div class="mb-3">
                            <label for="txtDNI" class="form-label">DNI</label>
                            <asp:TextBox ID="txtDNI" runat="server" CssClass="form-control" Placeholder="DNI (8 dígitos)" MaxLength="8" TextMode="Number" />
                        </div>
                        <div class="mb-3">
                            <label for="txtTelefono" class="form-label">Teléfono</label>
                            <asp:TextBox ID="txtTelefono" runat="server" CssClass="form-control" Placeholder="Teléfono (9 dígitos)" MaxLength="9" TextMode="Number" />
                        </div>
                        <div class="mb-3">
                            <label for="txtDireccion" class="form-label">Dirección</label>
                            <asp:TextBox ID="txtDireccion" runat="server" CssClass="form-control" Placeholder="Dirección Completa" />
                        </div>
                        <div class="mb-3">
                            <label for="txtCorreo" class="form-label">Correo Electrónico</label>
                            <asp:TextBox ID="txtCorreo" runat="server" CssClass="form-control" Placeholder="ejemplo@dominio.com" TextMode="Email" />
                        </div>
                        
                        <asp:HiddenField ID="hfClienteID" runat="server" />
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>
                        <asp:Button ID="btnAgregar" runat="server" CssClass="btn btn-success" Text="Guardar" OnClick="btnAgregar_Click" />
                        <asp:Button ID="btnActualizar" runat="server" CssClass="btn btn-primary" Text="Actualizar" OnClick="btnActualizar_Click" />
                    </div>
                </div>
            </div>
        </div>

        <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
        <script>
            // JavaScript for handling the sidebar toggle on mobile
            document.addEventListener('DOMContentLoaded', function () {
                var sidebarToggle = document.getElementById('sidebarToggle');
                var sidebarMenu = document.getElementById('sidebarMenu');
                var body = document.body;
                var sidebarBackdrop = document.createElement('div');
                sidebarBackdrop.className = 'sidebar-backdrop';
                document.body.appendChild(sidebarBackdrop);

                // Function to toggle sidebar
                function toggleSidebar() {
                    sidebarMenu.classList.toggle('show');
                    sidebarBackdrop.classList.toggle('show');
                    body.classList.toggle('overflow-hidden'); // Prevent body scroll
                }

                if (sidebarToggle) {
                    sidebarToggle.addEventListener('click', function () {
                        toggleSidebar();
                    });
                }

                if (sidebarBackdrop) {
                    sidebarBackdrop.addEventListener('click', function () {
                        toggleSidebar();
                    });
                }

                // Close sidebar when a nav link is clicked on small screens
                var navLinks = document.querySelectorAll('.sidebar .nav-link');
                navLinks.forEach(function (link) {
                    link.addEventListener('click', function () {
                        if (window.innerWidth < 768) {
                            // Check if it's a collapsible parent or a direct link
                            var isCollapsibleParent = this.getAttribute('data-bs-toggle') === 'collapse';
                            if (!isCollapsibleParent) {
                                // Only close if it's not a parent of a collapsible menu
                                toggleSidebar();
                            }
                        }
                    });
                });

                // Clear form and set "Add" mode for Cliente Modal
                var clienteModal = document.getElementById('clienteModal');
                if (clienteModal) {
                    clienteModal.addEventListener('shown.bs.modal', function (event) {
                        var button = event.relatedTarget;
                        var isAddModeButton = button && button.getAttribute('data-mode') === 'add';

                        if (isAddModeButton) {
                            clearModalFormAndSetAddModeCliente();
                        }
                    });
                }
            });

            // JavaScript function to clear the form fields and set "Add" mode for Client Modal
            function clearModalFormAndSetAddModeCliente() {
                var txtPrimerNombre = document.getElementById('<%= txtPrimerNombre.ClientID %>');
                var txtApellidoPaterno = document.getElementById('<%= txtApellidoPaterno.ClientID %>');
                var txtApellidoMaterno = document.getElementById('<%= txtApellidoMaterno.ClientID %>');
                var txtDNI = document.getElementById('<%= txtDNI.ClientID %>');
                var txtTelefono = document.getElementById('<%= txtTelefono.ClientID %>');
                var txtDireccion = document.getElementById('<%= txtDireccion.ClientID %>');
                var txtCorreo = document.getElementById('<%= txtCorreo.ClientID %>');
                var hfClienteID = document.getElementById('<%= hfClienteID.ClientID %>');
                var lblMensaje = document.getElementById('<%= lblMensaje.ClientID %>');
                var btnAgregar = document.getElementById('<%= btnAgregar.ClientID %>');
                var btnActualizar = document.getElementById('<%= btnActualizar.ClientID %>');
                var modalTitle = document.getElementById('clienteModalLabel');

                if (txtPrimerNombre) txtPrimerNombre.value = '';
                if (txtApellidoPaterno) txtApellidoPaterno.value = '';
                if (txtApellidoMaterno) txtApellidoMaterno.value = '';
                if (txtDNI) txtDNI.value = '';
                if (txtTelefono) txtTelefono.value = '';
                if (txtDireccion) txtDireccion.value = '';
                if (txtCorreo) txtCorreo.value = '';
                if (hfClienteID) hfClienteID.value = '';
                if (lblMensaje) {
                    lblMensaje.innerHTML = '';
                    lblMensaje.className = '';
                }

                if (btnAgregar) btnAgregar.style.display = 'inline-block';
                if (btnActualizar) btnActualizar.style.display = 'none';

                if (modalTitle) modalTitle.innerText = 'Agregar Nuevo Cliente';
            }

            // JavaScript function to show the Client modal (called from C#)
            function showClienteModal() {
                var myModal = new bootstrap.Modal(document.getElementById('clienteModal'));
                myModal.show();
            }

            // JavaScript function to hide the Client modal (called from C#)
            function hideClienteModal() {
                var myModal = bootstrap.Modal.getInstance(document.getElementById('clienteModal'));
                if (myModal) {
                    myModal.hide();
                }
            }
        </script>
    </form>
</body>
</html>