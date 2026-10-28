
-- ============================================
-- SGG - Migración: asignar un entrenador a cada socio
-- (Ejecutar sobre la base SGG existente)
-- Agrega la columna Socios.EntrenadorId para registrar
-- qué entrenador tiene a cargo a cada socio.
-- La usan dos módulos:
--   - Recepcionista: alta y edición de socios.
--   - Entrenador: "Mis Alumnos" (socios a cargo del entrenador logueado).
-- No se eliminan columnas ni datos existentes. La columna es
-- opcional: los socios ya cargados quedan sin entrenador asignado.
-- ============================================
USE SGG;
GO

-- ============================================
-- PASO 1: Agregar EntrenadorId a Socios
-- (NULL = socio todavía sin entrenador asignado)
-- ============================================
IF COL_LENGTH('Socios', 'EntrenadorId') IS NULL
    ALTER TABLE Socios
    ADD EntrenadorId INT NULL;
GO

-- ============================================
-- PASO 2: Crear la clave foránea hacia Usuarios
-- El entrenador es un Usuario con rol "Entrenador".
-- La base no valida el rol: eso lo controla la aplicación.
-- Sin ON DELETE en cascada a propósito: las bajas de usuarios
-- son lógicas (Activo = 0), nunca se borra la fila.
-- ============================================
IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_Socios_Entrenador'
)
    ALTER TABLE Socios
    ADD CONSTRAINT FK_Socios_Entrenador
        FOREIGN KEY (EntrenadorId)
        REFERENCES Usuarios(Id);
GO

-- ============================================
-- PASO 3: Índice por EntrenadorId
-- "Mis Alumnos" filtra por entrenador; sin índice,
-- esa consulta recorre toda la tabla de socios.
-- ============================================
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Socios_EntrenadorId'
      AND object_id = OBJECT_ID('Socios')
)
    CREATE INDEX IX_Socios_EntrenadorId
        ON Socios(EntrenadorId);
GO

-- ============================================
-- PASO 4: Verificar la estructura de Socios
-- (consulta opcional para comprobar las columnas)
-- ============================================
-- SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
-- FROM INFORMATION_SCHEMA.COLUMNS
-- WHERE TABLE_NAME = 'Socios'
-- ORDER BY ORDINAL_POSITION;
GO
