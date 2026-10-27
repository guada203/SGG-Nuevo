# Segunda entrega — Plan de integración con base de datos

> Plan de trabajo verificado contra el código (última verificación: oct 2026). Objetivo: que las tres pantallas (Administrador, Recepcionista y Entrenador) lean y escriban en la base de datos real, sin datos demo.

---

## 1. Estado actual (verificado)

| Zona | Estado | Fuente |
|---|---|---|
| Login (Usuarios/Roles) | ✅ Conectado a BD | `UsuarioRepositorio` + `ServicioAutenticacion` |
| Admin — Panel de inicio (KPIs, gráfico y actividad reciente) | ✅ Conectado a BD | `ServicioDashboard` + `ServicioPagos` + `ServicioAsistencias` |
| Admin — Gestión de Socios | ✅ Conectado a BD | `ServicioSocios` |
| Admin — Gestión de Usuarios + AltaUsuario | ✅ Conectado a BD | `ServicioUsuarios` |
| Admin — Gestión de Membresías + AltaMembresia | ✅ Conectado a BD | `ServicioMembresias` |
| Admin — Reportes (pagos, asistencias, socios + exportaciones) | ✅ Conectado a BD | `ServicioReportes` + `ServicioPagos` + `ServicioAsistencias` |
| Recepcionista — RegistrarPago (pago + renovación + comprobante) | ✅ Conectado a BD | `ServicioPagos` |
| Recepcionista — ControlAsistencia (ingreso + historial + estado de cuota) | ✅ Conectado a BD | `ServicioAsistencias` |
| Recepcionista — Panel de inicio (KPIs, gráfico de ingresos y actividad reciente) | ✅ Conectado a BD | `ServicioDashboard` + `ServicioPagos` + `ServicioAsistencias` |
| Recepcionista — Gestión de Socios (grilla, edición, baja/reactivación) | ✅ Conectado a BD | `ServicioSocios` |
| Recepcionista — AltaSocio (alta, edición y reactivación por DNI) | ✅ Conectado a BD | `ServicioSocios` + `ServicioMembresias` |
| Entrenador — todas las ventanas | ⛔ Fuera de alcance | lo desarrolla otra persona del equipo |

**Ya existen** (no se vuelven a crear):

- Repositorios: `SocioRepositorio`, `PagoRepositorio`, `AsistenciaRepositorio`, `MembresiaRepositorio`, `UsuarioRepositorio`, `RolRepositorio` (`SGG.Datos/Repositorios`).
- Servicios: `ServicioUsuarios`, `ServicioSocios`, `ServicioPagos`, `ServicioAsistencias`, `ServicioReportes`, `ServicioMembresias`, `ServicioDashboard`, `ServicioAutenticacion` (`SGG.Logica/Servicios`).
- Entidades de dominio: `Socio`, `Membresia`, `Pago`, `Asistencia`, `Rutina`, `Ejercicio`, `Usuario`, `Rol` (`SGG.Dominio/Entidades`).
- Tablas en BD: `Membresias`, `Usuarios`, `Socios`, `Pagos`, `Asistencias`, `Rutinas` (script `SGG.Datos/Scripts/01_EsquemaBase.sql`).

---

## 2. Recepcionista — conectar a la BD

- [x] **Panel de inicio** (`PanelInicioRecepcionista.xaml.cs`): conectado a `ServicioDashboard` (mismo patrón que el Admin). KPIs reales (socios activos, ingresos del mes, cuotas por vencer en los próximos 30 días) y gráfico de ingresos de los últimos 6 meses.
  - La "actividad reciente" se deriva de `Pagos` + `Asistencias` reales (los 5 eventos más recientes, con tiempo relativo). No se creó tabla nueva. Si no hay movimientos, muestra "Todavía no hay actividad registrada.".
- [x] **Gestión de Socios** (`GestionSocios.xaml.cs`): conectado a `ServicioSocios`. Grilla, edición y baja/reactivación contra la BD real.
  - Nota: la pantalla guarda en un diccionario las entidades del último listado (`_sociosPorId`) para poder abrir la edición y la baja sin volver a consultar. Un socio sin cuota pagada muestra `Sin cuota` en la columna de vencimiento.
- [x] **AltaSocio** (`AltaSocio.xaml.cs`): usa `ServicioSocios.AltaSocio()` (valida nombre, apellido, DNI duplicado y membresía vigente) y `ServicioSocios.EditarSocio()` para la edición. El combo de planes ahora sale del catálogo real (`ServicioMembresias.ObtenerVigentes()`).
  - El DNI es inalterable en la edición (RF-02) y editar los datos **no** toca el vencimiento ni la fecha de alta del socio.
  - Si el DNI ya existe pero el socio está **inactivo**, se ofrece reactivarlo en lugar de registrarlo de nuevo.
  - Un socio recién dado de alta queda **sin vencimiento** hasta que registre su primer pago (el pago es el que fija el vencimiento). Hasta entonces figura como `Sin cuota`.
  - El combo de **entrenador** está conectado: se llena con los usuarios activos de rol `Entrenador` y la asignación se guarda en `Socios.EntrenadorId` (script `07`). Es opcional: un socio puede quedar sin entrenador asignado.
