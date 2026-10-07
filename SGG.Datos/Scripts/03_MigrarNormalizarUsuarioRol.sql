-- ============================================
-- SGG - Migración: Normalizar Usuario y Rol
-- (Ejecutar UNA SOLA VEZ sobre la base SGG existente)
-- Los usuarios confirmaron tener roles exactos: Administrador, Recepcionista, Entrenador
-- ============================================
USE SGG;
GO

-- ============================================
-- PASO 1: Crear tabla Roles + insertar 3 roles fijos
-- ============================================
IF OBJECT_ID('Roles', 'U') IS NOT NULL
    DROP TABLE Roles;
GO

CREATE TABLE Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);
GO

INSERT INTO Roles (Nombre) VALUES
    ('Administrador'),
    ('Recepcionista'),
    ('Entrenador');
GO

-- ============================================
-- PASO 2: Agregar columnas nuevas a Usuarios
-- Dni nullable (para no romper UNIQUE), RolId nullable (para migrar datos)
-- ============================================
IF COL_LENGTH('Usuarios', 'Apellido') IS NULL
    ALTER TABLE Usuarios ADD Apellido NVARCHAR(100) NULL;
GO

IF COL_LENGTH('Usuarios', 'Direccion') IS NULL
    ALTER TABLE Usuarios ADD Direccion NVARCHAR(200) NULL;
GO

IF COL_LENGTH('Usuarios', 'Telefono') IS NULL
    ALTER TABLE Usuarios ADD Telefono NVARCHAR(20) NULL;
GO

IF COL_LENGTH('Usuarios', 'Dni') IS NULL
    ALTER TABLE Usuarios ADD Dni NVARCHAR(20) NULL;
GO

IF COL_LENGTH('Usuarios', 'RolId') IS NULL
    ALTER TABLE Usuarios ADD RolId INT NULL;
GO

-- ============================================
-- PASO 3: Migrar datos — Rol string -> RolId
-- ============================================
UPDATE u
SET u.RolId = r.Id
FROM Usuarios u
LEFT JOIN Roles r ON r.Nombre = u.Rol;
GO

-- ============================================
-- PASO 4: RolId -> NOT NULL + FK a Roles
-- ============================================
ALTER TABLE Usuarios ALTER COLUMN RolId INT NOT NULL;
GO

ALTER TABLE Usuarios
    ADD CONSTRAINT FK_Usuarios_Roles
    FOREIGN KEY (RolId) REFERENCES Roles(Id);
GO

-- ============================================
-- PASO 5: Eliminar columna vieja Rol y aplicar constraints
-- ============================================
ALTER TABLE Usuarios DROP COLUMN Rol;
GO

-- UNIQUE en Dni: índice único FILTRADO que ignora los NULL.
-- En SQL Server un UNIQUE constraint estándar NO permite más de un NULL
-- (solo una fila NULL). Con el filtro WHERE Dni IS NOT NULL se exige
-- unicidad solo entre los DNI reales y los usuarios viejos quedan NULL sin romper.
CREATE UNIQUE INDEX UQ_Usuarios_Dni
ON Usuarios(Dni)
WHERE Dni IS NOT NULL;
GO

-- Email ya tiene UNIQUE en el esquema original (no requiere cambio)
-- Verificación final (opcional):
-- SELECT COLUMN_NAME, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='Usuarios';
-- SELECT * FROM Roles;
GO
