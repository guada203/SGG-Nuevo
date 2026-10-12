# Segunda entrega — Plan de integración con base de datos

> Plan de trabajo verificado contra el código (fecha: sept 2026). Objetivo: que las tres pantallas (Administrador, Recepcionista y Entrenador) lean y escriban en la base de datos real, sin datos demo.

---

## 1. Estado actual (verificado)

| Zona | Estado | Fuente |
|---|---|---|
| Login (Usuarios/Roles) | ✅ Conectado a BD | `UsuarioRepositorio` + `ServicioAutenticacion` |
| Admin — Panel de inicio (KPIs) | ✅ Conectado a BD | `ServicioDashboard` |
| Admin — Gestión de Socios | ✅ Conectado a BD | `ServicioSocios` |
| Admin — Gestión de Usuarios + AltaUsuario | ✅ Conectado a BD | `ServicioUsuarios` |
| Admin — Gestión de Membresías + AltaMembresia | ✅ Conectado a BD | `ServicioMembresias` |
| Admin — Reportes | ❌ Demo | `DatosRecepDemo` |
| Recepcionista — todas las pantallas | ❌ Demo | `DatosRecepDemo` |
| Entrenador — todas las ventanas | ❌ Demo | listas en memoria (`CargarDatosDePrueba`) |

**Ya existen** (no se vuelven a crear):

- Repositorios: `SocioRepositorio`, `PagoRepositorio`, `MembresiaRepositorio`, `UsuarioRepositorio`, `RolRepositorio` (`SGG.Datos/Repositorios`).
- Servicios: `ServicioUsuarios`, `ServicioSocios`, `ServicioReportes`, `ServicioMembresias`, `ServicioDashboard`, `ServicioAutenticacion` (`SGG.Logica/Servicios`).
- Entidades de dominio: `Socio`, `Membresia`, `Pago`, `Asistencia`, `Rutina`, `Ejercicio`, `Usuario`, `Rol` (`SGG.Dominio/Entidades`).
- Tablas en BD: `Membresias`, `Usuarios`, `Socios`, `Pagos`, `Asistencias`, `Rutinas` (script `SGG.Datos/Scripts/01_EsquemaBase.sql`).

---

## 2. Recepcionista — conectar a la BD

- [ ] **Panel de inicio** (`PanelInicioRecepcionista.xaml.cs`): reemplazar `DatosRecepDemo` por `ServicioDashboard` (mismo patrón que ya usa el Admin). KPIs + gráfico de ingresos reales.
- [ ] **Gestión de Socios** (`GestionSocios.xaml.cs`): usar `ServicioSocios` (existe pero esta pantalla no lo usa). Grilla, edición y baja contra BD.
- [ ] **AltaSocio** (`AltaSocio.xaml.cs`): usar `ServicioSocios.AltaSocio()` — ya valida nombre, apellido, DNI duplicado y membresía vigente.
- [x] **RegistrarPago** (`RegistrarPago.xaml.cs`): registro real de pagos implementado — `ServicioPagos` + `PagoRepositorio.Agregar` + comprobante imprimible.
  - [ ] Renovación de membresía: al registrar el pago, extender `Membresia.FechaVencimiento` (todavía pendiente).
- [ ] **ControlAsistencia** (`ControlAsistencia.xaml.cs`): crear `AsistenciaRepositorio` — registrar ingreso en `Asistencias`, historial y estado de cuota real (vencida / al día) según `FechaVencimiento`.

---

## 3. Admin — lo que falta

- [ ] **Reportes** (`Reportes.xaml.cs`): reemplazar `DatosRecepDemo` por `ServicioReportes` (ya tiene `ObtenerPagosPorMes`, `ObtenerAsistenciasPorRango`, `ObtenerTodosLosSocios`).
  - El botón **"Ver detalle"** (asistencias) NO cambia: es un filtro por socio sobre el mismo resultado. No requiere tabla nueva.
- [ ] **Panel de inicio** (`PanelInicioAdmin.xaml.cs`): quitar la simulación de barras cuando no hay pagos (líneas ~60-67) y la "actividad reciente" hardcodeada — reemplazar por datos reales o derivados.

---

## 4. Entrenador — el perfil con más trabajo

- [ ] Crear `RutinaRepositorio` + `ServicioRutinas` (no existen).
- [ ] **VentanaListaRutinas** (`VentanaListaRutinas.xaml.cs`): hoy carga `CargarDatosDePrueba()` con listas inventadas → listar rutinas reales de la BD.
- [ ] **VentanaGestionRutinas**: crear / editar rutina → guardar en `Rutinas` (+ `Ejercicios`).
- [ ] **VentanaElegirRutina**: elegir rutina de la BD para asignar a un socio.
- [ ] **VentanaMisAlumnos** (`VentanaMisAlumnos.xaml.cs`): socios activos asignados al entrenador logueado (depende de `Socios.EntrenadorId`, ver sección 5).
- [ ] **PanelInicioEntrenador** (`PanelInicioEntrenador.xaml.cs`): `CargarDatosDePrueba()` → datos reales (rutinas creadas, activas, socios sin rutina).

