<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CategoriasProductos.aspx.cs" Inherits="VetWeb.CategoriasProductos" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Gestión de Categorías de Productos - VetWeb</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        body {
            min-height: 100vh;
            overflow-x: hidden;
            font-family: 'Inter', sans-serif; /* Using Inter font */
            background-color: #f8f9fa; /* Light background */
        }
        .sidebar {
            position: fixed;
            top: 0;
            left: 0;
            height: 100vh;
            width: 220px;
            background-color: #343a40;
            padding-top: 1rem;
            box-shadow: 2px 0 5px rgba(0,0,0,0.1); /* Subtle shadow */
        }
        .sidebar .nav-link {
            color: #adb5bd;
            font-weight: 500;
            padding: 12px 20px;
            transition: background-color 0.3s, color 0.3s; /* Smooth transitions */
            border-radius: 8px; /* Rounded corners for nav links */
            margin: 0 10px 5px 10px; /* Spacing */
        }
        .sidebar .nav-link:hover, .sidebar .nav-link.active {
            background-color: #495057;
            color: #fff;
        }
        .sidebar-brand {
            color: #fff;
            font-size: 1.8rem; /* Slightly larger font */
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
            background-color: rgba(0, 0, 0, 0.03);
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
        .search-input-group #btnBuscarCategoria { /* Updated ID */
            background-color: #0d6efd; /* Primary blue */
            color: white;
            border-color: #0d6efd;
            border-top-right-radius: 0;
            border-bottom-right-radius: 0;
        }
        .search-input-group #btnBuscarCategoria:hover { /* Updated ID */
            background-color: #0b5ed7; /* Darker blue on hover */
            border-color: #0b5ed7;
        }

        /* Specific styles for "Limpiar" button */
        .search-input-group #btnLimpiarBusquedaCategoria { /* Updated ID */
            background-color: #6c757d; /* Secondary gray */
            color: white;
            border-color: #6c757d;
            border-top-right-radius: 8px; /* Rounded corner on the far right */
            border-bottom-right-radius: 8px;
        }
        .search-input-group #btnLimpiarBusquedaCategoria:hover { /* Updated ID */
            background-color: #5c636a; /* Darker gray on hover */
            border-color: #565e64;
        }

        /* Ensure correct border-radius for input-group buttons */
        .search-input-group .btn:not(:last-child) {
            border-right: 1px solid rgba(0,0,0,.125); /* Small separator between buttons */
        }

        /* Custom styles for alert messages within the modal */
        #categoriaProductoModal .alert { /* Changed to categoriaProductoModal */
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

        #categoriaProductoModal .alert-danger { /* Changed to categoriaProductoModal */
            background-color: #f8d7da; /* Light red */
            color: #721c24; /* Dark red text */
            border-color: #f5c6cb; /* Red border */
            position: relative; /* Needed for pseudo-elements */
        }

        #categoriaProductoModal .alert-danger::before { /* Changed to categoriaProductoModal */
            content: "\2716"; /* Unicode 'heavy multiplication x' for a cross */
            font-size: 1.2rem;
            margin-right: 0.5rem;
            vertical-align: middle;
            display: inline-block;
            line-height: 1; /* Align with text */
            color: #dc3545; /* Bootstrap red */
        }

        #categoriaProductoModal .alert-success::before { /* Changed to categoriaProductoModal */
            content: "\2714"; /* Unicode 'heavy check mark' */
            font-size: 1.2rem;
            margin-right: 0.5rem;
            vertical-align: middle;
            display: inline-block;
            line-height: 1;
            color: #28a745; /* Bootstrap green */
        }

        /* Increase modal width for better display of long messages */
        #categoriaProductoModal .modal-dialog { /* Changed to categoriaProductoModal */
            max-width: 500px; 
            width: 90%; /* Responsive width */
        }

        @media (min-width: 576px) {
            #categoriaProductoModal .modal-dialog {
                max-width: 550px; 
            }
        }
        @media (min-width: 768px) {
            #categoriaProductoModal .modal-dialog {
                max-width: 600px; 
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <!-- Sidebar Navigation -->
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
                <li class="nav-item"><a class="nav-link active" href="CategoriasProductos.aspx">Cat. Productos</a></li>
                <li class="nav-item"><a class="nav-link" href="Subcategoria.aspx">Subcategorías</a></li>
                <li class="nav-item"><a class="nav-link" href="Servicios.aspx">Servicios</a></li>
                <li class="nav-item"><a class="nav-link" href="Citas.aspx">Citas</a></li>
                <li class="nav-item"><a class="nav-link" href="CitaServicios.aspx">Cita-Servicios</a></li>
                <li class="nav-item"><a class="nav-link" href="VentaServicios.aspx">Venta Servicios</a></li>
            </ul>
        </nav>

        <!-- Main Content Area -->
        <div class="content">
            <h2 class="mb-4">Gestión de Categorías de Productos</h2>

            <!-- Search Bar -->
            <div class="input-group mb-3 search-input-group">
                <asp:TextBox ID="txtBuscarNombreCategoria" runat="server" CssClass="form-control" Placeholder="Buscar por nombre de categoría" />
                <asp:Button ID="btnBuscarCategoria" runat="server" CssClass="btn btn-outline-secondary" Text="Buscar" OnClick="btnBuscarCategoria_Click" />
                <asp:Button ID="btnLimpiarBusquedaCategoria" runat="server" CssClass="btn btn-outline-secondary" Text="Limpiar" OnClick="btnLimpiarBusquedaCategoria_Click" />
            </div>

            <!-- Button to open the Add/Edit Modal -->
            <button type="button" class="btn btn-custom mb-4" data-bs-toggle="modal" data-bs-target="#categoriaProductoModal" data-mode="add">
                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-plus-circle me-2" viewBox="0 0 16 16">
                    <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14m0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16"/>
                    <path d="M8 4a.5.5 0 0 1 .5.5v3h3a.5.5 0 0 1 0 1h-3v3a.5.5 0 0 1-1 0v-3h-3a.5.5 0 0 1 0-1h3v-3A.5.5 0 0 1 8 4"/>
                </svg>
                Agregar Nueva Categoría
            </button>

            <hr />

            <!-- GridView to display existing categories -->
            <asp:GridView ID="gvCategoriasProductos" runat="server" AutoGenerateColumns="False" OnRowCommand="gvCategoriasProductos_RowCommand"
                CssClass="table table-striped table-bordered table-hover"
                HeaderStyle-CssClass="table-primary"
                DataKeyNames="CategoriaProductoID"> 
                <Columns>
                    <%-- Removed CategoriaProductoID column from display, using DataKeyNames instead --%>
                    <asp:BoundField DataField="NombreCategoria" HeaderText="Nombre de Categoría" />
                    <asp:TemplateField HeaderText="Acciones" ItemStyle-Width="180px">
                        <ItemTemplate>
                            <asp:Button ID="btnEditarCategoria" runat="server" CommandName="Editar" Text="Editar" CssClass="btn btn-warning btn-sm me-2" CommandArgument="<%# Container.DataItemIndex %>" />
                            <asp:Button ID="btnEliminarCategoria" runat="server" CommandName="Eliminar" Text="Eliminar" CssClass="btn btn-danger btn-sm" CommandArgument="<%# Container.DataItemIndex %>" OnClientClick="return confirm('¿Está seguro de que desea eliminar esta categoría de producto?');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <!-- Bootstrap Modal for Add/Edit CategoriaProducto -->
        <div class="modal fade" id="categoriaProductoModal" tabindex="-1" aria-labelledby="categoriaProductoModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="categoriaProductoModalLabel">Gestión de Categoría de Producto</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <!-- Message Label -->
                        <asp:Label ID="lblMensaje" runat="server" EnableViewState="false"></asp:Label><br />
                        
                        <!-- Input field for NombreCategoria -->
                        <div class="mb-3">
                            <label for="txtNombreCategoria" class="form-label">Nombre de la Categoría</label>
                            <asp:TextBox ID="txtNombreCategoria" runat="server" CssClass="form-control" Placeholder="Nombre de la Categoría" />
                        </div>
                        
                        <!-- Hidden Field for CategoriaProductoID -->
                        <asp:HiddenField ID="hfCategoriaProductoID" runat="server" />
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>
                        <!-- Add and Update Buttons -->
                        <asp:Button ID="btnAgregar" runat="server" CssClass="btn btn-success" Text="Guardar" OnClick="btnAgregar_Click" />
                        <asp:Button ID="btnActualizar" runat="server" CssClass="btn btn-primary" Text="Actualizar" OnClick="btnActualizar_Click" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Bootstrap JS Bundle -->
        <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
        <script>
            // JavaScript function to clear the form fields and set "Add" mode for CategoriaProducto Modal
            function clearModalFormAndSetAddModeCategoriaProducto() {
                var txtNombreCategoria = document.getElementById('<%= txtNombreCategoria.ClientID %>');
                var hfCategoriaProductoID = document.getElementById('<%= hfCategoriaProductoID.ClientID %>');
                var lblMensaje = document.getElementById('<%= lblMensaje.ClientID %>');
                var btnAgregar = document.getElementById('<%= btnAgregar.ClientID %>');
                var btnActualizar = document.getElementById('<%= btnActualizar.ClientID %>');
                var modalTitle = document.getElementById('categoriaProductoModalLabel');

                if (txtNombreCategoria) txtNombreCategoria.value = '';
                if (hfCategoriaProductoID) hfCategoriaProductoID.value = '';
                if (lblMensaje) {
                    lblMensaje.innerHTML = '';
                    lblMensaje.className = '';
                }

                if (btnAgregar) btnAgregar.style.display = 'inline-block';
                if (btnActualizar) btnActualizar.style.display = 'none';

                if (modalTitle) modalTitle.innerText = 'Agregar Nueva Categoría';
            }

            // JavaScript function to show the CategoriaProducto modal (called from C#)
            function showCategoriaProductoModal() {
                var myModal = new bootstrap.Modal(document.getElementById('categoriaProductoModal'));
                myModal.show();
            }

            // JavaScript function to hide the CategoriaProducto modal (called from C#)
            function hideCategoriaProductoModal() {
                var myModal = bootstrap.Modal.getInstance(document.getElementById('categoriaProductoModal'));
                if (myModal) {
                    myModal.hide();
                }
            }

            // Event listener for when the CategoriaProducto modal is shown (for "Agregar" button)
            document.addEventListener('DOMContentLoaded', function () {
                var categoriaProductoModal = document.getElementById('categoriaProductoModal');
                if (categoriaProductoModal) {
                    categoriaProductoModal.addEventListener('shown.bs.modal', function (event) {
                        var button = event.relatedTarget; 
                        var isAddModeButton = button && button.getAttribute('data-mode') === 'add';

                        if (isAddModeButton) {
                            clearModalFormAndSetAddModeCategoriaProducto();
                        }
                    });
                }
            });
        </script>
    </form>
</body>
</html>
