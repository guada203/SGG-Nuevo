# Proposal: Normalizar usuario y rol

## Intent

Sistema-normalizar la entidad Usuario para separar el campo `Rol` (string) en una entidad `Roles` independiente con Foreign Key `RolId`, adicionar campos nuevos `Apellido`, `Direccion`, `Telefono`, `Dni` a la tabla `Usuarios`, y asegurar constraints UNIQUE en `Email` y `Dni`. Este cambio permite una gestión de roles más robusta, evita hardcoding de strings y sienta las bases para futuras asignaciones de permisos. Los scripts son SQL puros (sin EF Core Migrations) para crear la tabla Roles, migrar datos existentes y aplicar los cambios de esquema.

## Scope

### In Scope

- Crear tabla `Roles` (Id PK int identity, Nombre string único) e insertar roles fijos: Administrador, Recepcionista, Entrenador.
- Adicionar 4 columnas nuevas a tabla `Usuarios`: `Apellido`, `Direccion`, `Telefono`, `Dni` (todas opcionales en migración, NOT NULL en nuevo diseño).
- Migrar usuarios existentes: traducir el `Rol` string actual a `RolId` correspondiente mediante UPDATE con JOIN / CASE.
- Eliminar columna vieja `Rol` (string) de la tabla `Usuarios`.
- Constraint UNIQUE en `Dni` y confirmar que `Email` ya lo tiene.
- Normalizar todo el código C# que usa `Rol` como string a usar `RolId` FK.
- Actualizar formularios: `AltaUsuario`, `VentanaLogin`, `VentanaSeleccionRol`, `GestionUsuarios`, `MenuLateral`, `Reportes`, `VentanaPrincipalAdmin`.
- Decisión de diseño en flujo de login: el rol elegido en `VentanaSeleccionRol` (string nombre) se resuelve a `RolId` antes de pasar a `ValidarCredenciales`.

### Out of Scope

- Cambios a entidades `Socio`, `Pago`, `Membresia`, `Asistencia`, `Rutina` (no tocar salvo que una FK lo requiera).
- Introducir EF Core Migrations — el proyecto usa scripts SQL manuales.
- Cambiar patrón de contraseñas o hashing.
- Modificar entidad `Socio` o su estructura.

## Approach

1. **Script SQL de migración**:
   a. Crear tabla `Roles` e insertar los 3 roles fijos.
   b. `ALTER TABLE Usuarios ADD Apellido NVARCHAR(100)`, `Direccion NVARCHAR(200)`, `Telefono NVARCHAR(20)`, `Dni NVARCHAR(10)`.
   c. UPDATE migrando `Rol` string a `RolId` con CASE: cuándo Rol = 'Administrador' → RolId = 1, etc.
   d. `ALTER TABLE Usuarios ADD RolId INT FOREIGN KEY REFERENCES Roles(Id)`.
   e. `UPDATE Usuarios SET Dni = ...` (limpiar/destacar DNI si existe).
   f. `ALTER TABLE Usuarios DROP COLUMN Rol`.
   g. `CREATE UNIQUE INDEX UQ_Usuarios_Dni ON Usuarios(Dni)` y confirmar `UQ_Usuarios_Email` ya existe.

