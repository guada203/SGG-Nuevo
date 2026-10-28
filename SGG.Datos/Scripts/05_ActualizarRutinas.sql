
-- ============================================
-- SGG - Migración: Actualizar la estructura de Rutinas
-- (Ejecutar sobre la base SGG existente)
-- Se agregan los campos necesarios para crear, editar
-- y asignar rutinas desde el módulo Entrenador.
-- No se eliminan columnas ni datos existentes.
-- ============================================
USE SGG;
GO

-- ============================================
-- PASO 1: Agregar Objetivo a Rutinas
-- ============================================
IF COL_LENGTH('Rutinas', 'Objetivo') IS NULL
    ALTER TABLE Rutinas
    ADD Objetivo NVARCHAR(500) NOT NULL
        CONSTRAINT DF_Rutinas_Objetivo DEFAULT ('');
GO

-- ============================================
-- PASO 2: Agregar DuracionSemanas
-- (valor inicial 4 para las rutinas existentes)
-- ============================================
IF COL_LENGTH('Rutinas', 'DuracionSemanas') IS NULL
    ALTER TABLE Rutinas
    ADD DuracionSemanas INT NOT NULL
        CONSTRAINT DF_Rutinas_DuracionSemanas DEFAULT (4);
GO

-- ============================================
-- PASO 3: Agregar FrecuenciaSemanal
-- (valor inicial 3 para las rutinas existentes)
-- ============================================
IF COL_LENGTH('Rutinas', 'FrecuenciaSemanal') IS NULL
    ALTER TABLE Rutinas
    ADD FrecuenciaSemanal INT NOT NULL
        CONSTRAINT DF_Rutinas_FrecuenciaSemanal DEFAULT (3);
GO

-- ============================================
-- PASO 4: Agregar Nivel
-- (Principiante, Intermedio o Avanzado)
-- ============================================
IF COL_LENGTH('Rutinas', 'Nivel') IS NULL
    ALTER TABLE Rutinas
    ADD Nivel NVARCHAR(30) NOT NULL
        CONSTRAINT DF_Rutinas_Nivel DEFAULT ('Principiante');
GO

-- ============================================
-- PASO 5: Agregar Estado
-- (Activa o Completada)
-- ============================================
IF COL_LENGTH('Rutinas', 'Estado') IS NULL
    ALTER TABLE Rutinas
    ADD Estado NVARCHAR(30) NOT NULL
        CONSTRAINT DF_Rutinas_Estado DEFAULT ('Activa');
GO

-- ============================================
-- PASO 6: Agregar Notas
-- (observaciones adicionales del entrenador)
-- ============================================
IF COL_LENGTH('Rutinas', 'Notas') IS NULL
    ALTER TABLE Rutinas
    ADD Notas NVARCHAR(MAX) NOT NULL
        CONSTRAINT DF_Rutinas_Notas DEFAULT ('');
GO

-- ============================================
-- PASO 7: Verificar la estructura de Rutinas
-- (consulta opcional para comprobar las columnas)
-- ============================================
-- SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
-- FROM INFORMATION_SCHEMA.COLUMNS
-- WHERE TABLE_NAME = 'Rutinas'
-- ORDER BY ORDINAL_POSITION;
GO
