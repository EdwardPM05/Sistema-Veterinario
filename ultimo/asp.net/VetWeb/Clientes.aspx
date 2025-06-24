<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Clientes.aspx.cs" Inherits="VetWeb.Clientes" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Gestión de Clientes - VetWeb</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    
    <!-- Favicon -->
 <link href="~/favicon.ico" rel="shortcut icon" type="image/x-icon" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" />
    <style>
        html, body {
            box-sizing: border-box;
        }
        *, *::before, *::after {
            box-sizing: inherit;
        }

        :root {
            /* Paleta 3: Elegante y Moderna (Tonos Gris Azulado y Toques de Blanco/Plata) */
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
            
            /* Colores de citas (pueden ajustarse para que coincidan con esta paleta si es necesario) */
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
            color: var(--sidebar-hover-text); /* El color del texto de la marca del sidebar (VetWeb) sigue al hover-text para contraste */
            font-size: 1.8rem;
            font-weight: 700;
            padding: 0 20px 1rem;
            border-bottom: 1px solid var(--sidebar-hover-bg); /* El borde sigue el hover-bg */
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
            background-color: var(--card-header-bg); /* Usa el nuevo color de encabezado */
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
            background-color: #5A7F9D; /* Gris azulado más claro de Paleta 3 para "Agregar Nuevo Cliente" */
            color: white;
            border-radius: 8px;
            padding: 10px 20px;
            font-size: 1.1rem;
            transition: background-color 0.3s ease;
        }
        .btn-custom:hover {
            background-color: #4A6572; /* Gris azulado ligeramente más oscuro al pasar el ratón */
            color: white;
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

        /* Table enhancements */
        .table-striped tbody tr:nth-of-type(odd) {
            background-color: #fff; /* Force white background for odd rows (if table-striped is still used) */
        }
        .table-hover tbody tr:hover {
            background-color: rgba(0, 0, 0, 0.04); /* Un hover más suave */
        }
        .table-primary th {
            background-color: #5A7F9D; /* Gris azulado más claro de Paleta 3 para encabezado de tabla */
            color: white;
            border-color: #5A7F9D;
        }

        .modal-header {
            background-color: #5A7F9D; /* Gris azulado más claro de Paleta 3 para el encabezado del modal */
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
            border-color: #5A7F9D; /* Gris azulado de Paleta 3 para el foco */
            box-shadow: 0 0 0 0.25rem rgba(90, 127, 157, 0.25); /* Sombra de foco gris azulado */
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
            background-color: #5A7F9D; /* Gris azulado más claro de Paleta 3 para "Buscar" */
            color: white;
            border-color: #5A7F9D;
            border-top-right-radius: 0;
            border-bottom-right-radius: 0;
        }
        .search-input-group #btnBuscarCliente:hover { /* Updated ID */
            background-color: #4A6572; /* Gris azulado más oscuro al pasar el ratón */
            border-color: #4A6572;
        }

        /* Specific styles for "Limpiar" button */
        .search-input-group #btnLimpiarBusquedaCliente { /* Updated ID */
            background-color: #95A5A6; /* Gris Plata de Paleta 3 para "Limpiar" */
            color: white;
            border-color: #95A5A6;
            border-top-right-radius: 8px; /* Rounded corner on the far right */
            border-bottom-right-radius: 8px;
        }
        .search-input-group #btnLimpiarBusquedaCliente:hover { /* Updated ID */
            background-color: #7F8C8D; /* Gris Plata más oscuro al pasar el ratón */
            border-color: #7F8C8D;
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
            background-color: #f8d7da; /* Light red (standard Bootstrap danger) */
            color: #721c24; /* Dark red text (standard Bootstrap danger) */
            border-color: #f5c6cb; /* Red border (standard Bootstrap danger) */
            position: relative; /* Needed for pseudo-elements */
        }

        #clienteModal .alert-danger::before { /* Changed to clienteModal */
            content: "\2716"; /* Unicode 'heavy multiplication x' for a cross */
            font-size: 1.2rem;
            margin-right: 0.5rem;
            vertical-align: middle;
            display: inline-block;
            line-height: 1; /* Align with text */
            color: #DC3545; /* Rojo de Bootstrap para el ícono de peligro */
        }

        #clienteModal .alert-success::before { /* Changed to clienteModal */
            content: "\2714"; /* Unicode 'heavy check mark' */
            font-size: 1.2rem;
            margin-right: 0.5rem;
            vertical-align: middle;
            display: inline-block;
            line-height: 1;
            color: #28a745; /* Verde de Bootstrap para el ícono de éxito */
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
               <i class="bi bi-hospital-fill"></i> VetWeb
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

            </ul>
        </nav>

        <div class="content">


            <div class="input-group mb-3 search-input-group">
                <asp:TextBox ID="txtBuscarNombreCliente" runat="server" CssClass="form-control" Placeholder="Buscar por nombre o DNI" />
                <asp:LinkButton ID="btnBuscarCliente" runat="server" OnClick="btnBuscarCliente_Click"
                    CssClass="btn btn-outline-secondary" 
                    ToolTip="Buscar">
                    <i class="bi bi-search fs-6"></i> 
                    <span class="sr-only">Buscar</span> 
                </asp:LinkButton>

                <asp:LinkButton ID="btnLimpiarBusquedaCliente" runat="server" OnClick="btnLimpiarBusquedaCliente_Click"
                    CssClass="btn btn-outline-secondary"
                    ToolTip="Limpiar">
                    <i class="bi bi-x-lg fs-6"></i> 
                    <span class="sr-only">Limpiar</span> 
                </asp:LinkButton>

            </div>

            <div class="d-flex justify-content-end align-items-center mb-4">
                <button type="button" class="btn btn-custom me-2" data-bs-toggle="modal" data-bs-target="#clienteModal" data-mode="add">
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-person-plus me-2" viewBox="0 0 16 16">
                        <path d="M6 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6m2-3a2 2 0 1 1-4 0 2 2 0 0 1 4 0m4 8c0 1-1 1-1 1H1s-1 0-1-1 1-4 6-4 6 3 6 4m-1-.004c-.001-.246-.154-.986-.832-1.664C11.516 10.68 10.289 10 8 10s-3.516.68-4.168 1.332c-.678.678-.83 1.418-.832 1.664z"/>
                        <path fill-rule="evenodd" d="M13.5 5a.5.5 0 0 1 .5.5V7h1.5a.5.5 0 0 1 0 1H14v1.5a.5.5 0 0 1-1 0V8h-1.5a.5.5 0 0 1 0-1H13V5.5a.5.5 0 0 1 .5-.5"/>
                    </svg>
                    Agregar Nuevo Cliente
                </button>

                <%-- **NUEVO BOTÓN PARA IMPRIMIR PDF (CON ESTILO CUSTOM)** --%>
                <asp:LinkButton ID="btnImprimirPdf" runat="server" CssClass="btn btn-custom" OnClick="btnImprimirPdf_Click">
                    <i class="bi bi-file-earmark-pdf me-2"></i> Reporte PDF
                </asp:LinkButton>
            </div>

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
                    <asp:TemplateField HeaderText="Acciones" ItemStyle-Width="80px">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnEditarCliente" runat="server" CommandName="Editar"
                                CssClass="icon-action icon-edit-custom ms-2 me-2"
                                CommandArgument="<%# Container.DataItemIndex %>"
                                ToolTip="Editar Cliente">
                                <i class="bi bi-pencil fs-5"></i>
                                <span class="sr-only">Editar</span>
                            </asp:LinkButton>
                
                            <asp:LinkButton ID="btnEliminarCliente" runat="server" CommandName="Eliminar"
                                CssClass="icon-action text-danger me-2"
                                CommandArgument="<%# Container.DataItemIndex %>"
                                OnClientClick="return confirm('¿Está seguro de que desea eliminar este cliente?');"
                                ToolTip="Eliminar Cliente">
                                <i class="bi bi-trash fs-5"></i>
                                <span class="sr-only">Eliminar</span>
                            </asp:LinkButton>
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