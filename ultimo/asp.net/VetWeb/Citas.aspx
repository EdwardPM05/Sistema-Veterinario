﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Citas.aspx.cs" Inherits="VetWeb.Citas" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Gestión de Citas - VetWeb</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" />
    
    <!-- Favicon -->
 <link href="~/favicon.ico" rel="shortcut icon" type="image/x-icon" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr/dist/flatpickr.min.css" />
        <style>
        /* Inclusión de la fuente Flexo Soft Demi */
        @font-face {
            font-family: 'Flexo Soft Demi';
            src: url('<%= ResolveUrl("~/Assets/Fonts/FlexoSoftDemi.woff") %>') format('woff');
            font-weight: 600;
            font-style: normal;
            font-display: swap;
        }

        /* Global box-sizing for consistent layout */
        html, body {
            box-sizing: border-box;
        }
        *, *::before, *::after {
            box-sizing: inherit;
        }

        /* Definición de variables CSS para colores */
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
            --alert-success-icon-color: #28a745;
            --alert-danger-icon-color: #DC3545;
        }

        body {
            min-height: 100vh;
            overflow-x: hidden;
            font-family: 'Flexo Soft Demi', 'Inter', sans-serif;
            background-color: var(--body-bg);
            color: var(--text-color);
            transition: background-color 0.3s, color 0.3s;
            padding-left: 220px;
        }
        
        /* Sidebar styling */
        .sidebar {
            position: fixed; top: 0; width: 220px; height: 100vh; background-color: var(--sidebar-bg);
            padding-top: 1rem; box-shadow: 2px 0 5px var(--card-shadow); overflow-y: auto;
            overflow-x: hidden; z-index: 1030; transition: left 0.3s ease; left: 0;
        }
        .sidebar .nav-link {
            color: var(--sidebar-text); font-weight: 500; padding: 12px 20px;
            transition: background-color 0.3s, color 0.3s; border-radius: 8px;
            margin: 0 10px 5px 10px; display: flex; align-items: center;
            justify-content: space-between; white-space: nowrap; overflow: hidden;
            text-overflow: ellipsis;
        }
        .sidebar .nav-link i {
            margin-right: 10px; font-size: 1.1rem; width: 20px; text-align: center; flex-shrink: 0;
        }
        .sidebar .nav-link span {
            flex-grow: 1; flex-shrink: 1; min-width: 0;
        }
        .sidebar .nav-link:hover, .sidebar .nav-link.active {
            background-color: var(--sidebar-hover-bg); color: var(--sidebar-hover-text);
        }
        .sidebar-brand {
            color: var(--sidebar-hover-text); font-size: 1.8rem; font-weight: 700;
            padding: 0 20px 1rem; border-bottom: 1px solid var(--sidebar-hover-bg);
            margin-bottom: 1rem; display: flex; align-items: center; justify-content: center; text-decoration: none;
        }
        .sidebar-brand svg {
            margin-right: 10px; font-size: 2.2rem;
        }

        /* Content area positioning */
        .content {
            margin-left: 0; padding: 2rem; position: relative;
        }
        h2 {
            color: var(--text-color); transition: color 0.3s;
        }
        .card {
            border-radius: 10px; box-shadow: 0 4px 10px var(--card-shadow);
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
            text-align: center;
            font-size: 1.1rem;
            padding: 15px;
            text-transform: uppercase;
        }
        .card-body h3 {
            font-size: 2.5rem; font-weight: 700; color: var(--card-number-color); transition: color 0.3s;
        }
        .card-text.text-muted {
            color: var(--text-color) !important;
        }

        /* Styles for dropdown caret */
        .dropdown-caret {
            transition: transform 0.3s ease; margin-left: 8px; flex-shrink: 0; font-size: 0.9rem;
        }
        .nav-link[aria-expanded="true"] .dropdown-caret {
            transform: rotate(-180deg);
        }

        /* Styles for collapsible sub-menu items */
        .sidebar .nav-item .collapse .nav-item {
            margin-left: 20px; margin-bottom: 2px; margin-right: 0;
        }
        .sidebar .nav-item .collapse .nav-link {
            justify-content: flex-start;
        }
        .sidebar .nav-item .collapse .nav-link i {
            font-size: 0.95rem; width: 18px; flex-shrink: 0; margin-right: 8px; text-align: center;
        }

        /* Specific styles for forms, tables, modals */
        .btn-custom {
            background-color: var(--btn-custom-bg); color: white; border-radius: 8px;
            padding: 10px 20px; font-size: 1.1rem; transition: background-color 0.3s ease; border: none;
        }
        .btn-custom:hover {
            background-color: var(--btn-custom-hover-bg); color: white;
        }

        /* --- STYLES PARA LA TABLA --- */
        .table {
            border-collapse: collapse;
            border-spacing: 0;
            border-radius: 0; /* No redondeado */
            box-shadow: 0 2px 5px rgba(0,0,0,0.08);
            background-color: var(--card-bg);
            border: 1px solid #EEEEEE; /* Borde exterior de la tabla */
        }

        .table thead {
            background-color: #F8F8F8;
        }

        .table th {
            color: #666666;
            font-weight: bold;
            text-transform: uppercase;
            font-size: 0.9em;
            padding: 10px 10px; /* Reducir padding horizontal para los encabezados */
            border: 1px solid #EEEEEE; /* Bordes para las celdas del encabezado */
            border-top: none;
            border-left: none;
            border-right: none;
            text-align: left; /* Alineación a la izquierda para los textos del encabezado */
        }

        /* Solo la primera y última th tendrán borde lateral para el "marco" */
        .table th:first-child {
            border-left: 1px solid #EEEEEE;
        }
        .table th:last-child {
            border-right: 1px solid #EEEEEE;
            text-align: right; /* La última columna (acciones) a la derecha */
            padding-right: 25px; /* Aumentar el padding derecho para "ACCIONES" */
            min-width: 100px; /* Asegura un poco más de espacio para "ACCIONES" */
        }

        /* Eliminar border-radius específicos para thead */
        .table th:first-child { border-top-left-radius: 0; }
        .table th:last-child { border-top-right-radius: 0; }

        /* **Mágico: Sobrescribimos las variables CSS de Bootstrap directamente en las celdas** */
        .table td {
            --bs-table-bg: var(--card-bg) !important;
            --bs-table-striped-bg: var(--card-bg) !important;
            background-color: var(--card-bg) !important;
            padding: 10px 10px; /* Reducir padding horizontal para las celdas de datos */
            color: var(--text-color);
            border: none; /* Quitar todos los bordes de TD por defecto */
            border-bottom: 1px solid #EEEEEE; /* Solo borde inferior para las filas */
            vertical-align: middle; /* Asegura que el contenido, incluyendo iconos, esté centrado verticalmente */
            text-align: left; /* Alineación a la izquierda para el contenido de las celdas de datos */
            font-size: 0.95em;
        }
        
        /* Aseguramos que la fila completa (tr) también sea blanca */
        .table tbody tr {
            background-color: var(--card-bg) !important;
        }

        /* Eliminar bordes inferiores para la última fila */
        .table tbody tr:last-child {
            border-bottom: none;
        }
        .table tbody tr:last-child td {
            border-bottom: none; /* Asegura que la última fila no tenga borde inferior */
        }
        /* Eliminar border-radius específicos para tbody */
        .table tbody tr:last-child td:first-child { border-bottom-left-radius: 0; }
        .table tbody tr:last-child td:last-child { border-bottom-right-radius: 0; }


        /* Sobreescribir .table-striped y .table-hover con mayor especificidad y !important */
        .table.table-striped > tbody > tr:nth-of-type(odd) {
            background-color: var(--card-bg) !important;
        }
        .table.table-hover > tbody > tr:hover {
            background-color: #F5F5F5 !important;
        }
        
        /* Para GridView si estás usando HeaderStyle CssClass="thead-light" */
        .table thead.thead-light th {
            background-color: #F8F8F8 !important;
            color: #666666 !important;
        }

        /* Estilos para los iconos de acción dentro de la tabla */
        .table .action-icons-cell {
            text-align: right; /* Alinear los iconos de acción a la derecha */
            white-space: nowrap;
            padding-right: 25px; /* Aumentar el padding derecho para la celda de acciones */
            /* min-width ya se definió en th:last-child para controlar el ancho de la columna */
        }

        .table .icon-action {
            text-decoration: none !important;
            display: inline-flex;
            align-items: center;
            justify-content: center;
            width: 25px;
            height: 25px;
            border-radius: 50%;
            color: #666666;
            font-size: 1em;
            transition: all 0.2s ease-in-out;
            background-color: transparent;
            border: none;
            cursor: pointer;
            vertical-align: middle;
        }
        .table .icon-action:hover {
            background-color: rgba(0,0,0,0.05);
            color: #333333;
            transform: translateY(-2px);
        }
        .table .icon-action.edit-icon {
            color: #007bff;
        }
        .table .icon-action.delete-icon {
            color: #dc3545;
        }
        .table .icon-action + .icon-action {
            margin-left: 3px;
        }


        /* Otros estilos que ya tenías */
        .modal-header {
            background-color: var(--card-header-bg); color: white; border-top-left-radius: 10px;
            border-top-right-radius: 10px;
        }
        .modal-content {
            border-radius: 10px; box-shadow: 0 0 20px rgba(0,0,0,0.2);
        }
        .form-control { border-radius: 8px; }
        .btn { border-radius: 8px; }
        .alert {
            border-radius: 8px; padding: 10px 15px; margin-bottom: 15px; font-size: 0.95rem;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); word-wrap: break-word; white-space: normal;
        }
        .alert-success { background-color: var(--alert-success-bg); color: var(--alert-success-color); border-color: var(--alert-success-border); }
        .alert-danger { background-color: var(--alert-danger-bg); color: var(--alert-danger-color); border-color: var(--alert-danger-border); }
        .alert-danger::before { content: "\2716"; font-size: 1.2rem; margin-right: 0.5rem; vertical-align: middle; display: inline-block; line-height: 1; color: var(--alert-danger-icon-color); }
        .alert-success::before { content: "\2714"; font-size: 1.2rem; margin-right: 0.5rem; vertical-align: middle; display: inline-block; line-height: 1; color: var(--alert-success-icon-color); }

        .search-input-group .form-control { border-top-left-radius: 8px; border-bottom-left-radius: 8px; border-top-right-radius: 0; border-bottom-right-radius: 0; border-color: #ced4da; box-shadow: none; }
        .search-input-group .form-control:focus { border-color: var(--form-control-focus-border); box-shadow: 0 0 0 0.25rem var(--form-control-focus-shadow); }
        .search-input-group .btn { border-radius: 0; font-weight: 600; padding-left: 1rem; padding-right: 1rem; }
        .search-input-group #btnBuscarCita { background-color: var(--btn-custom-bg); color: white; border-color: var(--btn-custom-bg); border-top-right-radius: 0; border-bottom-right-radius: 0; }
        .search-input-group #btnBuscarCita:hover { background-color: var(--btn-custom-hover-bg); border-color: var(--btn-custom-hover-bg); }
        .search-input-group #btnLimpiarBusquedaCita { background-color: var(--btn-clear-bg); color: white; border-color: var(--btn-clear-bg); border-top-right-radius: 8px; border-bottom-right-radius: 8px; }
        .search-input-group #btnLimpiarBusquedaCita:hover { background-color: var(--btn-clear-hover-bg); border-color: var(--btn-clear-hover-bg); }
        .search-input-group .btn:not(:last-child) { border-right: 1px solid rgba(0,0,0,.125); }

        /* Modal width adjustments */
        .modal-dialog { max-width: 600px; width: 90%; }
        @media (min-width: 576px) { .modal-dialog { max-width: 650px; } }
        @media (min-width: 768px) { .modal-dialog { max-width: 700px; } }

        /* Responsive styles */
        @media (max-width: 767.98px) {
            .sidebar { left: -220px; box-shadow: none; z-index: 1040; }
            .sidebar.show { left: 0; box-shadow: 2px 0 5px var(--card-shadow); }
            body { padding-left: 0; padding-top: 5rem; }
            .content { padding: 1rem; }
            #sidebarToggle {
                display: flex !important; position: fixed; top: 15px; left: 15px; z-index: 1050;
                border-radius: 50%; width: 45px; height: 45px; align-items: center; justify-content: center;
                background-color: var(--sidebar-bg); border-color: var(--sidebar-hover-bg); color: var(--sidebar-text);
                transition: background-color 0.3s, color 0.3s, border-color 0.3s;
            }
            #sidebarToggle:hover { background-color: var(--sidebar-hover-bg); color: var(--sidebar-hover-text); }
            .sidebar-backdrop {
                position: fixed; top: 0; left: 0; width: 100%; height: 100%;
                background-color: rgba(0, 0, 0, 0.5); z-index: 1039; display: none; transition: opacity 0.3s ease; opacity: 0;
            }
            .sidebar-backdrop.show { display: block; opacity: 1; }
            body.overflow-hidden { overflow: hidden; }
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
                    <a class="nav-link active" href="Citas.aspx"> <%-- Marked as active --%>
                        <i class="bi bi-calendar-check-fill"></i><span>Citas</span>
                    </a>
                </li>
                <li class="nav-item">
                    <a class="nav-link" href="CitaServicios.aspx">
                        <i class="bi bi-clipboard-check-fill"></i><span>Cita-Servicios</span>
                    </a>
                </li>

            </ul>
        </nav>

        <!-- Área de Contenido Principal -->
        <div class="content">

            <!-- Barra de Búsqueda -->
            <div class="input-group mb-3 search-input-group">
                <asp:TextBox ID="txtBuscarCita" runat="server" CssClass="form-control" Placeholder="Buscar por cliente, mascota o empleado" />
                <asp:LinkButton ID="btnBuscarCita" runat="server" OnClick="btnBuscarCita_Click"
                    CssClass="btn btn-outline-secondary" 
                    ToolTip="Buscar">
                    <i class="bi bi-search fs-6"></i> 
                    <span class="sr-only"></span> 
                </asp:LinkButton>

                <asp:LinkButton ID="btnLimpiarBusquedaCita" runat="server" OnClick="btnLimpiarBusquedaCita_Click"
                    CssClass="btn btn-outline-secondary"
                    ToolTip="Limpiar">
                    <i class="bi bi-x-lg fs-6"></i> 
                    <span class="sr-only"></span> 
                </asp:LinkButton>

            </div>

            
            <div class="d-flex justify-content-between align-items-center mb-4">
                <button type="button" class="btn btn-custom me-2" data-bs-toggle="modal" data-bs-target="#citaModal" data-mode="add">
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-plus-circle me-2" viewBox="0 0 16 16">
                        <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14m0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16"/>
                        <path d="M8 4a.5.5 0 0 1 .5.5v3h3a.5.5 0 0 1 0 1h-3v3a.5.5 0 0 1-1 0v-3h-3a.5.5 0 0 1 0-1h3v-3A.5.5 0 0 1 8 4"/>
                    </svg>
                    Agendar Nueva Cita
                </button>
                <div>
                <asp:LinkButton ID="btnImprimirPdf" runat="server" CssClass="btn btn-custom" OnClick="btnImprimirPdf_Click">
                    <i class="bi bi-file-earmark-pdf me-2"></i> Reporte PDF
                </asp:LinkButton>
                <asp:LinkButton ID="btnExportarExcel" runat="server" CssClass="btn btn-light" OnClick="btnExportarExcel_Click" ToolTip="Exportar a Excel"
                    Style="padding: 0.5rem 1rem; width: auto; height: auto; display: inline-flex; align-items: center; justify-content: center; border: 1px solid #dee2e6; min-width: 45px;">
                    <img src="<%= ResolveUrl("~/Assets/Images/excel.png") %>" alt="Excel" style="width: 28px; height: 28px; margin: 0; padding: 0;" />
                </asp:LinkButton>
                </div>
            </div>

            <hr />

            <!-- GridView para mostrar las citas existentes -->
            <asp:GridView ID="gvCitas" runat="server" AutoGenerateColumns="False" OnRowCommand="gvCitas_RowCommand"
                CssClass="table table-bordered table-striped table-hover"
                DataKeyNames="CitaID, MascotaID, EmpleadoID, ClienteID"> <%-- Claves de datos para una edición robusta --%>
                <Columns>
                    <%-- Las columnas se muestran en el orden que se definen aquí --%>
                    <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:yyyy-MM-dd}" />
                    <asp:BoundField DataField="NombreCliente" HeaderText="Cliente" />
                    <asp:BoundField DataField="NombreMascota" HeaderText="Mascota" />
                    <asp:BoundField DataField="NombreEmpleado" HeaderText="Empleado" />
                    <asp:TemplateField HeaderText="Acciones" ItemStyle-Width="120px">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnEditarCita" runat="server" CommandName="Editar"
                                CssClass="icon-action icon-edit-custom ms-4 me-2"
                                CommandArgument="<%# Container.DataItemIndex %>"
                                ToolTip="Editar Cita">
                                <i class="bi bi-pencil fs-5"></i>
                                <span class="sr-only"></span>
                            </asp:LinkButton>
                
                            <asp:LinkButton ID="btnEliminarCita" runat="server" CommandName="Eliminar"
                                CssClass="icon-action text-danger me-2"
                                CommandArgument="<%# Container.DataItemIndex %>"
                                OnClientClick="return confirm('¿Está seguro de que desea eliminar esta cita?');"
                                ToolTip="Eliminar Servicio">
                                <i class="bi bi-trash fs-5"></i>
                                <span class="sr-only"></span>
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <!-- Modal de Bootstrap para Añadir/Editar Cita -->
        <div class="modal fade" id="citaModal" tabindex="-1" aria-labelledby="citaModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="citaModalLabel">Gestión de Cita</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <!-- Etiqueta para mensajes de validación/éxito/error -->
                        <asp:Label ID="lblMensaje" runat="server"></asp:Label><br /> <%-- Removed EnableViewState="false" --%>
                        
                        <!-- Campos de entrada del formulario -->
                        <div class="mb-3">
                            <label for="txtFecha" class="form-label">Fecha de la Cita</label>
                            <asp:TextBox ID="txtFecha" runat="server" CssClass="form-control" Placeholder="Fecha (YYYY-MM-DD)" />
                        </div>
                        
                        <div class="mb-3">
                            <label for="ddlClientes" class="form-label">Cliente</label>
                            <asp:DropDownList ID="ddlClientes" runat="server" CssClass="form-select" AutoPostBack="True" OnSelectedIndexChanged="ddlClientes_SelectedIndexChanged">
                                <%-- Los clientes se cargarán desde el code-behind --%>
                            </asp:DropDownList>
                        </div>

                        <div class="mb-3">
                            <label for="ddlMascotas" class="form-label">Mascota</label>
                            <asp:DropDownList ID="ddlMascotas" runat="server" CssClass="form-select">
                                <%-- Las mascotas se cargarán dinámicamente según el cliente --%>
                            </asp:DropDownList>
                        </div>

                        <div class="mb-3">
                            <label for="ddlEmpleados" class="form-label">Empleado</label>
                            <asp:DropDownList ID="ddlEmpleados" runat="server" CssClass="form-select">
                                <%-- Los empleados se cargarán desde el code-behind --%>
                            </asp:DropDownList>
                        </div>
                        
                        <!-- Campo oculto para almacenar el ID de la cita al editar -->
                        <asp:HiddenField ID="hfCitaID" runat="server" />
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>
                        <!-- Botones de Guardar (Agregar) y Actualizar -->
                        <asp:Button ID="btnAgregar" runat="server" CssClass="btn btn-success" Text="Guardar" OnClick="btnAgregar_Click" />
                        <asp:Button ID="btnActualizar" runat="server" CssClass="btn btn-primary" Text="Actualizar" OnClick="btnActualizar_Click" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Archivos JavaScript de Bootstrap y Flatpickr -->
        <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
        <script src="https://cdn.jsdelivr.net/npm/flatpickr"></script>
        <script src="https://cdn.jsdelivr.net/npm/flatpickr/dist/l10n/es.js"></script> <!-- Localización en español -->
        <script>
            // Inicializar Flatpickr en el campo de fecha
            flatpickr("#<%= txtFecha.ClientID %>", {
                enableTime: false,
                dateFormat: "Y-m-d", // Formato de fecha para la base de datos
                altInput: true, // Muestra un campo de entrada alternativo formateado para el usuario
                altFormat: "d F Y", // Formato amigable para el usuario (ej. "05 Julio 2025")
                locale: "es", // Establecer el idioma a español
                minDate: "today", // ¡ESTA ES LA CLAVE! No permite seleccionar fechas anteriores a hoy.
                // Habilitar el autopostback para el ddlClientes al cambiar la fecha
                onChange: function (selectedDates, dateStr, instance) {
                    // Puedes agregar lógica aquí si es necesario al cambiar la fecha.
                    // Para ddlClientes_SelectedIndexChanged, el AutoPostBack ya lo maneja.
                }
            }); 

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
            function clearModalFormAndSetAddModeCita() {
                var txtFecha = document.getElementById('<%= txtFecha.ClientID %>');
                var ddlClientes = document.getElementById('<%= ddlClientes.ClientID %>');
                var ddlMascotas = document.getElementById('<%= ddlMascotas.ClientID %>');
                var ddlEmpleados = document.getElementById('<%= ddlEmpleados.ClientID %>');
                var hfCitaID = document.getElementById('<%= hfCitaID.ClientID %>');
                var lblMensaje = document.getElementById('<%= lblMensaje.ClientID %>');
                var btnAgregar = document.getElementById('<%= btnAgregar.ClientID %>');
                var btnActualizar = document.getElementById('<%= btnActualizar.ClientID %>');
                var modalTitle = document.getElementById('citaModalLabel');

                if (txtFecha) {
                    txtFecha._flatpickr.clear(); // Limpiar el valor de Flatpickr
                    txtFecha.value = ''; // Asegurarse de que el input base también esté vacío
                }
                if (ddlClientes) ddlClientes.selectedIndex = 0;
                if (ddlMascotas) {
                    ddlMascotas.innerHTML = '<option value="">Seleccione una Mascota</option>'; // Restablecer a solo la opción por defecto
                }
                if (ddlEmpleados) ddlEmpleados.selectedIndex = 0;
                if (hfCitaID) hfCitaID.value = '';

                // Limpiar el mensaje y sus clases para que no ocupe espacio
                if (lblMensaje) {
                    lblMensaje.innerHTML = '';
                    lblMensaje.className = '';
                }

                if (btnAgregar) btnAgregar.style.display = 'inline-block';
                if (btnActualizar) btnActualizar.style.display = 'none';

                if (modalTitle) modalTitle.innerText = 'Agendar Nueva Cita';
            }

            // Función JavaScript para mostrar el modal de Cita (llamada desde C#)
            function showCitaModal() {
                var myModal = new bootstrap.Modal(document.getElementById('citaModal'));
                myModal.show();
            }

            // Función JavaScript para ocultar el modal de Cita (llamada desde C#)
            function hideCitaModal() {
                var myModal = bootstrap.Modal.getInstance(document.getElementById('citaModal'));
                if (myModal) {
                    myModal.hide();
                }
            }

            // Listener de evento para cuando el modal de Cita se muestra (para manejar el reseteo en modo 'agregar')
            document.addEventListener('DOMContentLoaded', function () {
                var citaModal = document.getElementById('citaModal');
                if (citaModal) {
                    citaModal.addEventListener('shown.bs.modal', function (event) {
                        var button = event.relatedTarget;
                        // Solo limpiar si el modal fue activado por el botón 'Add New' (que tiene data-mode="add")
                        if (button && button.getAttribute('data-mode') === 'add') {
                            clearModalFormAndSetAddModeCita();
                        }
                    });
                    // Listener de evento para cuando el modal se oculta completamente
                    citaModal.addEventListener('hidden.bs.modal', function () {
                        // Resetear la visibilidad de los botones para la próxima vez que se abra el modal en modo 'agregar'
                        clearModalFormAndSetAddModeCita();
                    });
                }
            });
        </script>
    </form>
</body>
</html>
