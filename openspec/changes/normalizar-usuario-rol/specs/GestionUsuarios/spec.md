# GestionUsuarios Specification

## Purpose

Define the user management workflow for administration interfaces. This includes the AltaUsuario form with separated name/apellido fields, mandatory DNI validation, ComboBox role selection, and grid conteos that use RolId instead of string role values. The system must support both new user creation and existing user listing/editing with the normalized schema.

## Requirements

### Requirement: Formulario AltaUsuario separa Nombre y Apellido

The system MUST present separate input fields for Nombre and Apellido in the AltaUsuario form. These fields are independent and both are captured when creating a new user.

#### Scenario: Formulario captura Nombre y Apellido por separado

- GIVEN que el usuario completa Nombre="Juan" y Apellido="Gomez" en el formulario AltaUsuario
- WHEN el usuario envía el formulario
- THEN el sistema persiste Nombre="Juan" y Apellido="Gomez" en la entidad usuario respectiva

### Requirement: Campos Direccion, Telefono y Dni en formulario AltaUsuario

The system MUST include input fields for Direccion, Telefono, and Dni in the AltaUsuario form. The Dni field is mandatory; Direccion and Telefono are optional.

#### Scenario: Formulario AltaUsuario requiere DNI para enviar

- GIVEN que el usuario completa todos los campos menos DNI y tenta enviar
- WHEN el sistema valida el envío
- THEN el sistema bloquea el envío y muestra error "DNI es obligatorio"

#### Scenario: Formulario AltaUsuario acepta Direccion y Telefono opcionales

- GIVEN que el usuario proporciona Direccion="Calle Principal" y Telefono="9876543" pero omite Dni
- WHEN intenta enviar (con Dni vacío)
- THEN el sistema bloquea el envío por DNI requerido

### Requirement: ComboBox cmbRol muestra Nombre pero guarda RolId

The system MUST use ComboBox cmbRol con SelectedValuePath="Id" y DisplayMemberPath="Nombre". Esto permite que la UI muestre el nombre legible del rol al usuario, pero el valor guardado en la entidad sea el RolId numérico.

#### Scenario: ComboBox selecciona por nombre y guarda Id numérico

- GIVEN que cmbRol ItemsSource contiene roles: (Id=1, Nombre="Administrador"), (Id=2, Nombre="Recepcionista")
- WHEN el usuario selecciona "Recepcionista" del desplegable
- THEN la entidad usuario se guarda con RolId=2

### Requirement: Grid GestionUsuarios muestra conteos por rol usando RolId

The system MUST display conteos (líneas 83-85) en la interfaz GestionUsuarios usando RolId en lugar de string Role. Las líneas 68 y 151 del código ya operan con RolId/objeto Rol en lugar del antiguo string.

#### Scenario: Filtro por rol en grid usa RolId para conteo

- GIVEN que el usuario filtra la lista de usuarios por rol "Administrador" en GestionUsuarios
- WHEN el sistema consulta usuarios con RolId=1
- THEN el grid muestra el conteo correcto de usuarios administrador y las líneas 68 y 151 reflejan el RolId

### Requirement: Validación de DNI único en AltaUsuario

The system MUST validate that the DNI provided in AltaUsuario is unique among existing users (excluding the current user being edited, if any). This complements the DB constraint and provides immediate feedback.

#### Scenario: AltaUsuario rechaza DNI de usuario existente

- GIVEN que el usuario intenta dar de alta un nuevo usuario con un DNI que ya pertenece a otro usuario existente
- WHEN se invoca la validación de DNI único
- THEN el sistema devuelve error de DNI duplicado y no crea el usuario

#### Scenario: AltaUsuario acepta DNI de usuario que se está editando

- GIVEN que se edita un usuario existente y se proporciona su DNI original sin cambios
- WHEN se valida la unicidad de DNI
- THEN el sistema permite el guardado (el DNI es el mismo, no es duplicado)

## ADDED Requirements (New behavior for this change)

### Requirement: Campo Dni es obligatorio en formulario AltaUsuario a nivel de UI

The system MUST enforce DNI as a required field at the UI level in AltaUsuario, preventing form submission until a valid DNI is entered.

#### Scenario: Formulario impide envío sin DNI válido

- GIVEN que el campo DNI está vacío o tiene formato inválido
- WHEN el usuario hace clic en "Aceptar"
- THEN el sistema muestra mensaje de requerimiento y no envía el formulario

### Requirement: Conteos de usuarios por rol en GestionUsuarios usan RolId

The system MUST report user counts per role using the RolId foreign key. This replaces the old string-based role counting and aligns with the normalized schema.

#### Scenario: Grid GestionUsuarios muestra conteo correcto por RolId

- GIVEN que hay 3 usuarios con RolId=1 (Administrador), 2 con RolId=2 (Recepcionista), 1 con RolId=3 (Entrenador)
- WHEN el usuario consulta el grid de GestionUsuarios
- THEN los conteos muestran: Administrador: 3, Recepcionista: 2, Entrenador: 1

## REMOVED Requirements (Old behavior deprecated)

### Requirement: Conteos de rol usando string Role en entidad Usuario

(Previously: Las líneas 83-85 y 68/151 del código de GestionUsuarios usaban Rol como string NVARCHAR(20)). This requirement is removed as the counts now use RolId foreign key reference. Código legacy que refería usuario.Rol como string debe actualizarse a usuario.RolId.