2. **Normalización C#**:
   - `SGG.Dominio/Entidades/Usuario.cs`: `Rol string` → `RolId int` FK + nuevos campos (`Apellido`, `Direccion`, `Telefono`, `Dni`).
   - `SGG.Datos/Contexto/SggDbContext.cs`: `DbSet<Roles>`, configuración de FK, índices únicos en `Email` y `Dni`.
   - `SGG.Datos/Repositorios/UsuarioRepositorio.cs`: incluir carga del `Rol` navigation property; actualizar `ObtenerPorEmail`, `ExisteEmail`.
   - `SGG.Logica/Servicios/ServicioUsuarios.cs`: `AltaUsuario` firma cambia a `(nombre, apellido, direccion, telefono, dni, email, password, rolId)`. Quitar `Rol = rol`. Agregar chequeo DNI único.
   - `SGG.Logica/Servicios/ServicioAutenticacion.cs`: `ValidarCredenciales` compara `usuario.RolId` contra `rolEsperado` (int) en vez de `usuario.Rol != rolEsperado` (string).
   - `SGG/Formularios/Login/VentanaLogin.xaml.cs`: usar `_rolSeleccionado` (int/RolId) y pasarlo a `ValidarCredenciales`; decisión explícita: el string del combo se mapea a RolId antes de la validación.
   - `SGG/Formularios/Login/VentanaSeleccionRol.xaml(.cs)`: pantalla donde el usuario elige rol por nombre; el result se convierte a RolId antes de abrir login.
   - `SGG/Formularios/Admin/AltaUsuario.xaml(.cs)`: separar `Nombre` y `Apellido` en 2 campos, agregar `Direccion`, `Telefono`, `Dni`. `ComboBox cmbRol` muestra texto "Administrador/Recepcionista/Entrenador" pero guarda `RolId` por detrás (items tipados con Id+Nombre).
   - `SGG/Formularios/Admin/GestionUsuarios.xaml.cs`: líneas que comparan `u.Rol == "Administrador"`/etc (líneas 83-85), línea 68 (`Rol = u.Rol`), línea 151 (propiedad `Rol string`). Usar `RolId` u objeto `Rol`.
   - `SGG/Controles/MenuLateral.xaml.cs`: `ConfigurarRol(string rol)` — evaluar si recibe string o `RolId`.
   - `SGG/Formularios/Admin/Reportes.xaml.cs`: hardcodea `ConfigurarRol("Administrador")` y `VentanaSeleccionRol()`.
   - `SGG/Formularios/Admin/VentanaPrincipalAdmin.xaml.cs`: hardcodea `ConfigurarRol("Administrador")` y `VentanaSeleccionRol()`.

3. **Flujo de login resolviendo RolId**:
   - `VentanaSeleccionRol` ofrece 3 botones por nombre de rol ("Administrador", "Recepcionista", "Entrenador").
   - Al hacer clic, se convierte el nombre string al `RolId` correspondiente (1, 2, 3) consultando la tabla Roles o mapeo fijo.
   - Ese `RolId` se pasa a `VentanaLogin` como parámetro `rolSeleccionado` (ahora int).
   - `ValidarCredenciales` recibe `rolEsperado` como int y compara `usuario.RolId == rolEsperado`.
   - Esto rompe la comparación de string con string y centraliza la resolución de rol en la pantalla de selección.

## Affected Areas

| Area | Impact | Description |
|------|--------|-------------|
| `SGG.Dominio/Entidades/Usuario.cs` | Modified | `Rol string` → `RolId int` FK + `Apellido`, `Direccion`, `Telefono`, `Dni` columns |
| `SGG.Datos/Contexto/SggDbContext.cs` | Modified | Add `DbSet<Roles>`, FK configuration, unique indexes on `Email` and `Dni` |
| `SGG.Datos/Repositorios/UsuarioRepositorio.cs` | Modified | Include `Rol` navigation; update query methods |
| `SGG.Logica/Servicios/ServicioUsuarios.cs` | Modified | `AltaUsuario` new signature; DNI uniqueness check |
| `SGG.Logica/Servicios/ServicioAutenticacion.cs` | Modified | Compare `RolId` instead of `Rol` string |
| `SGG/Formularios/Login/VentanaLogin.xaml.cs` | Modified | Pass `RolId` (int) to `ValidarCredenciales` |
| `SGG/Formularios/Login/VentanaSeleccionRol.xaml(.cs)` | Modified | Resolve selected role name → `RolId` before opening login |
| `SGG/Formularios/Admin/AltaUsuario.xaml(.cs)` | Modified | Separate Nombre/Apellido; add Direccion, Telefono, Dni; ComboBox with typed items saving `RolId` |
| `SGG/Formularios/Admin/GestionUsuarios.xaml.cs` | Modified | Replace `u.Rol == "Admin"` with `u.RolId == X`; fix `Rol = u.Rol` |
| `SGG/Controles/MenuLateral.xaml.cs` | Modified | `ConfigurarRol` evaluate `RolId` or normalized string |
| `SGG/Formularios/Admin/Reportes.xaml.cs` | Modified | Hardcoded `ConfigurarRol("Administrador")` calls |
| `SGG/Formularios/Admin/VentanaPrincipalAdmin.xaml.cs` | Modified | Hardcoded `ConfigurarRol("Administrador")` calls |

