<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="VetWeb.Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Dashboard - VetWeb</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />

    <!-- Favicon -->
    <link href="~/favicon.ico" rel="shortcut icon" type="image/x-icon" />

    <!-- Bootstrap CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />

    <!-- Bootstrap Icons CSS -->
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
        
        /* Colores de citas (ajustados ligeramente para que coincidan con esta paleta si es necesario) */
        --cita-card-bg-gradient-start: #E6F0F5; /* Azul grisáceo muy claro */
        --cita-card-bg-gradient-end: #F0F5FA;   /* Azul grisáceo muy claro un poco diferente */
        --cita-card-border: #BCDDEB; /* Borde azul grisáceo claro */
        --cita-card-shadow: rgba(0, 0, 0, 0.08); /* Sombra sutil para tarjeta de cita */
        --cita-client-color: #333333; /* Gris oscuro para nombre del cliente */
        --cita-icon-client: #5A7F9D; /* Gris azulado más claro para icono de cliente */
        --cita-date-bubble-bg: #5A7F9D; /* Gris azulado más claro para burbuja de fecha */
        --cita-text-color: #495057; /* Gris medio para texto de cita */
        --cita-icon-mascota: #60B080; /* Verde/azul verdoso suave (ajustado de #28a745) */
        --cita-footer-border: #CEDAE0; /* Borde gris azulado punteado para pie de cita */
        --cita-footer-text: #6c757d; /* Gris medio para texto de pie de cita */
        --cita-icon-empleado: #8C6A9E; /* Púrpura suave (ajustado de #6f42c1) */
        --no-citas-bg: #EAECEE; /* Gris muy claro para mensaje de no citas */
        --no-citas-text: #6c757d; /* Gris medio para mensaje de no citas */

        /* Colores para alertas y botones de formulario específicos de Clientes.aspx */
        --btn-custom-bg: #5A7F9D; /* Gris azulado más claro para "Agregar Nuevo Cliente" y "Buscar" */
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
        font-family: 'Inter', sans-serif; /* Asumiendo que 'Inter' está disponible o se importa */
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

    /* Styles for chart containers */
    .chart-container {
        position: relative;
        height: 40vh;
        width: 100%;
    }

    /* Styles for new appointment cards */
    .cita-card {
        background: linear-gradient(135deg, var(--cita-card-bg-gradient-start), var(--cita-card-bg-gradient-end));
        border: 1px solid var(--cita-card-border);
        border-radius: 12px;
        box-shadow: 0 6px 15px var(--cita-card-shadow);
        padding: 25px;
        margin-bottom: 25px;
        display: flex;
        flex-direction: column;
        justify-content: space-between;
        height: 100%;
        transition: all 0.3s ease;
        position: relative;
        overflow: hidden;
    }

    .cita-card:hover {
        transform: translateY(-8px) scale(1.02);
        box-shadow: 0 10px 25px var(--cita-card-shadow);
    }

    .cita-card-header {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        margin-bottom: 15px;
        font-size: 1.1rem;
        font-weight: 600;
        color: var(--cita-client-color);
        position: relative;
        z-index: 1;
    }
    .cita-card-header .client-info {
        display: flex;
        align-items: center;
        font-size: 1.15rem;
        font-weight: 700;
        color: var(--cita-client-color);
    }
    .cita-card-header .client-info svg {
        margin-right: 10px;
        color: var(--cita-icon-client);
        font-size: 1.5rem;
    }
    .cita-card-header .date-bubble {
        background-color: var(--cita-date-bubble-bg);
        color: white;
        padding: 8px 15px;
        border-radius: 8px;
        font-weight: 700;
        font-size: 1.1rem;
        box-shadow: 0 2px 5px rgba(0,0,0,0.2);
        text-align: center;
        min-width: 120px;
    }
    
    .cita-card-body {
        margin-top: 5px;
        padding-bottom: 5px;
    }
    .cita-card-body p {
        margin-bottom: 5px;
        font-size: 1.05rem;
        color: var(--cita-text-color);
        display: flex;
        align-items: center;
    }
    .cita-card-body p svg {
        margin-right: 10px;
        color: var(--cita-icon-mascota);
        font-size: 1.3rem;
    }
    .cita-card-body .label {
        font-weight: 600;
        color: var(--text-color);
        margin-right: 5px;
    }
    .cita-card-footer {
        margin-top: 10px;
        padding-top: 10px;
        border-top: 1px dashed var(--cita-footer-border);
        font-size: 0.98rem;
        color: var(--cita-footer-text);
        display: flex;
        align-items: center;
    }
    .cita-card-footer svg {
        margin-right: 10px;
        color: var(--cita-icon-empleado);
        font-size: 1.3rem;
    }
    .cita-card-footer .label {
        font-weight: 600;
        color: var(--text-color);
        margin-right: 5px;
    }

    /* Style for when there are no appointments */
    .no-citas-message {
        text-align: center;
        padding: 30px;
        color: var(--no-citas-text);
        font-size: 1.1rem;
        background-color: var(--no-citas-bg);
        border-radius: 10px;
        box-shadow: inset 0 0 10px rgba(0,0,0,0.05);
        margin-top: 20px;
        font-weight: 500;
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

    /* Specific styles for Clientes.aspx content */
    .btn-custom {
        background-color: var(--btn-custom-bg);
        color: white;
        border-radius: 8px;
        padding: 10px 20px;
        font-size: 1.1rem;
        transition: background-color 0.3s ease;
        border: none; /* Add border: none; to ensure consistent styling */
    }
    .btn-custom:hover {
        background-color: var(--btn-custom-hover-bg);
        color: white;
    }
    /* Table enhancements */
    .table-striped tbody tr:nth-of-type(odd) {
        background-color: #fff; /* Force white background for odd rows (if table-striped is still used) */
    }
    .table-hover tbody tr:hover {
        background-color: rgba(0, 0, 0, 0.04); /* Un hover más suave */
    }
    .table-primary th {
        background-color: var(--card-header-bg); /* Usa el mismo color del encabezado de tarjeta */
        color: white;
        border-color: var(--card-header-bg);
    }

    .modal-header {
        background-color: var(--card-header-bg); /* Usa el mismo color del encabezado de tarjeta */
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
        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); /* Subtle shadow for depth */
        word-wrap: break-word; /* Ensures long words break to fit */
        white-space: normal; /* Allows text to wrap naturally */
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
        border-color: var(--form-control-focus-border);
        box-shadow: 0 0 0 0.25rem var(--form-control-focus-shadow);
    }

    .search-input-group .btn {
        /* Common styles for search buttons */
        border-radius: 0; /* Remove default radius inherited from .btn */
        font-weight: 600; /* Slightly bolder text */
        padding-left: 1rem;
        padding-right: 1rem;
    }

    /* Specific styles for "Buscar" button */
    .search-input-group #btnBuscarCliente {
        background-color: var(--btn-custom-bg);
        color: white;
        border-color: var(--btn-custom-bg);
        border-top-right-radius: 0;
        border-bottom-right-radius: 0;
    }
    .search-input-group #btnBuscarCliente:hover {
        background-color: var(--btn-custom-hover-bg);
        border-color: var(--btn-custom-hover-bg);
    }

    /* Specific styles for "Limpiar" button */
    .search-input-group #btnLimpiarBusquedaCliente {
        background-color: var(--btn-clear-bg);
        color: white;
        border-color: var(--btn-clear-bg);
        border-top-right-radius: 8px; /* Rounded corner on the far right */
        border-bottom-right-radius: 8px;
    }
    .search-input-group #btnLimpiarBusquedaCliente:hover {
        background-color: var(--btn-clear-hover-bg);
        border-color: var(--btn-clear-hover-bg);
    }

    /* Ensure correct border-radius for input-group buttons */
    .search-input-group .btn:not(:last-child) {
        border-right: 1px solid rgba(0,0,0,.125); /* Small separator between buttons */
    }

    /* Custom styles for alert messages within the modal */
    #clienteModal .alert {
        font-size: 1rem; /* Slightly larger text for readability */
        padding: 0.75rem 1.25rem; /* More comfortable padding */
        margin-top: 1rem; /* Space above the message */
        margin-bottom: 1.5rem; /* Space below the message */
        text-align: center; /* Center the text */
        font-weight: 600; /* Bolder text for emphasis */
    }

    #clienteModal .alert-danger::before {
        content: "\2716"; /* Unicode 'heavy multiplication x' for a cross */
        font-size: 1.2rem;
        margin-right: 0.5rem;
        vertical-align: middle;
        display: inline-block;
        line-height: 1; /* Align with text */
        color: var(--alert-danger-icon-color);
    }

    #clienteModal .alert-success::before {
        content: "\2714"; /* Unicode 'heavy check mark' */
        font-size: 1.2rem;
        margin-right: 0.5rem;
        vertical-align: middle;
        display: inline-block;
        line-height: 1;
        color: var(--alert-success-icon-color);
    }

    /* Increase modal width for better display of long messages */
    #clienteModal .modal-dialog {
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
</style>
</head>
<body>
    <form id="form1" runat="server">
        <!-- Required for C# to execute JavaScript scripts -->
        <asp:ScriptManager runat="server"></asp:ScriptManager>

        <!-- Button to toggle sidebar on small screens (mobile) -->
        <button type="button" id="sidebarToggle" class="btn btn-primary d-md-none">
            <i class="bi bi-list fs-5"></i>
        </button>

        <!-- Side Navigation Bar -->
        <nav class="sidebar" id="sidebarMenu">
           <a href="Default.aspx" class="sidebar-brand">
               <i class="bi bi-hospital-fill"></i> VetWeb
           </a>
            <ul class="nav flex-column">
                <li class="nav-item">
                    <a class="nav-link active" href="Default.aspx">
                        <i class="bi bi-speedometer2"></i><span>Dashboard</span>
                    </a>
                </li>
                <li class="nav-item">
                    <a class="nav-link" href="Clientes.aspx">
                        <i class="bi bi-people-fill"></i><span>Clientes</span>
                    </a>
                </li>
                
                <!-- Collapsible Menu: Mascotas -->
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

                <!-- Collapsible Menu: Empleados -->
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

                <!-- Collapsible Menu: Servicios -->
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
            </ul>
        </nav>

        <!-- Backdrop for mobile sidebar overlay -->
        <div id="sidebarBackdrop" class="sidebar-backdrop"></div> 

        <!-- Main Content Area -->
        <div class="content" id="mainContent">

            <!-- Summary Cards -->
            <div class="row g-4 mb-5">
                <!-- Total Clients Card -->
                <div class="col-md-6 col-lg-3">
                    <div class="card text-center">
                        <div class="card-header">
                            Clientes Registrados
                        </div>
                        <div class="card-body">
                            <h3 class="card-title">
                                <asp:Label ID="lblTotalClientes" runat="server" Text="0"></asp:Label>
                            </h3>
                            <p class="card-text text-muted">Numero de clientes.</p>
                        </div>
                    </div>
                </div>

                <!-- Total Pets Card -->
                <div class="col-md-6 col-lg-3">
                    <div class="card text-center">
                        <div class="card-header">
                            Mascotas Registradas
                        </div>
                        <div class="card-body">
                            <h3 class="card-title">
                                <asp:Label ID="lblTotalMascotas" runat="server" Text="0"></asp:Label>
                            </h3>
                            <p class="card-text text-muted">Numero de mascotas.</p>
                        </div>
                    </div>
                </div>

                <!-- Total Employees Card -->
                <div class="col-md-6 col-lg-3">
                    <div class="card text-center">
                        <div class="card-header">
                            Empleados Activos
                        </div>
                        <div class="card-body">
                            <h3 class="card-title">
                                <asp:Label ID="lblTotalEmpleados" runat="server" Text="0"></asp:Label>
                            </h3>
                            <p class="card-text text-muted">Personal existente.</p>
                        </div>
                    </div>
                </div>

                <!-- Pending Appointments Today Card -->
                <div class="col-md-6 col-lg-3">
                    <div class="card text-center">
                        <div class="card-header">
                            Citas Pendientes (Hoy)
                        </div>
                        <div class="card-body">
                            <h3 class="card-title">
                                <asp:Label ID="lblCitasPendientesHoy" runat="server" Text="0"></asp:Label>
                            </h3>
                            <p class="card-text text-muted">Citas programadas para hoy.</p>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Charts Section -->
            <div class="row g-4 mb-5">
                <!-- Line Chart: Appointments per Month -->
                <div class="col-lg-6">
                    <div class="card">
                        <div class="card-header">
                            Citas Programadas (Últimos 6 Meses)
                        </div>
                        <div class="card-body">
                            <div class="chart-container">
                                <canvas id="citasMesChart"></canvas>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Doughnut Chart: Pets by Species -->
                <div class="col-lg-6">
                    <div class="card">
                        <div class="card-header">
                            Mascotas por Especie
                        </div>
                        <div class="card-body">
                            <div class="chart-container">
                                <canvas id="mascotasEspecieChart"></canvas>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="row g-4 mb-5">
                 <!-- Bar Chart: Top 5 Most Used Services -->
                <div class="col-lg-6">
                    <div class="card">
                        <div class="card-header">
                            Top 5 Servicios Más Utilizados
                        </div>
                        <div class="card-body">
                            <div class="chart-container">
                                <canvas id="topServiciosChart"></canvas>
                            </div>
                        </div>
                    </div>
                </div>
                <!-- Bar Chart: Top 5 Employees with Most Appointments -->
                <div class="col-lg-6">
                    <div class="card">
                        <div class="card-header">
                            Top 5 Empleados con Más Citas
                        </div>
                        <div class="card-body">
                            <div class="chart-container">
                                <canvas id="topEmpleadosChart"></canvas>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Upcoming Appointments Section -->
            <div class="row mb-5">
                <div class="col-12">
                    <div class="card">
                        <div class="card-header">
                            Próximas Citas
                        </div>
                        <div class="card-body">
                            <asp:Repeater ID="rptProximasCitas" runat="server">
                                <HeaderTemplate>
                                    <div class="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-4"> <%-- Columnas responsivas para tarjetas --%>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <div class="col">
                                        <div class="cita-card">
                                            <div class="cita-card-header">
                                                <span class="client-info">
                                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-person-fill" viewBox="0 0 16 16">
                                                        <path d="M3 14s-1 0-1-1 1-4 6-4 6 3 6 4-1 1-1 1zm5-6a3 3 0 1 0 0-6 3 3 0 0 0 0 6"/>
                                                    </svg>
                                                    <span class="label">Cliente:</span> <%# Eval("NombreCliente") %>
                                                </span>
                                                <span class="date-bubble"><%# Eval("Fecha", "{0:dd MMM.yyyy}") %></span> <%-- Formato de fecha mejorado --%>
                                            </div>
                                            <div class="cita-card-body">
                                                <p>
                                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-paw-fill" viewBox="0 0 16 16">
                                                        <path d="M2.544 5.034c.005.011.021.045.048.096L.516 8.52A.7.7 0 0 0 .54 9.573c.123.473.539.815 1.054.815.228 0 .437-.083.593-.236q.107-.107.207-.21l2.427-2.426C4.858 7.327 5.119 8 5.75 8s.892.673 1.25.75L7.22 8.94c.091.229.176.452.26.666.303.77.892 1.597 2.054 1.597 1.066 0 1.96-1.127 2.274-2.22l.5-.833.011-.018c.003-.006.009-.022.036-.068.041-.073.094-.16.155-.26.216-.367.43-.75.643-1.077C14.735 6.012 15 5.561 15 4.887c0-.585-.34-1.228-.972-1.288-.363-.035-.747.098-1.092.355L12.43 4.29l-.265 2.148c-.021.173-.1.353-.223.532-.14.205-.306.402-.505.58-.23.2-.497.35-.8.43-.226.06-.463.09-.705.09-.272 0-.528-.05-.75-.15-.22-.1-.407-.23-.55-.38-.073-.075-.143-.153-.2-.23-.056-.077-.104-.155-.14-.223-.016-.03-.028-.05-.034-.055-.02-.016-.027-.018-.031-.019C8.046 5.483 8.356 3.65 6.64 2.893 4.544 1.956 2.015 2.17 1.545 3.023c-.346.634-.143 1.34.417 1.97zM5.5 5.5c-.828 0-1.5-.672-1.5-1.5S4.672 2.5 5.5 2.5s1.5.672 1.5 1.5S6.328 5.5 5.5 5.5z"/>
                                                    </svg>
                                                    <span class="label">Mascota:</span> <%# Eval("NombreMascota") %>
                                                </p>
                                            </div>
                                            <div class="cita-card-footer">
                                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-person-badge-fill" viewBox="0 0 16 16">
                                                    <path d="M2 2a2 2 0 0 0-2 2v12l4-1.5 4 1.5 4-1.5 4 1.5V4a2 2 0 0 0-2-2zM4.5 0A2.5 2.5 0 0 0 2 2.5C2 3.75 3 5 4.5 5s2.5-1.25 2.5-2.5A2.5 2.5 0 0 0 4.5 0"/>
                                                </svg>
                                                <span class="label">Atendido por:</span> <%# Eval("NombreEmpleado") %>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </div> <%-- Close row for columns --%>
                                </FooterTemplate>
                            </asp:Repeater>
                            <asp:Label ID="lblNoCitas" runat="server" Text="No hay citas próximas programadas." Visible="false" CssClass="no-citas-message"></asp:Label>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Bootstrap JS Bundle -->
        <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
        <!-- Chart.js CDN -->
        <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
        <script>
            // Global variables to store chart instances
            let myCitasChart;
            let myMascotasChart;
            let myTopServiciosChart;
            let myTopEmpleadosChart;

            // Global storage for chart data (populated by C# ScriptManager calls)
            window.chartData = {
                citas: { labels: [], data: [] },
                mascotas: { labels: [], data: [] },
                servicios: { labels: [], data: [] },
                empleados: { labels: [], data: [] }
            };

            // Function to draw all charts (called by C# after data is ready)
            function drawAllCharts() {
                drawCitasMesChart(window.chartData.citas.labels, window.chartData.citas.data);
                drawMascotasEspecieChart(window.chartData.mascotas.labels, window.chartData.mascotas.data);
                drawTopServiciosChart(window.chartData.servicios.labels, window.chartData.servicios.data);
                drawTopEmpleadosChart(window.chartData.empleados.labels, window.chartData.empleados.data);
            }

            // Event listener for DOM content loaded
            document.addEventListener('DOMContentLoaded', function () {
                const sidebarToggle = document.getElementById('sidebarToggle');
                const sidebar = document.getElementById('sidebarMenu');
                const sidebarBackdrop = document.getElementById('sidebarBackdrop');

                // Sidebar toggle logic for mobile screens
                if (sidebarToggle && sidebar && sidebarBackdrop) {
                    sidebarToggle.addEventListener('click', function () {
                        sidebar.classList.toggle('show');
                        sidebarBackdrop.classList.toggle('show');
                        document.body.classList.toggle('overflow-hidden');
                    });

                    sidebarBackdrop.addEventListener('click', function () {
                        sidebar.classList.remove('show');
                        sidebarBackdrop.classList.remove('show');
                        document.body.classList.remove('overflow-hidden');
                    });

                    // Close sidebar on link click (important for mobile UX)
                    sidebar.querySelectorAll('.nav-link').forEach(link => {
                        link.addEventListener('click', function () {
                            if (window.innerWidth < 768) {
                                // If it's a collapse toggle link, don't close the sidebar immediately.
                                if (!this.hasAttribute('data-bs-toggle') || this.getAttribute('data-bs-toggle') !== 'collapse') {
                                    sidebar.classList.remove('show');
                                    sidebarBackdrop.classList.remove('show');
                                    document.body.classList.remove('overflow-hidden');
                                }
                            }
                        });
                    });
                }
            });

            // --- Chart Drawing Functions (unchanged, using light mode colors) ---

            function drawCitasMesChart(labels, data) {
                window.chartData.citas.labels = labels;
                window.chartData.citas.data = data;
                if (!labels || labels.length === 0 || !data || data.length === 0) {
                    console.warn("CitasMesChart: No data provided or data is empty. Chart will not be drawn.");
                    if (myCitasChart) myCitasChart.destroy();
                    return;
                }
                const ctx = document.getElementById('citasMesChart').getContext('2d');
                const borderColor = 'rgba(13, 110, 253, 1)';
                const backgroundColor = 'rgba(13, 110, 253, 0.2)';
                const textColor = '#212529';
                const gridColor = 'rgba(0,0,0,0.1)';
                const tickColor = '#6c757d';
                if (myCitasChart) myCitasChart.destroy();
                myCitasChart = new Chart(ctx, {
                    type: 'line',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Citas Programadas',
                            data: data,
                            borderColor: borderColor,
                            backgroundColor: backgroundColor,
                            tension: 0.3,
                            fill: true,
                            pointBackgroundColor: borderColor,
                            pointBorderColor: '#fff',
                            pointHoverBackgroundColor: '#fff',
                            pointHoverBorderColor: borderColor
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        scales: {
                            y: {
                                beginAtZero: true,
                                title: { display: true, text: 'Número de Citas', color: textColor },
                                ticks: { color: tickColor },
                                grid: { color: gridColor }
                            },
                            x: {
                                title: { display: true, text: 'Mes', color: textColor },
                                ticks: { color: tickColor },
                                grid: { color: gridColor }
                            }
                        },
                        plugins: {
                            legend: { display: true, position: 'top', labels: { color: textColor } },
                            title: { display: true, text: 'Tendencia de Citas por Mes', color: textColor }
                        }
                    }
                });
            }

            function drawMascotasEspecieChart(labels, data) {
                window.chartData.mascotas.labels = labels;
                window.chartData.mascotas.data = data;
                if (!labels || labels.length === 0 || !data || data.length === 0) {
                    console.warn("MascotasEspecieChart: No data provided or data is empty. Chart will not be drawn.");
                    if (myMascotasChart) myMascotasChart.destroy();
                    return;
                }
                const ctx = document.getElementById('mascotasEspecieChart').getContext('2d');
                const textColor = '#212529';
                const borderColor = '#fff';
                if (myMascotasChart) myMascotasChart.destroy();
                myMascotasChart = new Chart(ctx, {
                    type: 'doughnut',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Cantidad de Mascotas',
                            data: data,
                            backgroundColor: [
                                'rgba(255, 159, 64, 0.8)', 'rgba(75, 192, 192, 0.8)', 'rgba(255, 99, 132, 0.8)', 'rgba(54, 162, 235, 0.8)', 'rgba(153, 102, 255, 0.8)', 'rgba(201, 203, 207, 0.8)'
                            ],
                            borderColor: borderColor,
                            borderWidth: 2
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: { position: 'right', align: 'start', labels: { color: textColor } },
                            title: { display: true, text: 'Distribución de Mascotas por Especie', color: textColor }
                        }
                    }
                });
            }

            function drawTopServiciosChart(labels, data) {
                window.chartData.servicios.labels = labels;
                window.chartData.servicios.data = data;
                if (!labels || labels.length === 0 || !data || data.length === 0) {
                    console.warn("TopServiciosChart: No data provided or data is empty. Chart will not be drawn.");
                    if (myTopServiciosChart) myTopServiciosChart.destroy();
                    return;
                }
                const ctx = document.getElementById('topServiciosChart').getContext('2d');
                const textColor = '#212529';
                const gridColor = 'rgba(0,0,0,0.1)';
                const tickColor = '#6c757d';
                if (myTopServiciosChart) myTopServiciosChart.destroy();
                myTopServiciosChart = new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Veces Utilizado',
                            data: data,
                            backgroundColor: [
                                'rgba(0, 123, 255, 0.7)', 'rgba(40, 167, 69, 0.7)', 'rgba(255, 193, 7, 0.7)', 'rgba(220, 53, 69, 0.7)', 'rgba(108, 117, 125, 0.7)'
                            ],
                            borderColor: [
                                'rgba(0, 123, 255, 1)', 'rgba(40, 167, 69, 1)', 'rgba(255, 193, 7, 1)', 'rgba(220, 53, 69, 1)', 'rgba(108, 117, 125, 1)'
                            ],
                            borderWidth: 1
                        }]
                    },
                    options: {
                        indexAxis: 'y',
                        responsive: true,
                        maintainAspectRatio: false,
                        scales: {
                            x: {
                                beginAtZero: true,
                                title: { display: true, text: 'Número de Utilizaciones', color: textColor },
                                ticks: { color: tickColor },
                                grid: { color: gridColor }
                            },
                            y: {
                                title: { display: true, text: 'Servicio', color: textColor },
                                ticks: { color: tickColor },
                                grid: { color: gridColor }
                            }
                        },
                        plugins: {
                            legend: { display: false },
                            title: { display: true, text: 'Top 5 Servicios Más Utilizados', color: textColor }
                        }
                    }
                });
            }

            function drawTopEmpleadosChart(labels, data) {
                window.chartData.empleados.labels = labels;
                window.chartData.empleados.data = data;
                if (!labels || labels.length === 0 || !data || data.length === 0) {
                    console.warn("TopEmpleadosChart: No data provided or data is empty. Chart will not be drawn.");
                    if (myTopEmpleadosChart) myTopEmpleadosChart.destroy();
                    return;
                }
                const ctx = document.getElementById('topEmpleadosChart').getContext('2d');
                const textColor = '#212529';
                const gridColor = 'rgba(0,0,0,0.1)';
                const tickColor = '#6c757d';
                if (myTopEmpleadosChart) myTopEmpleadosChart.destroy();
                myTopEmpleadosChart = new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Número de Citas',
                            data: data,
                            backgroundColor: [
                                'rgba(111, 66, 193, 0.7)', 'rgba(23, 162, 184, 0.7)', 'rgba(253, 126, 20, 0.7)', 'rgba(102, 16, 242, 0.7)', 'rgba(32, 201, 151, 0.7)'
                            ],
                            borderColor: [
                                'rgba(111, 66, 193, 1)', 'rgba(23, 162, 184, 1)', 'rgba(253, 126, 20, 1)', 'rgba(102, 16, 242, 1)', 'rgba(32, 201, 151, 1)'
                            ],
                            borderWidth: 1
                        }]
                    },
                    options: {
                        indexAxis: 'y',
                        responsive: true,
                        maintainAspectRatio: false,
                        scales: {
                            x: {
                                beginAtZero: true,
                                title: { display: true, text: 'Cantidad de Citas', color: textColor },
                                ticks: { color: tickColor },
                                grid: { color: gridColor }
                            },
                            y: {
                                title: { display: true, text: 'Empleado', color: textColor },
                                ticks: { color: tickColor },
                                grid: { color: gridColor }
                            }
                        },
                        plugins: {
                            legend: { display: false },
                            title: { display: true, text: 'Top 5 Empleados con Más Citas', color: textColor }
                        }
                    }
                });
            }
        </script>
    </form>
</body>
</html>