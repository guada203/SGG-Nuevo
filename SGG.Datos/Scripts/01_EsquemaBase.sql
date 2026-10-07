-- ============================================
-- SGG - Esquema completo de base de datos
-- (Ejecutar solo si la base SGG no existe o para recrearla de cero)
-- ============================================

-- CREATE DATABASE SGG;   -- descomentar solo si la base no existe todavía
-- GO
USE SGG;
GO

-- 1. MEMBRESIAS
CREATE TABLE Membresias (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TipoActividad NVARCHAR(20) NOT NULL,
    Precio DECIMAL(10,2) NOT NULL,
    FechaInicio DATE NOT NULL,
    FechaVencimiento DATE NOT NULL,
    Vigente BIT NOT NULL DEFAULT 1
);
GO

-- 2. USUARIOS
CREATE TABLE Usuarios (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Rol NVARCHAR(20) NOT NULL,
    Activo BIT NOT NULL DEFAULT 1
);
GO

-- 3. SOCIOS
CREATE TABLE Socios (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Nombre NVARCHAR(100) NOT NULL,
    Apellido NVARCHAR(100) NOT NULL,
    Dni NVARCHAR(20) NOT NULL UNIQUE,
    FechaNacimiento DATE NOT NULL,
    Telefono NVARCHAR(20),
    Email NVARCHAR(100),
    Activo BIT NOT NULL DEFAULT 1,
    MembresiaId INT NOT NULL,
    CONSTRAINT FK_Socios_Membresias FOREIGN KEY (MembresiaId) REFERENCES Membresias(Id)
);
GO

-- 4. PAGOS
CREATE TABLE Pagos (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SocioId INT NOT NULL,
    Monto DECIMAL(10,2) NOT NULL,
    FechaPago DATE NOT NULL,
    MetodoPago NVARCHAR(30) NOT NULL,
    CONSTRAINT FK_Pagos_Socios FOREIGN KEY (SocioId) REFERENCES Socios(Id)
);
GO

-- 5. ASISTENCIAS
CREATE TABLE Asistencias (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SocioId INT NOT NULL,
    FechaHoraIngreso DATETIME NOT NULL,
    CONSTRAINT FK_Asistencias_Socios FOREIGN KEY (SocioId) REFERENCES Socios(Id)
);
GO

-- 6. RUTINAS
CREATE TABLE Rutinas (
    Id INT PRIMARY KEY IDENTITY(1,1),
    SocioId INT NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Descripcion NVARCHAR(MAX),
    FechaAsignacion DATE NOT NULL,
    CONSTRAINT FK_Rutinas_Socios FOREIGN KEY (SocioId) REFERENCES Socios(Id)
);
GO