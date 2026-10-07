# Rol Specification

## Purpose

Define the Roles entity as an independent table with a name field and unique constraint. This enables proper role management, foreign key relationships from Users, and extensible permission granularity. The system must support three fixed roles initialised at setup.

## Requirements

### Requirement: Tabla Roles con Id PK y Nombre único

The system MUST create a Roles table with Id as primary key (int, identity) and Nombre as a unique string field. This structure ensures each role has a immutable identifier and prevents duplicate role names.

#### Scenario: Tabla Roles se crea con estructura correcta

- GIVEN que se ejecuta script SQL de creación de tabla Roles
- WHEN la tabla se crea con Id PK int identity y Nombre string único
- THEN la estructura de la tabla es válida y acepta inserciones

### Requirement: Inserción de 3 roles fijos Administrador, Recepcionista, Entrenador

The system MUST insert three fixed roles into the Roles table during initial setup: Administrador, Recepcionista, Entrenador. These roles provide the baseline permission structure for the application.

#### Scenario: Tres roles fijos se insertan correctamente

- GIVEN que se ejecuta script de inicialización de roles
- WHEN se insertan roles Administrador, Recepcionista, Entrenador
- THEN la tabla Roles contiene exactamente 3 filas con los nombres esperados

### Requirement: FK RolId en entidad Usuario referencia Roles.Id

The system MUST establish a foreign key relationship from Usuario.RolId to Roles.Id. This ensures referential integrity: a user's RolId must correspond to an existing Role.

#### Scenario: FK constraint impide usuario con RolId inválido

- GIVEN que existe un usuario con RolId=99 y no hay Role con Id=99
- WHEN se intenta persister el usuario o consultarlo
- THEN la base de datos lanza error de FK violation y la operación falla

### Requirement: Role name "Administrador" tiene Id=1

The system MUST assign Id=1 to the "Administrador" role. This fixed mapping is established during initial seed and remains consistent for all FK references.

#### Scenario: Role Administrador tiene Id=1 tras seed inicial

- GIVEN que el script de seed inserta Role "Administrador" como primer registro
- WHEN se consulta el Role por nombre
- THEN el Role retornado tiene Id=1

## ADDED Requirements (New behavior for this change)

### Requirement: Tabla Roles existe antes de migrar usuarios

The system MUST ensure the Roles table is created and populated before migrating users from Rol string to RolId. The migration script depends on having the Role records available for the CASE mapping.

#### Scenario: Error si se intenta migrar antes de crear Roles

- GIVEN que el script de migración intenta hacer UPDATE con JOIN a Roles antes de insertar los 3 roles fijos
- WHEN se ejecuta el script de migración
- THEN el script falla por falta de filas en Roles y la migración no procede

## REMOVED Requirements (Old behavior deprecated)

### Requirement: Rol como columna string en entidad Usuario

(Previously: La entidad Usuario tenía columna Rol NVARCHAR(20) NOT NULL). This requirement is removed as the role is now an independent entity referenced via RolId FK. The old Rol column is dropped after migration completes.