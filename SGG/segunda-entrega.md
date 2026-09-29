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
- [ ] **RegistrarPago** (`RegistrarPago.xaml.cs`): crear `ServicioPagos` que inserte en `Pagos` y extienda el vencimiento de la membresía (renovación).
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