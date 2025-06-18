<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="VetWeb.Default" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Dashboard - VetWeb</title>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <!-- Bootstrap CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
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
        .card {
            border-radius: 10px; /* Esquinas redondeadas para las tarjetas */
            box-shadow: 0 4px 10px rgba(0,0,0,0.08); /* Sombra más pronunciada */
            transition: transform 0.2s ease-in-out; /* Animación al pasar el ratón */
        }
        .card:hover {
            transform: translateY(-5px); /* Pequeño levantamiento al pasar el ratón */
        }
        .card-header {
            background-color: #0d6efd; /* Azul primario para el encabezado de las tarjetas */
            color: white;
            font-weight: 600;
            border-top-left-radius: 10px;
            border-top-right-radius: 10px;
        }
        .card-body h3 {
            font-size: 2.5rem; /* Tamaño de fuente grande para los números */
            font-weight: 700;
            color: #343a40;
        }
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
        /* Estilos específicos para los contenedores de gráficos */
        .chart-container {
            position: relative;
            height: 40vh; /* Altura responsiva */
            width: 100%;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <!-- Necesario para que el C# pueda ejecutar scripts JavaScript -->
        <asp:ScriptManager runat="server"></asp:ScriptManager>

        <!-- Barra de Navegación Lateral -->
        <nav class="sidebar">
            <a href="Default.aspx" class="sidebar-brand">VetWeb</a>
            <ul class="nav flex-column">
                <li class="nav-item"><a class="nav-link active" href="Default.aspx">Dashboard</a></li>
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
                <li class="nav-item"><a class="nav-link" href="CitaServicios.aspx">Cita-Servicios</a></li>
                <li class="nav-item"><a class="nav-link" href="VentaServicios.aspx">Venta Servicios</a></li>
            </ul>
        </nav>

        <!-- Área de Contenido Principal -->
        <div class="content">
            <h2 class="mb-4">Dashboard</h2>
            <hr />

            <!-- Tarjetas de Resumen -->
            <div class="row g-4 mb-5">
                <!-- Tarjeta Total Clientes -->
                <div class="col-md-6 col-lg-3">
                    <div class="card text-center">
                        <div class="card-header">
                            Clientes Registrados
                        </div>
                        <div class="card-body">
                            <h3 class="card-title">
                                <asp:Label ID="lblTotalClientes" runat="server" Text="0"></asp:Label>
                            </h3>
                            <p class="card-text text-muted">Total de clientes en tu base de datos.</p>
                        </div>
                    </div>
                </div>

                <!-- Tarjeta Total Mascotas -->
                <div class="col-md-6 col-lg-3">
                    <div class="card text-center">
                        <div class="card-header">
                            Mascotas Registradas
                        </div>
                        <div class="card-body">
                            <h3 class="card-title">
                                <asp:Label ID="lblTotalMascotas" runat="server" Text="0"></asp:Label>
                            </h3>
                            <p class="card-text text-muted">Total de mascotas registradas.</p>
                        </div>
                    </div>
                </div>

                <!-- Tarjeta Total Empleados -->
                <div class="col-md-6 col-lg-3">
                    <div class="card text-center">
                        <div class="card-header">
                            Empleados Activos
                        </div>
                        <div class="card-body">
                            <h3 class="card-title">
                                <asp:Label ID="lblTotalEmpleados" runat="server" Text="0"></asp:Label>
                            </h3>
                            <p class="card-text text-muted">Personal veterinario y asistente.</p>
                        </div>
                    </div>
                </div>

                <!-- Tarjeta Citas Pendientes Hoy -->
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

            <!-- Sección de Gráficos Existentes -->
            <div class="row g-4 mb-5">
                <!-- Gráfico de Barras: Servicios por Subcategoría -->
                <div class="col-lg-6">
                    <div class="card">
                        <div class="card-header">
                            Servicios Registrados por Subcategoría
                        </div>
                        <div class="card-body">
                            <div class="chart-container">
                                <canvas id="serviciosSubcategoriaChart"></canvas>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Gráfico de Líneas: Citas por Mes -->
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
            </div>

            <!-- Nuevos Gráficos -->
            <div class="row g-4 mb-5">
                <!-- Gráfico de Donas: Mascotas por Especie -->
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

                <!-- Gráfico de Barras: Top 5 Servicios Más Utilizados -->
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
            </div>

            <!-- Nuevo Gráfico de Empleados con Más Citas -->
            <div class="row g-4 mb-5">
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
                 <div class="col-lg-6">
                    <!-- Espacio en blanco o para otro gráfico futuro si se desea -->
                </div>
            </div>


            <!-- Sección de Próximas Citas -->
            <div class="row">
                <div class="col-12">
                    <div class="card">
                        <div class="card-header">
                            Próximas Citas
                        </div>
                        <div class="card-body">
                            <asp:GridView ID="gvProximasCitas" runat="server" AutoGenerateColumns="False"
                                CssClass="table table-striped table-bordered table-hover"
                                HeaderStyle-CssClass="table-primary">
                                <Columns>
                                    <asp:BoundField DataField="Fecha" HeaderText="Fecha" DataFormatString="{0:dd/MM/yyyy}" />
                                    <asp:BoundField DataField="NombreMascota" HeaderText="Mascota" />
                                    <asp:BoundField DataField="NombreCliente" HeaderText="Cliente" />
                                    <asp:BoundField DataField="NombreEmpleado" HeaderText="Empleado" />
                                </Columns>
                            </asp:GridView>
                            <asp:Label ID="lblNoCitas" runat="server" Text="No hay citas próximas programadas." Visible="false" CssClass="text-muted"></asp:Label>
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
            // Función JavaScript para dibujar el gráfico de Servicios por Subcategoría
            function drawServiciosSubcategoriaChart(labels, data) {
                const ctx = document.getElementById('serviciosSubcategoriaChart').getContext('2d');
                new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Número de Servicios',
                            data: data,
                            backgroundColor: [
                                'rgba(255, 99, 132, 0.6)',
                                'rgba(54, 162, 235, 0.6)',
                                'rgba(255, 206, 86, 0.6)',
                                'rgba(75, 192, 192, 0.6)',
                                'rgba(153, 102, 255, 0.6)',
                                'rgba(255, 159, 64, 0.6)',
                                'rgba(192, 192, 192, 0.6)', // Color adicional
                                'rgba(201, 203, 207, 0.6)'  // Color adicional
                            ],
                            borderColor: [
                                'rgba(255, 99, 132, 1)',
                                'rgba(54, 162, 235, 1)',
                                'rgba(255, 206, 86, 1)',
                                'rgba(75, 192, 192, 1)',
                                'rgba(153, 102, 255, 1)',
                                'rgba(255, 159, 64, 1)',
                                'rgba(192, 192, 192, 1)',
                                'rgba(201, 203, 207, 1)'
                            ],
                            borderWidth: 1
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false, // Permitir que el chart-container controle el tamaño
                        scales: {
                            y: {
                                beginAtZero: true,
                                title: {
                                    display: true,
                                    text: 'Cantidad de Servicios'
                                }
                            },
                            x: {
                                title: {
                                    display: true,
                                    text: 'Subcategoría'
                                }
                            }
                        },
                        plugins: {
                            legend: {
                                display: false // No mostrar leyenda para una sola serie de datos en barras
                            },
                            title: {
                                display: true,
                                text: 'Distribución de Servicios por Subcategoría'
                            }
                        }
                    }
                });
            }

            // Función JavaScript para dibujar el gráfico de Citas por Mes
            function drawCitasMesChart(labels, data) {
                const ctx = document.getElementById('citasMesChart').getContext('2d');
                new Chart(ctx, {
                    type: 'line',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Citas Programadas',
                            data: data,
                            borderColor: 'rgba(13, 110, 253, 1)', // Color azul de Bootstrap
                            backgroundColor: 'rgba(13, 110, 253, 0.2)', // Fondo suave
                            tension: 0.3, // Suaviza las líneas
                            fill: true,
                            pointBackgroundColor: 'rgba(13, 110, 253, 1)',
                            pointBorderColor: '#fff',
                            pointHoverBackgroundColor: '#fff',
                            pointHoverBorderColor: 'rgba(13, 110, 253, 1)'
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        scales: {
                            y: {
                                beginAtZero: true,
                                title: {
                                    display: true,
                                    text: 'Número de Citas'
                                }
                            },
                            x: {
                                title: {
                                    display: true,
                                    text: 'Mes'
                                }
                            }
                        },
                        plugins: {
                            legend: {
                                display: true,
                                position: 'top'
                            },
                            title: {
                                display: true,
                                text: 'Tendencia de Citas por Mes'
                            }
                        }
                    }
                });
            }

            // Función JavaScript para dibujar el gráfico de Donas: Mascotas por Especie
            function drawMascotasEspecieChart(labels, data) {
                const ctx = document.getElementById('mascotasEspecieChart').getContext('2d');
                new Chart(ctx, {
                    type: 'doughnut', // Tipo de gráfico: dona (donut)
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Cantidad de Mascotas',
                            data: data,
                            backgroundColor: [
                                'rgba(255, 159, 64, 0.6)',  // Naranja
                                'rgba(75, 192, 192, 0.6)',  // Verde azulado
                                'rgba(255, 99, 132, 0.6)',  // Rojo
                                'rgba(54, 162, 235, 0.6)',  // Azul
                                'rgba(153, 102, 255, 0.6)', // Púrpura
                                'rgba(201, 203, 207, 0.6)'  // Gris
                            ],
                            borderColor: '#fff', // Borde blanco entre segmentos
                            borderWidth: 2
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: {
                                position: 'right', // Leyenda a la derecha
                                align: 'start'
                            },
                            title: {
                                display: true,
                                text: 'Distribución de Mascotas por Especie'
                            }
                        }
                    }
                });
            }

            // Función JavaScript para dibujar el gráfico de Barras: Top 5 Servicios Más Utilizados
            function drawTopServiciosChart(labels, data) {
                const ctx = document.getElementById('topServiciosChart').getContext('2d');
                new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Veces Utilizado',
                            data: data,
                            backgroundColor: [
                                'rgba(0, 123, 255, 0.6)',  // Azul de Bootstrap
                                'rgba(40, 167, 69, 0.6)',  // Verde de Bootstrap
                                'rgba(255, 193, 7, 0.6)',  // Amarillo de Bootstrap
                                'rgba(220, 53, 69, 0.6)',  // Rojo de Bootstrap
                                'rgba(108, 117, 125, 0.6)' // Gris de Bootstrap
                            ],
                            borderColor: [
                                'rgba(0, 123, 255, 1)',
                                'rgba(40, 167, 69, 1)',
                                'rgba(255, 193, 7, 1)',
                                'rgba(220, 53, 69, 1)',
                                'rgba(108, 117, 125, 1)'
                            ],
                            borderWidth: 1
                        }]
                    },
                    options: {
                        indexAxis: 'y', // Hace el gráfico de barras horizontal
                        responsive: true,
                        maintainAspectRatio: false,
                        scales: {
                            x: {
                                beginAtZero: true,
                                title: {
                                    display: true,
                                    text: 'Número de Utilizaciones'
                                }
                            },
                            y: {
                                title: {
                                    display: true,
                                    text: 'Servicio'
                                }
                            }
                        },
                        plugins: {
                            legend: {
                                display: false
                            },
                            title: {
                                display: true,
                                text: 'Top 5 Servicios Más Utilizados'
                            }
                        }
                    }
                });
            }

            // Función JavaScript para dibujar el gráfico de Barras: Top 5 Empleados con Más Citas (NUEVO)
            function drawTopEmpleadosChart(labels, data) {
                const ctx = document.getElementById('topEmpleadosChart').getContext('2d');
                new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'Número de Citas',
                            data: data,
                            backgroundColor: [
                                'rgba(111, 66, 193, 0.6)', // Púrpura similar al btn-custom
                                'rgba(23, 162, 184, 0.6)', // Teal
                                'rgba(253, 126, 20, 0.6)', // Naranja oscuro
                                'rgba(102, 16, 242, 0.6)', // Púrpura más oscuro
                                'rgba(32, 201, 151, 0.6)'  // Verde menta
                            ],
                            borderColor: [
                                'rgba(111, 66, 193, 1)',
                                'rgba(23, 162, 184, 1)',
                                'rgba(253, 126, 20, 1)',
                                'rgba(102, 16, 242, 1)',
                                'rgba(32, 201, 151, 1)'
                            ],
                            borderWidth: 1
                        }]
                    },
                    options: {
                        indexAxis: 'y', // Hace el gráfico de barras horizontal
                        responsive: true,
                        maintainAspectRatio: false,
                        scales: {
                            x: {
                                beginAtZero: true,
                                title: {
                                    display: true,
                                    text: 'Cantidad de Citas'
                                }
                            },
                            y: {
                                title: {
                                    display: true,
                                    text: 'Empleado'
                                }
                            }
                        },
                        plugins: {
                            legend: {
                                display: false
                            },
                            title: {
                                display: true,
                                text: 'Top 5 Empleados con Más Citas'
                            }
                        }
                    }
                });
            }
        </script>
    </form>
</body>
</html>
