-- ============================================
-- SGG - Datos de prueba
-- ============================================
USE SGG;
GO

-- Usuarios de prueba (1 por rol)
-- admin@sgg.com       / Admin123
-- recep@sgg.com       / Recepcion123
-- entrenador@sgg.com  / Entrenador123
INSERT INTO Usuarios (Nombre, Email, PasswordHash, Rol, Activo) VALUES
('Admin Principal', 'admin@sgg.com', '$2b$11$.LCELzyhjShdm2PVpQNqYe2hwpIqxlNsRBZoCSwZ00Iw4abOQpzrC', 'Administrador', 1),
('Recepcionista Principal', 'recep@sgg.com', '$2b$11$iRyVXscp.IhpF.EwfD1MEOk0G1TvFmKPxosNJ.0pf7vsSZHpnQafy', 'Recepcionista', 1),
('Entrenador Principal', 'entrenador@sgg.com', '$2b$11$gJBJtkIPOQENOcTVy2A0EOZ2yMV/wYjp8b8Ab42tv6NOtlcfgsnri', 'Entrenador', 1);
GO

-- Membresías de ejemplo
INSERT INTO Membresias (TipoActividad, Precio, FechaInicio, FechaVencimiento, Vigente) VALUES
('Musculacion', 18000, '2026-08-01', '2026-08-31', 1),
('Funcional', 15000, '2026-08-01', '2026-08-31', 1),
('Combinado', 25000, '2026-08-01', '2026-08-31', 1);
GO

-- Socios de ejemplo
INSERT INTO Socios (Nombre, Apellido, Dni, FechaNacimiento, Telefono, Email, Activo, MembresiaId) VALUES
('Carolina', 'Méndez', '38456123', '1990-03-15', '11-4567-8900', 'carolina.m@email.com', 1, 1),
('Tomás', 'Restrepo', '40123456', '1995-07-22', '11-9876-5432', 'tomas.r@email.com', 1, 2);
GO

-- Pagos de ejemplo
INSERT INTO Pagos (SocioId, Monto, FechaPago, MetodoPago) VALUES
(1, 18000, '2026-08-01', 'Efectivo'),
(2, 15000, '2026-08-03', 'Tarjeta');
GO

-- Asistencias de ejemplo
INSERT INTO Asistencias (SocioId, FechaHoraIngreso) VALUES
(1, '2026-08-21 08:42:00'),
(2, '2026-08-21 18:10:00');
GO