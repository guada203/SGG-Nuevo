# SGG — Sistema de Gestión de Gimnasio

Aplicación de escritorio WPF para la gestión integral de un gimnasio: socios, membresías, pagos, asistencias, rutinas y usuarios. Desarrollada en C#/.NET 10 con arquitectura en capas y Entity Framework Core.

## Perfiles del sistema

| Perfil | Funcionalidades principales |
|---|---|
| **Administrador** | Dashboard con KPIs, gestión de usuarios (alta/baja/reactivación con roles), gestión de socios, gestión de membresías, reportes de pagos/asistencias/socios con exportación a PDF |
| **Recepcionista** | Dashboard, gestión de socios, alta de socios, registro de pagos, control de asistencia (ingreso/egreso e historial del socio) |
| **Entrenador** | Mis alumnos, gestión de rutinas (crear/editar), listado de rutinas, asignación de rutinas a socios, creación de ejercicios |

## Stack tecnológico

- **.NET 10** — WPF (Windows Presentation Foundation)
- **Entity Framework Core** — acceso a datos con SQL Server
- **BCrypt.Net-Next** — hash seguro de contraseñas
- Arquitectura en 4 proyectos: `SGG` (UI), `SGG.Logica` (servicios), `SGG.Datos` (repositorios + EF Core), `SGG.Dominio` (entidades)

## Requisitos para ejecutar

- Windows (la app usa WPF)
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022 (17.14+) con la carga de trabajo "Desarrollo para escritorio con .NET"
- SQL Server (Express o superior), LocalDB o un servidor accesible en red

### Configuración de la base de datos

La cadena de conexión se encuentra en `SGG.Datos/Contexto/SggDbContext.cs`. Por defecto apunta a un servidor local:

```
Server=DESKTOP-2GR5V5M\SQLEXPRESS;Database=SGG;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=30;
```

El modelo de datos incluye las entidades `Socio`, `Usuario`, `Rol`, `Membresia`, `Pago`, `Asistencia` y `Rutina`, con sus relaciones y migraciones de EF Core correspondientes.

### Cómo levantar el proyecto

1. Abrir la solución `SGG.slnx` con Visual Studio.
2. Ajustar la cadena de conexión al servidor SQL disponible (ver sección anterior).
3. Aplicar las migraciones de EF Core para crear la base de datos y las tablas.
4. Compilar y ejecutar el proyecto `SGG`.

## Autenticación

El login solicita **email y contraseña**, y valida las credenciales contra la base de datos real usando BCrypt para el hash de las contraseñas. Al ingresar se selecciona el perfil (Administrador, Recepcionista o Entrenador) y cada perfil accede únicamente a sus módulos correspondientes.

El botón de "ver/ocultar" en el campo de contraseña permite revisar lo escrito sin comprometer la validación.

## Módulos conectados a base de datos real

| Módulo | Tecnología |
|---|---|
| Autenticación (login) | `ServicioAutenticacion` + BCrypt contra la BD |
| Gestión de usuarios (alta/baja/reactivación, roles) | `ServicioUsuarios` + `RolRepositorio` |
| Gestión de socios (perfil Administrador) | `ServicioSocios` + `SocioRepositorio` |
| Gestión de membresías | `ServicioMembresias` + repositorio EF Core |
| Dashboard del Administrador | `ServicioDashboard` (KPIs desde la BD) |

## Módulos con datos de demostración

`RegistrarPago`, `ControlAsistencia`, la gestión de socios del Recepcionista, los Reportes y los módulos del Entrenador operan sobre un conjunto de datos de demostración en memoria (`SGG/Formularios/Recepcionista/DatosRecepDemo.cs`) para que la aplicación sea funcional sin depender de datos precargados. La arquitectura de servicios ya está preparada para conectar estos módulos a la base de datos real en la próxima etapa.

## Estructura del repositorio

```
SGG.slnx
├── SGG/                     # Proyecto WPF (ventanas, estilos, navegación)
│   └── Formularios/
│       ├── Login/           # VentanaSeleccionRol, VentanaLogin
│       ├── Admin/           # Dashboard, usuarios, socios, membresías, reportes
│       ├── Recepcionista/   # Dashboard, socios, pagos, asistencia
│       └── Entrenador/      # Dashboard, rutinas, ejercicios, mis alumnos
├── SGG.Datos/               # EF Core, DbContext y repositorios
├── SGG.Dominio/             # Entidades del dominio
└── SGG.Logica/              # Servicios de aplicación (autenticación, socios, etc.)
```

## Notas

- Los datos demo se pierden al cerrar la aplicación (no persisten); los datos gestionados desde el perfil Administrador sí se guardan en la base de datos.
- Se usan estilos propios y un tema oscuro unificado definido en `App.xaml`.