---

## 5. Base de datos — esquema y decisiones pendientes

- [ ] **`Socios.EntrenadorId`** (FK → `Usuarios.Id`): sin esto no existen "Mis Alumnos" ni la asignación socio→entrenador en BD. Es migración.
- [ ] **Historial de membresías (decisión)**:
  - Opción A (recomendada): `Membresias` pasa a ser **1:N** con `SocioId` — cada renovación es una fila. Hace reales "cuotas por vencer" e "ingresos del mes".
  - Opción B: mantener 1:1 (como está) y registrar solo los pagos. Menos exacto para historial.
- [ ] **Tabla `Rutinas`**: faltan columnas que la UI ya muestra — `Estado`, `DuracionSemanas`, `FrecuenciaSemanal`, `Nivel`, `Objetivo`. Verificar también `Ejercicio`. Es migración.
- [ ] **`Socios.FechaAlta`** (opcional pero recomendado): hoy "nuevos socios del mes" usa como aproximación la fecha de inicio de la membresía. Con `FechaAlta` propia el dato es exacto.
- [ ] **Actividad reciente de dashboards**: NO crear tabla nueva; derivarla de `Pagos` + `Asistencias` recientes con consultas.

---

## 6. Orden de trabajo sugerido

1. Migraciones de esquema (EntrenadorId, historial de membresías, columnas de Rutinas).
2. Repositorios faltantes: `AsistenciaRepositorio`, `RutinaRepositorio`, `EjercicioRepositorio`.
3. Servicios faltantes: `ServicioPagos`, `ServicioAsistencias`, `ServicioRutinas`.
4. Conectar Recepcionista (panel, socios, alta, pagos, asistencia).
5. Conectar Reportes de Admin + limpiar simulaciones del dashboard.
6. Conectar Entrenador (rutinas, ejercicios, mis alumnos, panel).

> Nota: al conectar, los nombres que hoy se ven (Ana García, Martín López, etc.) serán reemplazados por los datos reales del script `SGG.Datos/Scripts/02_DatosPrueba.sql`.

---

## 7. Setup para otra máquina (cadena de conexión)

Pasos para dejar el sistema funcionando en una máquina distinta a la de desarrollo original. No hace falta tocar código: la cadena de conexión se resuelve por archivos de configuración.

1. **Traer el código actualizado.** Clonar el repositorio o, si ya lo tiene, actualizarse a la rama de trabajo:
   ```
   git fetch origin
   git checkout feature/gest-usuarios
   git pull
   ```

2. **Instalar SQL Server Express + SSMS.** Cualquier instancia sirve (por defecto `.\SQLEXPRESS`). Instalar SQL Server Management Studio para ejecutar los scripts.

3. **Crear la base `SGG`.** El `CREATE DATABASE SGG;` está comentado al inicio de `SGG.Datos/Scripts/01_EsquemaBase.sql`. Descomentarlo (junto con el `GO` siguiente) o crear la base a mano desde SSMS.

4. **Ejecutar los scripts en orden** desde SSMS, conectado a la instancia local:
   - `SGG.Datos/Scripts/01_EsquemaBase.sql` — esquema de tablas.
   - `SGG.Datos/Scripts/02_DatosPrueba.sql` — datos de prueba (usuarios, socios, membresías, pagos).
   - `SGG.Datos/Scripts/03_MigrarNormalizarUsuarioRol.sql` — **obligatorio**: sin él el login falla porque EF espera `Usuarios.RolId` y la tabla `Roles`.

5. **Configurar la conexión sin tocar código.** Crear el archivo `SGG.Datos\appsettings.local.json` con su instancia local, por ejemplo:
   ```json
   {
     "ConnectionStrings": {
       "SGG": "Server=.\\SQLEXPRESS;Database=SGG;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=30;"
     }
   }
   ```
   Ese archivo está en `.gitignore`: es de cada máquina y nunca genera conflictos en git.

6. **Compilar y ejecutar.** Login con `admin@sgg.com` / `Admin123` (o cualquier otro usuario de los seed del script 02).

**Precedencia de la configuración:** `appsettings.json` es la plantilla commiteada (instancia `.\SQLEXPRESS`); `appsettings.local.json` la pisa si existe. Si no hay ningún archivo, la aplicación cae a `.\SQLEXPRESS` por defecto.

> Advertencia: los datos del script `02_DatosPrueba.sql` son de agosto 2026, así que las membresías y cuotas van a verse vencidas y "ingresos del mes" puede dar cero. Es el comportamiento esperado, no un bug.