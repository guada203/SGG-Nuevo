# Autenticacion Specification

## Purpose

Define the authentication flow that validates user credentials by translating the expected role string into a RolId via lookup in the Roles table. This is the single change in the login flow; the UI (VentanaSeleccionRol, VentanaLogin) continues operating with role strings unchanged.

## Requirements

### Requirement: ValidarCredenciales traduce string rolEsperado a RolId

The system MUST translate the expected role string (rolEsperado) into a RolId by performing a lookup in the Roles table by name. The validated user's RolId is then compared against this resolved value. This is the only change in the authentication flow; the rest of the credential validation (password, active status) remains unchanged.

#### Scenario: ValidarCredenciales lookupRole por nombre y compara RolId

- GIVEN que el usuario ingresa credenciales válidas y el rol esperado es "Administrador"
- WHEN ValidarCredenciales busca el Role por nombre "Administrador" en la tabla Roles
- THEN el sistema obtiene RolId=1 y compara contra usuario.RolId
- AND si usuario.RolId equals 1, la validación exitos continúa

### Requirement: La UI de login no cambia su flujo de string de rol

The system MUST NOT modify VentanaSeleccionRol or VentanaLogin to use RolId. These UI components continue to operate with the role name as a string, and this decision is explicitly out of scope for this change.

#### Scenario: VentanaLogin mantiene string de rol en su flujo

- GIVEN que la VentanaLogin presenta el campo de rol como string seleccionable
- WHEN el usuario selecciona un rol y ingresa credenciales
- THEN el flujo de login continúa usando el string del nombre del rol, sin traducir a RolId en la UI

### Requirement: Si el Role no existe en la tabla, la validación falla

The system MUST fail credential validation if the rolEsperado string does not correspond to any Role in the database. This prevents authentication with undefined roles.

#### Scenario: Validación falla cuando el rol esperado no existe

- GIVEN que el usuario intenta login con rol esperado "RolInexistente" que no está en la tabla Roles
- WHEN ValidarCredenciales busca por nombre "RolInexistente"
- THEN el sistema devuelve error de validación y no permite el acceso

## ADDED Requirements (New behavior for this change)

### Requirement: Lookup de Rol por nombre en ValidarCredenciales

The system MUST implement a lookup operation in ValidarCredenciales that queries the Roles table by Nombre to find the matching Id. This lookup is a new step in the authentication pipeline, added specifically for this normalization change.

#### Scenario: Lookup retorna RolId correcto para role existente

- GIVEN que la tabla Roles tiene registro con Nombre="Recepcionista" e Id=2
- WHEN ValidarCredenciales ejecuta lookup por nombre "Recepcionista"
- THEN el resultado es RolId=2 y la comparación contra usuario.RolId procede normalmente

## REMOVED Requirements (Old behavior deprecated)

### Requirement: Flujo de login completo con role como string sin lookup

(Previously: El flujo de login validaba el role simplemente comparando el string ingresado contra el string almacenado en usuario.Rol). This requirement is removed as the login flow now performs a lookup in the Roles table to translate the string to RolId before comparison.