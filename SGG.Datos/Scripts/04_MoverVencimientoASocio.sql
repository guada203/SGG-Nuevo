-- ============================================
-- SGG - Migración: Mover el vencimiento de la cuota al Socio
-- (Ejecutar UNA SOLA VEZ sobre la base SGG existente)
-- La membresía pasa a ser el catálogo de planes; cada socio lleva
-- sus propias fechas FechaAlta y FechaVencimiento.
-- ============================================
USE SGG;
GO

-- ============================================
-- PASO 1: Agregar FechaAlta y FechaVencimiento a Socios
-- (nullable: los socios existentes se completan en el paso 2)
-- ============================================
IF COL_LENGTH('Socios', 'FechaAlta') IS NULL
    ALTER TABLE Socios ADD FechaAlta DATE NULL;
GO

IF COL_LENGTH('Socios', 'FechaVencimiento') IS NULL
    ALTER TABLE Socios ADD FechaVencimiento DATE NULL;
GO

-- ============================================
-- PASO 2: Migrar los datos actuales desde Membresias
-- (solo completa los socios que todavía no tienen fecha, así el
--  script puede re-ejecutarse sin pisar las renovaciones hechas)
-- ============================================
UPDATE s
SET s.FechaAlta = m.FechaInicio,
    s.FechaVencimiento = m.FechaVencimiento
FROM Socios s
INNER JOIN Membresias m ON s.MembresiaId = m.Id
WHERE s.FechaVencimiento IS NULL;
GO

-- Verificación final (opcional):
-- SELECT Id, FechaAlta, FechaVencimiento FROM Socios;
GO