## Risks

| Risk | Likelihood | Mitigation |
|------|------------|------------|
| Pérdida de datos en migración `Rol` string → `RolId` si hay roles desconocidos | Medium | Validar que todos los roles existentes mapean a los 3 roles fijos; agregar rol "Desconocido" como valor por defecto |
| Fallo en constraints UNIQUE si datos duplicados tienen Email/Dni | High | Ejecutar script de limpieza de duplicados antes de aplicar constraint; validar con `SELECT Email, COUNT(*) ... GROUP BY Email HAVING COUNT(*) > 1` |
| Formularios rompen por firma de método cambiada | High | Revisar cada llamada a `AltaUsuario`, `ValidarCredenciales` y actualizar firma consistente; tests unitarios después |
| ComboBox `cmbRol` muestra texto pero guarda RolId incorrecto | Medium | Usar `SelectedValuePath="Id"` + `DisplayMemberPath="Nombre"` en XAML; verificar en tiempo de compilación |

## Rollback Plan

1. Si la migración falla antes de `DROP COLUMN Rol`: mantener backup de la base de datos actual; revertir todo cambiando `DROP COLUMN` por `DROP CONSTRAINT` y manteniendo `Rol string`.
2. Si falla la inserción en tabla `Roles`: revertir `CREATE TABLE Roles` y mantener esquema anterior.
3. Si después de aplicar los scripts las aplicaciones no conectan: restaurar la base de datos desde backup y volver a compilar las aplicaciones con el esquema viejo; los cambios C# se mantienen en ramas feature y se reintegran después de verificar éxito.
4. **Rollback específico**: Ejecutar `DROP TABLE Roles; ALTER TABLE Usuarios DROP COLUMN RolId; ALTER TABLE Usuarios ADD Rol NVARCHAR(50);` para restaurar el estado anterior. Nota: los datos nuevos (`Apellido`, `Direccion`, `Telefono`, `Dni`) se perderían — este es un cambio irreversible de esquema, por lo que se recomienda tomar backup completo antes de aplicar.

## Dependencies

- Acceso a base de datos SGG en SQL Server `DESKTOP-2GR5V5M\SQLEXPRESS`.
- Scripts SQL a ejecutar en orden: primero tabla Roles, luego columnas Usuarios, luego migración de datos, luego constraint único, luego drop columna vieja.
- Compilar proyecto C# después de cada lote de cambios para validar que los tipos coinciden.

## Success Criteria

- [ ] Tabla `Roles` creada con 3 filas (Administrador, Recepcionista, Entrenador) y `Id` PK identity.
- [ ] `Usuarios` tabla tiene columnas nuevas: `Apellido`, `Direccion`, `Telefono`, `Dni`, `RolId` (FK).
- [ ] Todos los usuarios existentes migrados: `RolId` asignado correctamente según su `Rol` string viejo.
- [ ] Columna `Rol` (string) eliminada de `Usuarios`.
- [ ] Constraint UNIQUE en `Dni` creado y constraint `Email` confirmado/verificado.
- [ ] `ServicioUsuarios.AltaUsuario` firma nueva y chequea DNI único.
- [ ] `ServicioAutenticacion.ValidarCredenciales` compara `usuario.RolId` contra parámetro int.
- [ ] `VentanaLogin` recibe `RolId` (int) y lo pasa correctamente a `ValidarCredenciales`.
- [ ] `VentanaSeleccionRol` convierte nombre seleccionado a `RolId` antes de abrir login.
- [ ] `AltaUsuario` tiene campos separados Nombre/Apellido + Direccion/Tel/Dni y guarda `RolId`.
- [ ] `GestionUsuarios` usa `RolId` en lugar de comparar `u.Rol == "Administrador"` etc.
- [ ] Compilación exitiva sin errores de tipo en todos los archivos afectados.

---
*Proposal generada para change `normalizar-usuario-rol` en proyecto sgg-nuevo. Modo: engram. Tema: sdd/normalizar-usuario-rol/proposal.*