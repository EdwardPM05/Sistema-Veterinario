<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CitaServicios.aspx.cs" Inherits="VetWeb.CitaServicios" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Gestión de Servicios de Citas - VetWeb</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    
    <!-- Favicon -->
 <link href="~/favicon.ico" rel="shortcut icon" type="image/x-icon" />
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

        /* Definición de variables CSS para colores (Paleta: Elegante y Moderna - Tonos Gris Azulado y Toques de Blanco/Plata) */
        :root {
            --body-bg: #F0F2F5; /* Fondo principal gris claro muy suave */
            --text-color: #333333; /* Texto general gris oscuro */

            --sidebar-bg: #2C3E50; /* Fondo de la barra lateral: Gris azulado oscuro */
            --sidebar-text: #EAECEE; /* Color de texto del sidebar: Gris muy claro */
            --sidebar-hover-bg: #34495E; /* Fondo al pasar el mouse por el sidebar: Gris azulado medio */
            --sidebar-hover-text: #FFFFFF; /* Texto al pasar el mouse por el sidebar: Blanco */
            
            --card-bg: #FFFFFF; /* Fondo de tarjeta: Blanco puro */
            --card-shadow: rgba(0,0,0,0.05); /* Sombra de tarjeta muy sutil */
            --card-header-bg: #5A7F9D; /* Encabezado de tarjeta/tabla/modal: Gris azulado más claro */
            --card-number-color: #343a40; /* Color de número de tarjeta (se mantiene oscuro para contraste) */
            
            /* Colores para alertas y botones de formulario específicos */
            --btn-custom-bg: #5A7F9D; /* Gris azulado más claro para botones principales */
            --btn-custom-hover-bg: #4A6572; /* Gris azulado ligeramente más oscuro */
            --btn-clear-bg: #95A5A6; /* Gris Plata para "Limpiar" */
            --btn-clear-hover-bg: #7F8C8D; /* Gris Plata más oscuro */
            --form-control-focus-border: #5A7F9D; /* Borde de foco para input */
            --form-control-focus-shadow: rgba(90, 127, 157, 0.25); /* Sombra de foco para input */
            --alert-success-bg: #d4edda;
            --alert-success-color: #155724;
            --alert-success-border: #badbcc;
            --alert-danger-bg: #f8d7da;
            --alert-danger-color: #721c24;
            --alert-danger-border: #f5c6cb;
            --alert-success-icon-color: #28a745; /* Verde de Bootstrap para el ícono de éxito */
            --alert-danger-icon-color: #DC3545; /* Rojo de Bootstrap para el ícono de peligro */
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

        /* Specific styles for forms, tables, modals */
        .btn-custom {
            background-color: var(--btn-custom-bg);
            color: white;
            border-radius: 8px;
            padding: 10px 20px;
            font-size: 1.1rem;
            transition: background-color 0.3s ease;
            border: none;
        }
        .btn-custom:hover {
            background-color: var(--btn-custom-hover-bg);
            color: white;
        }
        .table-striped tbody tr:nth-of-type(odd) {
            background-color: var(--card-bg); /* Use card-bg for odd rows */
        }
        .table-hover tbody tr:hover {
            background-color: rgba(0, 0, 0, 0.04);
        }
        .table-primary th {
            background-color: var(--card-header-bg);
            color: white;
            border-color: var(--card-header-bg);
        }
        .modal-header {
            background-color: var(--card-header-bg);
            color: white;
            border-top-left-radius: 10px;
            border-top-right-radius: 10px;
        }
        .modal-content {
            border-radius: 10px;
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
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            word-wrap: break-word;
            white-space: normal;
        }
        .alert-success {
            background-color: var(--alert-success-bg);
            color: var(--alert-success-color);
            border-color: var(--alert-success-border);
        }
        .alert-danger {
            background-color: var(--alert-danger-bg);
            color: var(--alert-danger-color);
            border-color: var(--alert-danger-border);
        }
        .alert-danger::before {
            content: "\2716";
            font-size: 1.2rem;
            margin-right: 0.5rem;
            vertical-align: middle;
            display: inline-block;
            line-height: 1;
            color: var(--alert-danger-icon-color);
        }
        .alert-success::before {
            content: "\2714";
            font-size: 1.2rem;
            margin-right: 0.5rem;
            vertical-align: middle;
            display: inline-block;
            line-height: 1;
            color: var(--alert-success-icon-color);
        }

        /* Add this new rule for your table */
        .table {
            border-radius: 10px; /* Apply border-radius to the table */
            overflow: hidden; /* Crucial to clip internal elements to the rounded corners */
            border-collapse: separate; /* Required for border-radius to work with table borders */
            border-spacing: 0; /* Ensures no gap between cells if border-collapse is separate */
        }

        /* If your table is typically wrapped inside a .card-body or similar,
           you might want to ensure the table itself also has the rounded corners.
           The .card-header already has rounded top corners.
           For the bottom corners, you'd want the table's bottom corners to align.
        */
        .table-primary th:first-child {
            border-top-left-radius: 10px; /* Match card-header */
        }
        .table-primary th:last-child {
            border-top-right-radius: 10px; /* Match card-header */
        }

        /* For the bottom corners of the table */
        .table tbody tr:last-child td:first-child {
            border-bottom-left-radius: 10px;
        }
        .table tbody tr:last-child td:last-child {
            border-bottom-right-radius: 10px;
        }


        /* Agrega o modifica estas reglas en tu sección <style> */

        /* Regla para quitar la línea debajo de los iconos (si acaso persistía) */
        .table .icon-action {
            text-decoration: none !important; /* Asegura que no haya subrayado */
            display: inline-block; /* Importante para que el margen y el padding funcionen bien */
            padding: 0; /* Elimina cualquier padding predeterminado */
            margin: 0; /* Elimina cualquier margen predeterminado */
            border: none !important; /* Asegura que no haya bordes inesperados */
            background-color: transparent !important; /* Fondo transparente */
            line-height: 1; /* Ayuda a controlar el espacio vertical */
            vertical-align: middle; /* Alinea los iconos verticalmente */
        }

        /* Espacio entre los iconos, aplicado al primer icono */
        .table .icon-action:first-of-type {
            margin-right: 15px; /* Ajusta este valor (ej. 10px, 20px) para el espacio deseado entre el lápiz y el tacho */
        }

        /* Efecto hover opcional para cuando pases el mouse por encima del icono */
        .table .icon-action:hover {
            opacity: 0.7; /* Hace el icono ligeramente transparente */
            transform: scale(1.1); /* Hace el icono ligeramente más grande */
            transition: opacity 0.2s ease-in-out, transform 0.2s ease-in-out;
        }



        /* Asegurar que el sr-only no afecte el layout visual */
        .sr-only {
            position: absolute;
            width: 1px;
            height: 1px;
            padding: 0;
            margin: -1px;
            overflow: hidden;
            clip: rect(0, 0, 0, 0);
            white-space: nowrap;
            border: 0;
        }

        .search-input-group .form-control {
            border-top-left-radius: 8px;
            border-bottom-left-radius: 8px;
            border-top-right-radius: 0;
            border-bottom-right-radius: 0;
            border-color: #ced4da;
            box-shadow: none;
        }
        .search-input-group .form-control:focus {
            border-color: var(--form-control-focus-border);
            box-shadow: 0 0 0 0.25rem var(--form-control-focus-shadow);
        }
        .search-input-group .btn {
            border-radius: 0;
            font-weight: 600;
            padding-left: 1rem;
            padding-right: 1rem;
        }
        /* Specific IDs for search buttons. CitaServicios.aspx uses btnBuscarCitaServicio and btnLimpiarBusquedaCitaServicio */
        .search-input-group #btnBuscarCitaServicio { 
            background-color: var(--btn-custom-bg);
            color: white;
            border-color: var(--btn-custom-bg);
            border-top-right-radius: 0;
            border-bottom-right-radius: 0;
        }
        .search-input-group #btnBuscarCitaServicio:hover {
            background-color: var(--btn-custom-hover-bg);
            border-color: var(--btn-custom-hover-bg);
        }
        .search-input-group #btnLimpiarBusquedaCitaServicio {
            background-color: var(--btn-clear-bg);
            color: white;
            border-color: var(--btn-clear-bg);
            border-top-right-radius: 8px;
            border-bottom-right-radius: 8px;
        }
        .search-input-group #btnLimpiarBusquedaCitaServicio:hover {
            background-color: var(--btn-clear-hover-bg);
            border-color: var(--btn-clear-hover-bg);
        }
        .search-input-group .btn:not(:last-child) {
            border-right: 1px solid rgba(0,0,0,.125);
        }
        /* Modal width adjustments */
        .modal-dialog {
            max-width: 600px;
            width: 90%;
        }

        @media (min-width: 576px) {
            .modal-dialog {
                max-width: 650px;
            }
        }
        @media (min-width: 768px) {
            .modal-dialog {
                max-width: 700px;
            }
        }
        
        /* Estilos para etiquetas de cálculo (Precio actual, Subtotal, Total de la Cita) */
        .calculation-label {
            font-weight: 600;
            font-size: 1.1rem;
            margin-top: 0.5rem;
            display: block; /* Para que cada label ocupe su propia línea */
            color: var(--text-color);
        }
        .total-cita-container {
            border-top: 2px solid var(--card-header-bg); /* Usar el color del encabezado de la tarjeta */
            padding-top: 1rem;
            margin-top: 1.5rem;
            text-align: right; /* Alinear el total a la derecha */
        }
        .total-cita-label {
            font-weight: 700;
            font-size: 1.4rem;
            color: var(--card-header-bg); /* Usar el color del encabezado de la tarjeta */
            display: block;
        }

        /* --- RESPONSIVE STYLES FOR MOBILE --- */
        @media (max-width: 767.98px) {
            .sidebar {
                left: -220px;
                box-shadow: none;
                z-index: 1040;
            }
            .sidebar.show {
                left: 0;
                box-shadow: 2px 0 5px var(--card-shadow);
            }

            body {
                padding-left: 0;
                padding-top: 5rem;
            }

            .content {
                padding: 1rem;
            }

            /* Responsive button to toggle sidebar */
            #sidebarToggle {
                display: flex !important;
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
                background-color: rgba(0, 0, 0, 0.5);
                z-index: 1039;
                display: none;
                transition: opacity 0.3s ease;
                opacity: 0;
            }
            .sidebar-backdrop.show {
                display: block;
                opacity: 1;
            }

            /* Prevent body scrolling when sidebar is open */
            body.overflow-hidden {
                overflow: hidden;
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager runat="server"></asp:ScriptManager>

        <!-- Sidebar Toggle Button for Mobile -->
        <button type="button" id="sidebarToggle" class="btn btn-primary d-md-none">
            <i class="bi bi-list fs-5"></i>
        </button>

        <!-- Barra de Navegación Lateral -->
        <nav class="sidebar" id="sidebarMenu">
            <a href="Default.aspx" class="sidebar-brand">
                <i class="bi bi-hospital-fill"></i> VetWeb
            </a>
            <ul class="nav flex-column">
                <li class="nav-item">
                    <a class="nav-link" href="Default.aspx">
                        <i class="bi bi-speedometer2"></i><span>Dashboard</span>
                    </a>
                </li>
                <li class="nav-item">
                    <a class="nav-link" href="Clientes.aspx">
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
                    <a class="nav-link" data-bs-toggle="collapse" href="#serviciosSubmenu" role="button" aria-expanded="true" aria-controls="serviciosSubmenu">
                        <i class="bi bi-tools"></i><span>Servicios</span>
                        <i class="bi bi-chevron-down dropdown-caret"></i>
                    </a>
                    <div class="collapse show" id="serviciosSubmenu"> <%-- 'show' class added to keep it open --%>
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
                    <a class="nav-link active" href="CitaServicios.aspx"> <%-- Marked as active --%>
                        <i class="bi bi-clipboard-check-fill"></i><span>Cita-Servicios</span>
                    </a>
                </li>

            </ul>
        </nav>

        <!-- Área de Contenido Principal -->
        <div class="content">


            <div class="row align-items-center mb-4">
                <div class="col-md-8">
                    <label for="<%= ddlCitas.ClientID %>" class="form-label">Seleccione una Cita para gestionar sus servicios:</label>
                    <asp:DropDownList ID="ddlCitas" runat="server" CssClass="form-select" AutoPostBack="True" OnSelectedIndexChanged="ddlCitas_SelectedIndexChanged">
                        <%-- Las citas se cargarán desde el code-behind --%>
                    </asp:DropDownList>
                    <asp:HiddenField ID="hfSelectedCitaID" runat="server" Value="" />
                </div>
                <div class="col-md-4 text-end d-flex justify-content-end align-items-center mt-md-4">
                    <button type="button" class="btn btn-custom me-2" data-bs-toggle="modal" data-bs-target="#citaServicioModal" data-mode="add">
                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-plus-circle me-2" viewBox="0 0 16 16">
                            <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14m0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16"/>
                            <path d="M8 4a.5.5 0 0 1 .5.5v3h3a.5.5 0 0 1 0 1h-3v3a.5.5 0 0 1-1 0v-3h-3a.5.5 0 0 1 0-1h3v-3A.5.5 0 0 1 8 4"/>
                        </svg>
                        Agregar Servicio
                    </button>
                    <asp:LinkButton ID="btnImprimirPdf" runat="server" CssClass="btn btn-custom" OnClick="btnImprimirPdf_Click">
                        <i class="bi bi-file-earmark-pdf me-2"></i> Reporte PDF
                    </asp:LinkButton>
                </div>
            </div>

            <hr />

            <!-- GridView para mostrar los servicios de la cita seleccionada -->
            <h3>Servicios para la Cita seleccionada: <asp:Label ID="lblInfoCitaSeleccionada" runat="server" Text=""></asp:Label></h3>
            <asp:GridView ID="gvCitaServicios" runat="server" AutoGenerateColumns="False" OnRowCommand="gvCitaServicios_RowCommand"
                CssClass="table table-bordered table-hover"
                HeaderStyle-CssClass="table-primary"
                DataKeyNames="CitaServicioID, CitaID, ServicioID"> <%-- Claves de datos para una edición robusta --%>
                <Columns>
                    <%-- Las columnas se muestran en el orden que se definen aquí --%>
                    <asp:BoundField DataField="NombreServicio" HeaderText="Servicio" />
                    <asp:BoundField DataField="Cantidad" HeaderText="Cantidad" />
                    <asp:BoundField DataField="PrecioUnitario" HeaderText="Precio Unitario" DataFormatString="{0:C}" />
                    <asp:BoundField DataField="TotalServicio" HeaderText="Subtotal" DataFormatString="{0:C}" />
                    <asp:TemplateField HeaderText="Acciones" ItemStyle-Width="80px">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnEditarCitaServicio" runat="server" CommandName="Editar"
                                CssClass="icon-action icon-edit-custom ms-2 me-2"
                                CommandArgument="<%# Container.DataItemIndex %>"
                                ToolTip="Editar CitaServicio">
                                <i class="bi bi-pencil fs-5"></i>
                                <span class="sr-only">Editar</span>
                            </asp:LinkButton>
                
                            <asp:LinkButton ID="btnEliminarCitaServicio" runat="server" CommandName="Eliminar"
                                CssClass="icon-action text-danger me-2"
                                CommandArgument="<%# Container.DataItemIndex %>"
                                OnClientClick="return confirm('¿Está seguro de que desea eliminar este servucui?');"
                                ToolTip="Eliminar Servicio">
                                <i class="bi bi-trash fs-5"></i>
                                <span class="sr-only">Eliminar</span>
                            </asp:LinkButton>
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
                        <asp:Label ID="lblMensaje" runat="server"></asp:Label><br /> <%-- Removed EnableViewState="false" --%>
                        
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
            });

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
