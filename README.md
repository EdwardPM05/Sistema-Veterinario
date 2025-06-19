# Sistema-Veterinario
-- ===============================
-- TABLAS DE CATEGORÍAS Y SERVICIOS
-- ===============================

CREATE TABLE CategoriasProductos (
    CategoriaProductoID INT PRIMARY KEY IDENTITY(1,1),
    NombreCategoria NVARCHAR(100) NOT NULL
);

CREATE TABLE Subcategoria (
    SubcategoriaID INT PRIMARY KEY IDENTITY(1,1),
    CategoriaProductoID INT NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(MAX),
    FOREIGN KEY (CategoriaProductoID) REFERENCES CategoriasProductos(CategoriaProductoID)
);

CREATE TABLE Servicios (
    ServicioID INT PRIMARY KEY IDENTITY(1,1),
    NombreServicio NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(MAX),
    Precio DECIMAL(10,2) NOT NULL CHECK (Precio >= 0),
    SubcategoriaID INT ,
    FOREIGN KEY (SubcategoriaID) REFERENCES Subcategoria(SubcategoriaID)
);

-- ===============================
-- TABLAS DE PERSONAL
-- ===============================

CREATE TABLE Roles (
    RolID INT PRIMARY KEY IDENTITY(1,1),
    NombreRol NVARCHAR(50) NOT NULL
);

CREATE TABLE Empleados (
    EmpleadoID INT PRIMARY KEY IDENTITY(1,1),
    PrimerNombre NVARCHAR(50) NOT NULL,
    ApellidoPaterno NVARCHAR(50) NOT NULL,
    ApellidoMaterno NVARCHAR(50) NOT NULL,
    DNI CHAR(8) NOT NULL,
    Correo NVARCHAR(100) NOT NULL DEFAULT 'a@gmail.com',
    Telefono VARCHAR(30),
    RolID INT NOT NULL,
    CONSTRAINT UQ_Empleados_DNI UNIQUE (DNI),
    CONSTRAINT CHK_Empleados_DNI CHECK (DNI NOT LIKE '%[^0-9]%'),
    FOREIGN KEY (RolID) REFERENCES Roles(RolID)
);

-- ===============================
-- TABLAS DE CLIENTES Y MASCOTAS
-- ===============================

CREATE TABLE Clientes (
    ClienteID INT PRIMARY KEY IDENTITY(1,1),
    PrimerNombre NVARCHAR(50) NOT NULL,
    ApellidoPaterno NVARCHAR(50) NOT NULL,
    ApellidoMaterno NVARCHAR(50) NOT NULL,
    DNI CHAR(8) NOT NULL,
    Telefono VARCHAR(30),
    Direccion NVARCHAR(200),
    Correo NVARCHAR(100) NOT NULL DEFAULT 'a@gmail.com',
    CONSTRAINT UQ_Clientes_DNI UNIQUE (DNI),
    CONSTRAINT CHK_Clientes_DNI CHECK (DNI NOT LIKE '%[^0-9]%')
);

CREATE TABLE Especies (
    EspecieID INT PRIMARY KEY IDENTITY(1,1),
    NombreEspecie NVARCHAR(50) NOT NULL
);

CREATE TABLE Razas (
    RazaID INT PRIMARY KEY IDENTITY(1,1),
    NombreRaza NVARCHAR(50) NOT NULL,
    EspecieID INT NOT NULL,
    FOREIGN KEY (EspecieID) REFERENCES Especies(EspecieID)
);

CREATE TABLE Mascotas (
    MascotaID INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(50) NOT NULL,
    Edad INT CHECK (Edad >= 0),
    Sexo CHAR(1) NOT NULL CHECK (Sexo IN ('M', 'F', 'O')),
    ClienteID INT NOT NULL,
    RazaID INT NOT NULL,
    FOREIGN KEY (ClienteID) REFERENCES Clientes(ClienteID),
    FOREIGN KEY (RazaID) REFERENCES Razas(RazaID)
);

-- ===============================
-- TABLAS DE CITAS Y SERVICIOS EN CITA
-- ===============================

CREATE TABLE Citas (
    CitaID INT PRIMARY KEY IDENTITY(1,1),
    Fecha DATETIME NOT NULL DEFAULT GETDATE(),
    MascotaID INT NOT NULL,
    EmpleadoID INT NOT NULL,
    FOREIGN KEY (MascotaID) REFERENCES Mascotas(MascotaID),
    FOREIGN KEY (EmpleadoID) REFERENCES Empleados(EmpleadoID)
);
CREATE TABLE CitaServicios (
    CitaServicioID INT PRIMARY KEY IDENTITY(1,1),
    CitaID INT NOT NULL,
    ServicioID INT NOT NULL,
    Cantidad INT NOT NULL CHECK (Cantidad > 0),
    PrecioUnitario DECIMAL(10, 2) NOT NULL, -- Columna para guardar el precio del servicio en el momento de la transacción
    -- TotalServicio AS (Cantidad * PrecioUnitario) PERSISTED, -- Opcional: Columna calculada si tu SQL Server lo permite y la necesitas persistida
    FOREIGN KEY (CitaID) REFERENCES Citas(CitaID),
    FOREIGN KEY (ServicioID) REFERENCES Servicios(ServicioID)
);

SELECT name
FROM sys.check_constraints
WHERE parent_object_id = OBJECT_ID('dbo.Mascotas') AND col_name(parent_object_id, parent_column_id) = 'Sexo';

-- Reemplaza [NombreDeLaRestriccion] con el nombre real que obtuviste.
ALTER TABLE Mascotas
DROP CONSTRAINT [NombreDeLaRestriccion];

ALTER TABLE Mascotas
ADD CONSTRAINT CK_Mascotas_Sexo_MH CHECK (Sexo IN ('M', 'H'));

-- Encuentra todas las mascotas con un valor de Sexo diferente a 'M' o 'H'
SELECT * FROM Mascotas WHERE Sexo NOT IN ('M', 'H');

-- Si encuentras alguna, puedes actualizarla manualmente. Por ejemplo, si tienes 'F' y quieres que sea 'H':
-- UPDATE Mascotas SET Sexo = 'H' WHERE Sexo = 'F';
