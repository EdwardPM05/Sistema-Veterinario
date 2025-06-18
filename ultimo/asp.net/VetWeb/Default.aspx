<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="VetWeb.Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Dashboard VetWeb</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <!-- Bootstrap CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        body {
            min-height: 100vh;
            overflow-x: hidden;
        }
        .sidebar {
            position: fixed;
            top: 0;
            left: 0;
            height: 100vh;
            width: 220px;
            background-color: #343a40;
            padding-top: 1rem;
        }
        .sidebar .nav-link {
            color: #adb5bd;
            font-weight: 500;
            padding: 12px 20px;
        }
        .sidebar .nav-link:hover, .sidebar .nav-link.active {
            background-color: #495057;
            color: #fff;
        }
        .sidebar-brand {
            color: #fff;
            font-size: 1.5rem;
            font-weight: 700;
            padding: 0 20px 1rem;
            border-bottom: 1px solid #495057;
            margin-bottom: 1rem;
            display: block;
            text-decoration: none;
        }
        .content {
            margin-left: 220px;
            padding: 2rem;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">

        <nav class="sidebar">
            <a href="Default.aspx" class="sidebar-brand">VetWeb</a>
                <a class="nav-link active" href="Default.aspx">Dashboard</a>
                <a class="nav-link" href="Clientes.aspx">Clientes</a>
                <a class="nav-link" href="Razas.aspx">Razas</a>
                <a class="nav-link" href="Especies.aspx">Especies</a>
                <a class="nav-link" href="Mascotas.aspx">Mascotas</a>
                <a class="nav-link" href="Roles.aspx">Roles</a>
                <a class="nav-link" href="Empleados.aspx">Empleados</a>
                <a class="nav-link" href="CategoriasProductos.aspx">Cat. Productos</a>
                <a class="nav-link" href="Subcategoria.aspx">Subcategorías</a>
                <a class="nav-link" href="Servicios.aspx">Servicios</a>
                <a class="nav-link" href="Citas.aspx">Citas</a>
                <a class="nav-link" href="CitaServicios.aspx">Cita-Servicios</a>
                <a class="nav-link" href="VentaServicios.aspx">Venta Servicios</a>
            </nav>

        <div class="content">
            <h2>Dashboard</h2>
            <hr />
            <p>Selecciona una opción del menú lateral para gestionar la aplicación.</p>
        </div>

        <!-- Bootstrap JS Bundle -->
        <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    </form>
</body>
</html>
