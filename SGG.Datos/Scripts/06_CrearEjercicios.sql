
-- ============================================
-- SGG - Migración: Crear la tabla Ejercicios
-- (Ejecutar sobre la base SGG existente)
-- Se crea la tabla para almacenar los ejercicios
-- que pertenecen a cada rutina de entrenamiento.
-- Cada rutina puede contener varios ejercicios.
-- ============================================
USE SGG;
GO

-- ============================================
-- PASO 1: Crear la tabla Ejercicios
-- (solo si todavía no existe)
-- ============================================
IF OBJECT_ID('dbo.Ejercicios', 'U') IS NULL
BEGIN
    CREATE TABLE Ejercicios
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RutinaId INT NOT NULL,
        Nombre NVARCHAR(200) NOT NULL,
        GrupoMuscular NVARCHAR(100) NOT NULL,
        Series INT NOT NULL,
        Repeticiones INT NOT NULL,
        DescansoSegundos INT NOT NULL,

        CONSTRAINT FK_Ejercicios_Rutinas
            FOREIGN KEY (RutinaId)
            REFERENCES Rutinas(Id)
    );
END;
GO

-- ============================================
-- PASO 2: Verificar que se creó la tabla
-- (consulta opcional)
-- ============================================
-- SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
-- FROM INFORMATION_SCHEMA.COLUMNS
-- WHERE TABLE_NAME = 'Ejercicios'
-- ORDER BY ORDINAL_POSITION;
GO