- [x] **RegistrarPago** (`RegistrarPago.xaml.cs`): registro real de pagos implementado — `ServicioPagos` + `PagoRepositorio.Agregar` + comprobante imprimible.
  - [x] Renovación de cuota: al registrar el pago se extiende el vencimiento del socio por exactamente un mes (se apila sobre el vencimiento actual si todavía está vigente; si ya venció, se cuenta desde hoy) y el comprobante muestra el nuevo vencimiento. Modelo elegido (Opción B): el vencimiento vive en `Socio`; `Membresia` queda como catálogo de planes.
- [x] **ControlAsistencia** (`ControlAsistencia.xaml.cs`): conectado a la base real con `AsistenciaRepositorio` + `ServicioAsistencias`. Registra el ingreso en `Asistencias`, bloquea el ingreso si la cuota está vencida o el socio dado de baja (RF-08/RF-09), y muestra el historial por socio y el estado de cuota (Vencida / Al día) calculado desde `Socio.FechaVencimiento`.
  - Nota de diseño: el historial se consulta **por `SocioId`**, no por nombre. El filtro de "asistencias de hoy" usa el rango `[hoy, mañana)` porque `FechaHoraIngreso.Date` no se traduce de forma confiable a SQL.

---

## 3. Admin — completado

- [x] **Reportes** (`Reportes.xaml.cs`): conectado a datos reales. Pagos desde `ServicioPagos.ObtenerTodos()`, asistencias desde `ServicioAsistencias.ObtenerTodos()` y socios desde `ServicioReportes.ObtenerTodosLosSocios()`.
  - El botón **"Ver detalle"** (asistencias) NO cambia: es un filtro por socio sobre el mismo resultado. No requiere tabla nueva.
  - El join para mostrar DNI y actividad en el reporte de pagos se resuelve con un **diccionario de socios por `Id`**, porque `ServicioPagos.ObtenerTodos()` trae el `Socio` pero **no** su `Membresia`. No se tocó ningún repositorio.
  - Corrección de un bug real que traía el demo: las asistencias se agrupaban y se filtraban **por nombre**, lo que fusionaba dos socios distintos con el mismo nombre. Ahora agrupa y filtra por `SocioId` (se agregó `SocioId` a `ReporteAsistenciaSocioVista`).
  - ⚠️ La pantalla **no tiene selector de período** (verificado en el XAML: solo pestañas, buscador, exportadores y tortas). Por eso lista **todos** los registros. Los métodos `ServicioReportes.ObtenerPagosPorMes` y `ObtenerAsistenciasPorRango` quedan sin uso: son la base natural si más adelante se agrega un filtro mensual.
- [x] **Panel de inicio** (`PanelInicioAdmin.xaml.cs`): eliminada la simulación de barras (cuando no había pagos mostraba valores inventados) y la "actividad reciente" hardcodeada. Ahora el gráfico refleja los ingresos reales (si no hay pagos, se ven en cero, con la verdad) y la actividad se deriva de pagos + asistencias reales, igual que en el panel del Recepcionista.

> Con esto, **Administrador y Recepcionista no tienen ninguna pantalla con datos demo.** El único consumo restante de `DatosRecepDemo` es `VentanaMisAlumnos` del perfil Entrenador, que está fuera de alcance (sección 4).

---

## 4. Entrenador — FUERA DE ALCANCE

> ⛔ **El perfil Entrenador completo lo desarrolla otra persona del equipo.** No se toca nada de `SGG/Formularios/Entrenador/` ni se crean `RutinaRepositorio` / `ServicioRutinas` desde acá.
>
> Queda registrado para quien lo tome: falta `RutinaRepositorio` + `ServicioRutinas` (no existen) y conectar `VentanaListaRutinas`, `VentanaGestionRutinas`, `VentanaElegirRutina`, `VentanaMisAlumnos` (depende de `Socios.EntrenadorId`) y `PanelInicioEntrenador` (hoy usa `CargarDatosDePrueba()`).
>
> ⚠️ **Coordinación de scripts**: si esa persona agrega migraciones, tiene que tomar el siguiente número libre de `SGG.Datos/Scripts/` para no pisar los nuestros.

---

## 5. Base de datos — esquema y decisiones pendientes

