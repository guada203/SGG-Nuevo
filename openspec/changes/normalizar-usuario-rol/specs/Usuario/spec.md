# Usuario Specification

## Purpose

Define the normalized User entity with separated Role dependency, new contact and identification fields, and uniqueness constraints. The system must support both new user creation with full data migration and backward compatibility with existing users during transition.

## Requirements

### Requirement: Campo RolId en entidad Usuario

The system MUST store the user's Role reference via RolId foreign key instead of a plain string Role name. RolId references the Id primary key of the Roles table. This enables proper relational integrity and role-based authorization lookups.

#### Scenario: Usuario nuevo se crea con RolId válido

- GIVEN un rol existente en la tabla Roles con Id=1 (Administrador)
- WHEN se crea un nuevo usuario con RolId=1 y datos completos
- THEN el usuario se persiste con RolId=1 y la FK es válida en la base de datos

### Requirement: Campos adicionales en entidad Usuario

The system MUST include the following optional fields in the User entity: Apellido (string), Direccion (string), Telefono (string), Dni (string, nullable). These fields extend the user profile without affecting authentication logic.

#### Scenario: Usuario nuevo incluye campos opcionales

- GIVEN un usuario nuevo con RolId=2 (Recepcionista), Apellido="Gonzalez", Direccion="Calle Falsa 123", Telefono="1234567", Dni=NULL
- WHEN el usuario se guarda en la base de datos
- THEN todos los campos opcionales se persisten correctamente y son recuperables

### Requirement: Validación de DNI obligatorio en AltaUsuario

The system MUST require the DNI field when creating a new user via AltaUsuario. The DNI field is mandatory for new user registration to ensure unique identification.

#### Scenario: AltaUsuario rechaza creación sin DNI

- GIVEN que el usuario intenta dar de alta un nuevo usuario sin proporcionar DNI
- WHEN se invoca AltaUsuario sin campo Dni
- THEN el sistema devuelve error de validación y no crea el usuario

#### Scenario: AltaUsuario acepta creación con DNI único

- GIVEN que el usuario proporciona DNI único no existente en la base de datos
- WHEN se invoca AltaUsuario con DNI válido y todos los campos requeridos
- THEN el usuario se crea exitosamente y el DNI se almacena con constraint UNIQUE

### Requirement: Constraint UNIQUE en Email

The system MUST enforce uniqueness on the Email field. Email already has a unique constraint from the base schema (01_EsquemaBase.sql línea 26). This constraint prevents duplicate user registrations.

#### Scenario: Registro rechaza email duplicado

- GIVEN que un usuario intenta registrarse con un email ya existente en la base de datos
- WHEN AltaUsuario intenta crear el usuario con ese email
- THEN el sistema devuelve error de validación de email duplicado y no crea el usuario

### Requirement: Constraint UNIQUE en Dni

The system MUST enforce uniqueness on the Dni field. The Dni unique constraint applies directly on the column; NULL values do not conflict (SQL Server behavior), allowing existing users without DNI to remain valid.

#### Scenario: AltaUsuario rechaza DNI duplicado

- GIVEN que un usuario intenta registrarse con un DNI ya existente en la base de datos (entre usuarios que tienen DNI cargado)
- WHEN AltaUsuario intenta crear el usuario con ese DNI
- THEN el sistema devuelve error de validación de DNI duplicado y no crea el usuario

#### Scenario: Usuarios existentes con DNI=NULL no rompen constraint UNIQUE

- GIVEN que hay usuarios en la base de datos con Dni=NULL (carga migratoria antigua)
- WHEN se aplica constraint UNIQUE sobre Dni
- THEN el constraint se crea exitosamente sin error, ya que los NULLs no colisionan en SQL Server

### Requirement: Migración de Rol string a RolId

The system MUST migrate existing users from Rol string (NVARCHAR(20)) to RolId (int FK) via SQL UPDATE with CASE mapping. Each existing role name maps to the corresponding Role Id after the Roles table is populated.

#### Scenario: Migración completa traduce los 3 roles fijos

- GIVEN que la tabla Roles tiene 3 registros: Administrador (Id=1), Recepcionista (Id=2), Entrenador (Id=3)
- WHEN se ejecuta el script de migración con UPDATE JOIN CASE
- THEN todos los usuarios existentes quedan con RolId correspondiente: admin@sgg.com -> 1, recep@sgg.com -> 2, entrenador@sgg.com -> 3

### Requirement: Campo Dni pasa a NOT NULL después de migración

After the data migration is complete, the system MUST alter the Dni column to NOT NULL. This ensures data integrity for future user creation, while existing users with NULL DNI are expected to be updated by administration.

#### Scenario: Migración pone Dni NOT NULL después de cargar datos

- GIVEN que todos los usuarios tienen Dni cargado (ningún NULL restante)
- WHEN se ejecuta ALTER TABLE para poner Dni NOT NULL
- THEN el alter succeeds y la integridad de datos queda garantizada para nuevos registros

## ADDED Requirements (New behavior for this change)

### Requirement: Campo Dni obligatorio en formulario AltaUsuario

The system MUST present the DNI field as mandatory in the AltaUsuario form. The UI must prevent form submission if DNI is empty or invalid format.

#### Scenario: Formulario AltaUsuario impide envío sin DNI

- GIVEN que el usuario completa el formulario AltaUsuario sin llenar el campo DNI
- WHEN intenta enviar el formulario
- THEN el sistema bloquea el envío y muestra mensaje de requerimiento de DNI

### Requirement: ComboBox cmbRol muestra nombre pero guarda RolId

The system MUST use ComboBox cmbRol con SelectedValuePath="Id" y DisplayMemberPath="Nombre" para que la UI muestre el nombre del rol pero guarde el RolId en la entidad usuario.

#### Scenario: ComboBox selecciona rol por nombre y guarda su Id

- GIVEN que cmbRol tiene ItemsSource con Roles (Id=1, Nombre="Administrador")
- WHEN el usuario selecciona "Administrador" del ComboBox
- THEN el sistema guarda RolId=1 en la entidad usuario, no el string "Administrador"

## REMOVED Requirements (Old behavior deprecated)

### Requirement: Campo Rol como string en entidad Usuario

(Previously: La entidad Usuario almacenaba el rol como string NVARCHAR(20) NOT NULL). This requirement is removed as the role is now represented via foreign key RolId. References to usuario.Rol as string should be updated to usuario.RolId.

## RENAMED Requirements (Old name to new name)

### Requirement: Rol (string) → RolId (int FK)

(Reason: Normalizar entidad Role a tabla independiente con FK para integridad relacional y extensibilidad. Migración: actualizar todos los referencias de usuario.Rol a usuario.RolId y ajustar validaciones en las capas de lógica y presentación.)