- [x] **`Socios.EntrenadorId`** (FK → `Usuarios.Id`): implementado en el script `07_AgregarEntrenadorASocios.sql` (columna nullable, FK sin cascada e índice `IX_Socios_EntrenadorId`). El combo de entrenador de `AltaSocio` ya persiste la selección y la preselecciona al editar. (Ojo: "Mis Alumnos" es del perfil Entrenador, fuera de alcance; la columna ya la destraba, pero esa ventana sigue leyendo el demo hasta que se conecte.)
- [x] **Historial de membresías (decisión resuelta)**: se adoptó la **Opción B**: `Membresia` queda como **catálogo de planes** y la fecha de vencimiento de la cuota vive en `Socio` (`Socios.FechaVencimiento`), una por socio. La renovación apila un mes sobre el vencimiento vigente (o cuenta desde hoy si ya venció). Implementado en el script `04_MoverVencimientoASocio.sql`. La Opción A (`Membresias` en 1:N con `SocioId`, una fila por período) queda descartada para esta entrega: aportaría historial de períodos, que hoy no es un requerimiento de la UI.
- [ ] **Tabla `Rutinas`**: faltan columnas que la UI ya muestra — `Estado`, `DuracionSemanas`, `FrecuenciaSemanal`, `Nivel`, `Objetivo`. Verificar también `Ejercicio`. Es migración.
- [x] **`Socios.FechaAlta`**: implementado en el script `04_MoverVencimientoASocio.sql`. "Nuevos socios del mes" ahora usa la fecha de alta propia del socio (`SocioRepositorio.ContarNuevosDelMes`), no la fecha de inicio de la membresía.
- [x] **Actividad reciente de dashboards**: NO crear tabla nueva; derivarla de `Pagos` + `Asistencias` recientes con consultas. Implementado en los paneles del Recepcionista y del Admin (sección 3).
  - Limitación conocida y aceptada: se traen los registros completos (`ObtenerTodos()`) y se ordenan en memoria, porque el volumen del proyecto es chico. Con muchos datos convendría un `ObtenerUltimos(int)` en los repositorios.

---

## 6. Orden de trabajo sugerido

1. Migraciones de esquema: `Rutinas` (script 05), `Ejercicios` (script 06) y `Socios.EntrenadorId` (script 07) ya están resueltas. El vencimiento por socio y `Socios.FechaAlta` se resolvieron en el 04. No quedan migraciones propias pendientes.
2. Repositorios faltantes: `RutinaRepositorio`, `EjercicioRepositorio` (`AsistenciaRepositorio` ya existe).
3. Servicios faltantes: `ServicioRutinas` (`ServicioPagos` y `ServicioAsistencias` ya existen).
4. ~~Conectar Recepcionista: panel de inicio, gestión de socios y alta de socio.~~ ✅ Hecho (asistencia y pago ya estaban conectados). El perfil Recepcionista no tiene pantallas con datos demo.
5. ~~Conectar Reportes de Admin + limpiar simulaciones del dashboard (`PanelInicioAdmin`).~~ ✅ Hecho.
6. ~~Conectar Entrenador (rutinas, ejercicios, mis alumnos, panel).~~ ⛔ Fuera de alcance: lo hace otra persona del equipo.
7. ~~Habilitar el combo de entrenador de `AltaSocio` (requiere la migración `Socios.EntrenadorId`).~~ ✅ Hecho: script `07` + cableado en dominio, repositorio, servicio y formulario.

> Nota: Recepcionista y Admin ya leen datos reales. Los nombres demo (Ana García, Martín López, etc.) solo siguen apareciendo en la ventana "Mis Alumnos" del perfil Entrenador, fuera de alcance (sección 4).

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

4. **Ejecutar los scripts en orden (01 → 07)** desde SSMS, conectado a la instancia local:
   - `SGG.Datos/Scripts/01_EsquemaBase.sql` — esquema de tablas.
   - `SGG.Datos/Scripts/02_DatosPrueba.sql` — datos de prueba (usuarios, socios, membresías, pagos).
   - `SGG.Datos/Scripts/03_MigrarNormalizarUsuarioRol.sql` — **obligatorio**: sin él el login falla porque EF espera `Usuarios.RolId` y la tabla `Roles`.
   - `SGG.Datos/Scripts/04_MoverVencimientoASocio.sql` — **obligatorio**: agrega `FechaAlta` y `FechaVencimiento` a `Socios` (el vencimiento pasa a ser del socio, no del plan) y migra los datos actuales.
   - `SGG.Datos/Scripts/05_ActualizarRutinas.sql` — **obligatorio**: agrega `Objetivo`, `DuracionSemanas`, `FrecuenciaSemanal`, `Nivel`, `Estado` y `Notas` a `Rutinas`.
   - `SGG.Datos/Scripts/06_CrearEjercicios.sql` — **obligatorio**: crea la tabla `Ejercicios` con FK a `Rutinas`.
   - `SGG.Datos/Scripts/07_AgregarEntrenadorASocios.sql` — **obligatorio**: agrega `Socios.EntrenadorId` (FK a `Usuarios`) e índice, para asignar entrenador a cada socio.

   Los scripts 04 a 07 son re-ejecutables (tienen guardas `IF`). Los scripts 01, 02 y 03 **no**: se corren una sola vez (el 02 duplicaría datos y el 03 falla porque ya eliminó la columna `Rol`).

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
