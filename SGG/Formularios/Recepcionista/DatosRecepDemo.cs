using System;
using System.Collections.Generic;
using System.Linq;

namespace SGG.Formularios.Recepcionista
{
    // TODO integración BD: reemplazar esta clase por los repositorios reales (SGG.Datos) en la fase de conexión.

    /// <summary>
    /// Entidad demo de socio con membresía embebida (tipo, precio, vigencia y último pago).
    /// </summary>
    public class SocioDemo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Dni { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Activo { get; set; }

        // A cargo del entrenador (id de Usuario rol Entrenador). Front/demo: en la fase BD se une por FK.
        public int? EntrenadorId { get; set; }

        // Membresía embebida (en la fase BD vendría de la entidad Membresia)
        public string TipoMembresia { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal? UltimoPagoMonto { get; set; }
        public DateTime? UltimoPagoFecha { get; set; }

        public string NombreCompleto => $"{Nombre} {Apellido}".Trim();
    }

    /// <summary>Pago demo registrado en recepción.</summary>
    public class PagoDemo
    {
        public string SocioNombre { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string Metodo { get; set; } = string.Empty; // Efectivo / Tarjeta / Transferencia
    }

    /// <summary>Precio del catálogo de membresías (definido por el admin, demo).</summary>
    public class PrecioMembresiaDemo
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Etiqueta => $"{Nombre} — ${Monto.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("es-AR"))}";

        public override string ToString() => Etiqueta;
    }

    /// <summary>Registro de asistencia (ingreso) demo.</summary>
    public class AsistenciaDemo
    {
        public string SocioNombre { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
    }

    /// <summary>
    /// Única fuente de datos demo de las pantallas del Recepcionista.
    /// Las pantallas leen mediante Obtener*() (copia viva del respaldo, sin compartir
    /// instancias de colección entre sí); DatosRecepDemo.Socios queda expuesto porque
    /// AltaSocio/GestionSocios necesitan mutarlo (alta, edición y baja de socios).
    /// </summary>
    public static class DatosRecepDemo
    {
        private static readonly List<SocioDemo> _socios = CrearSocios();
        private static readonly List<PagoDemo> _pagos = CrearPagos();
        private static readonly List<AsistenciaDemo> _asistencias = CrearAsistencias();

        // TODO integración BD: reemplazar por el catálogo real de membresías (SGG.Datos) cuando corresponda.
        private static readonly List<PrecioMembresiaDemo> _preciosMembresias = new()
        {
            new PrecioMembresiaDemo { Nombre = "Musculación", Monto = 18000m },
            new PrecioMembresiaDemo { Nombre = "Funcional", Monto = 15000m },
            new PrecioMembresiaDemo { Nombre = "Combinado", Monto = 22000m }
        };

        public static List<SocioDemo> Socios => _socios;
        public static List<PagoDemo> Pagos => _pagos;
        public static List<AsistenciaDemo> Asistencias => _asistencias;

        /// <summary>Copia viva de la lista de socios (mismos objetos, lista nueva).</summary>
        public static List<SocioDemo> ObtenerSocios() => new(_socios);

        /// <summary>Copia viva de la lista de pagos.</summary>
        public static List<PagoDemo> ObtenerPagos() => new(_pagos);

        /// <summary>Copia viva de la lista de asistencias.</summary>
        public static List<AsistenciaDemo> ObtenerAsistencias() => new(_asistencias);

        /// <summary>Copia viva del catálogo de precios de membresías.</summary>
        public static List<PrecioMembresiaDemo> ObtenerPreciosMembresias() => new(_preciosMembresias);

        /// <summary>Serie de 6 montos simulados para el gráfico del dashboard (creciente).</summary>
        public static decimal[] IngresosPorMes() => new[] { 180000m, 195000m, 210000m, 240000m, 265000m, 285000m };

        /// <summary>Ingreso del mes actual, coherente con el último valor de IngresosPorMes().</summary>
        public static decimal IngresosDelMes()
        {
            var serie = IngresosPorMes();
            return serie[serie.Length - 1];
        }

        public static int CantidadSociosActivos() => _socios.Count(s => s.Activo);

        /// <summary>Cuotas por vencer: socios con vencimiento ya vencido o dentro de los próximos 30 días.</summary>
        public static int CantidadCuotasPorVencer()
        {
            var hoy = DateTime.Today;
            var limite = hoy.AddDays(30);
            return _socios.Count(s => s.FechaVencimiento < hoy || s.FechaVencimiento <= limite);
        }

        /// <summary>Estado de cuota calculado en vivo: "Vencida" si el vencimiento ya pasó, "Al día" en caso contrario.</summary>
        public static string EstadoCuota(SocioDemo socio)
            => socio.FechaVencimiento < DateTime.Today ? "Vencida" : "Al día";

        private static List<SocioDemo> CrearSocios()
        {
            var hoy = DateTime.Today;

            return new List<SocioDemo>
            {
                // Cuota VENCIDA (vencimiento en el pasado)
                new SocioDemo
                {
                    Id = 1, Nombre = "Ana", Apellido = "García", Dni = "30123456",
                    FechaNacimiento = new DateTime(1991, 4, 12), Telefono = "11-5555-0101",
                    Email = "ana.garcia@mail.com", Activo = true,
                    TipoMembresia = "Musculación", Precio = 18000m,
                    FechaInicio = hoy.AddDays(-35), FechaVencimiento = hoy.AddDays(-5),
                    UltimoPagoMonto = 18000m, UltimoPagoFecha = hoy.AddDays(-35)
                },
                // Cuota VENCIDA (vencimiento en el pasado)
                new SocioDemo
                {
                    Id = 2, Nombre = "Carlos", Apellido = "Pérez", Dni = "28765432",
                    FechaNacimiento = new DateTime(1988, 9, 3), Telefono = "11-5555-0102",
                    Email = "carlos.perez@mail.com", Activo = true,
                    TipoMembresia = "Funcional", Precio = 15000m,
                    FechaInicio = hoy.AddDays(-42), FechaVencimiento = hoy.AddDays(-12),
                    UltimoPagoMonto = 15000m, UltimoPagoFecha = hoy.AddDays(-42)
                },
                // PRÓXIMA a vencer (15 días)
                new SocioDemo
                {
                    Id = 3, Nombre = "Lucía", Apellido = "Fernández", Dni = "35223344",
                    FechaNacimiento = new DateTime(1995, 1, 20), Telefono = "11-5555-0103",
                    Email = "lucia.fernandez@mail.com", Activo = true,
                    TipoMembresia = "Combinado", Precio = 22000m,
                    FechaInicio = hoy.AddDays(-15), FechaVencimiento = hoy.AddDays(15),
                    UltimoPagoMonto = 22000m, UltimoPagoFecha = hoy.AddDays(-15)
                },
                // PRÓXIMA a vencer (21 días)
                new SocioDemo
                {
                    Id = 4, Nombre = "Martín", Apellido = "López", Dni = "31998877",
                    FechaNacimiento = new DateTime(1992, 11, 8), Telefono = "11-5555-0104",
                    Email = "martin.lopez@mail.com", Activo = true,
                    TipoMembresia = "Musculación", Precio = 18000m,
                    FechaInicio = hoy.AddDays(-9), FechaVencimiento = hoy.AddDays(21),
                    UltimoPagoMonto = 18000m, UltimoPagoFecha = hoy.AddDays(-9)
                },
                // Al día
                new SocioDemo
                {
                    Id = 5, Nombre = "Sofía", Apellido = "Ramírez", Dni = "33114455",
                    FechaNacimiento = new DateTime(1993, 6, 25), Telefono = "11-5555-0105",
                    Email = "sofia.ramirez@mail.com", Activo = true,
                    TipoMembresia = "Funcional", Precio = 15000m,
                    FechaInicio = hoy.AddDays(-15), FechaVencimiento = hoy.AddDays(45),
                    UltimoPagoMonto = 15000m, UltimoPagoFecha = hoy.AddDays(-15)
                },
                // Al día
                new SocioDemo
                {
                    Id = 6, Nombre = "Diego", Apellido = "Torres", Dni = "27557788",
                    FechaNacimiento = new DateTime(1985, 3, 17), Telefono = "11-5555-0106",
                    Email = "diego.torres@mail.com", Activo = true,
                    TipoMembresia = "Musculación", Precio = 18000m,
                    FechaInicio = hoy.AddDays(-2), FechaVencimiento = hoy.AddDays(58),
                    UltimoPagoMonto = 18000m, UltimoPagoFecha = hoy.AddDays(-2)
                },
                // INACTIVO (y con cuota vencida)
                new SocioDemo
                {
                    Id = 7, Nombre = "Valentina", Apellido = "Ríos", Dni = "36112233",
                    FechaNacimiento = new DateTime(1996, 8, 30), Telefono = "11-5555-0107",
                    Email = "valentina.rios@mail.com", Activo = false,
                    TipoMembresia = "Combinado", Precio = 22000m,
                    FechaInicio = hoy.AddDays(-50), FechaVencimiento = hoy.AddDays(-20),
                    UltimoPagoMonto = 22000m, UltimoPagoFecha = hoy.AddDays(-50)
                },
                // Al día
                new SocioDemo
                {
                    Id = 8, Nombre = "Joaquín", Apellido = "Silva", Dni = "30445566",
                    FechaNacimiento = new DateTime(1990, 5, 14), Telefono = "11-5555-0108",
                    Email = "joaquin.silva@mail.com", Activo = true,
                    TipoMembresia = "Funcional", Precio = 15000m,
                    FechaInicio = hoy.AddDays(-4), FechaVencimiento = hoy.AddDays(56),
                    UltimoPagoMonto = 15000m, UltimoPagoFecha = hoy.AddDays(-4)
                },
                // RECIÉN INGRESADO HOY (vence dentro de 30 días)
                new SocioDemo
                {
                    Id = 9, Nombre = "Camila", Apellido = "Benítez", Dni = "34889900",
                    FechaNacimiento = new DateTime(1994, 12, 2), Telefono = "11-5555-0109",
                    Email = "camila.benitez@mail.com", Activo = true,
                    TipoMembresia = "Musculación", Precio = 18000m,
                    FechaInicio = hoy, FechaVencimiento = hoy.AddDays(30),
                    UltimoPagoMonto = 18000m, UltimoPagoFecha = hoy
                }
            };
        }

        private static List<PagoDemo> CrearPagos()
        {
            var hoy = DateTime.Today;

            return new List<PagoDemo>
            {
                new PagoDemo { SocioNombre = "Camila Benítez", Monto = 18000m, Fecha = hoy.AddHours(9).AddMinutes(30),  Metodo = "Efectivo" },
                new PagoDemo { SocioNombre = "Diego Torres",    Monto = 18000m, Fecha = hoy.AddDays(-2).AddHours(18),  Metodo = "Efectivo" },
                new PagoDemo { SocioNombre = "Joaquín Silva",   Monto = 15000m, Fecha = hoy.AddDays(-4).AddHours(11),  Metodo = "Transferencia" },
                new PagoDemo { SocioNombre = "Martín López",    Monto = 18000m, Fecha = hoy.AddDays(-9).AddHours(17),  Metodo = "Transferencia" },
                new PagoDemo { SocioNombre = "Sofía Ramírez",   Monto = 15000m, Fecha = hoy.AddDays(-15).AddHours(10), Metodo = "Tarjeta" },
                new PagoDemo { SocioNombre = "Lucía Fernández", Monto = 22000m, Fecha = hoy.AddDays(-15).AddHours(19), Metodo = "Tarjeta" },
                new PagoDemo { SocioNombre = "Ana García",      Monto = 18000m, Fecha = hoy.AddDays(-35).AddHours(16), Metodo = "Efectivo" },
                new PagoDemo { SocioNombre = "Carlos Pérez",    Monto = 15000m, Fecha = hoy.AddDays(-42).AddHours(12), Metodo = "Efectivo" },
                new PagoDemo { SocioNombre = "Valentina Ríos",  Monto = 22000m, Fecha = hoy.AddDays(-50).AddHours(15), Metodo = "Tarjeta" }
            };
        }

        private static List<AsistenciaDemo> CrearAsistencias()
        {
            var hoy = DateTime.Today;

            return new List<AsistenciaDemo>
            {
                // Varias de HOY con distintas horas
                new AsistenciaDemo { SocioNombre = "Ana García",      FechaHora = hoy.AddHours(8).AddMinutes(42) },
                new AsistenciaDemo { SocioNombre = "Carlos Pérez",    FechaHora = hoy.AddHours(9).AddMinutes(15) },
                new AsistenciaDemo { SocioNombre = "Lucía Fernández", FechaHora = hoy.AddHours(10).AddMinutes(5) },
                new AsistenciaDemo { SocioNombre = "Camila Benítez",  FechaHora = hoy.AddHours(15).AddMinutes(45) },
                new AsistenciaDemo { SocioNombre = "Martín López",    FechaHora = hoy.AddHours(17).AddMinutes(30) },
                new AsistenciaDemo { SocioNombre = "Sofía Ramírez",   FechaHora = hoy.AddHours(18).AddMinutes(20) },
                new AsistenciaDemo { SocioNombre = "Diego Torres",    FechaHora = hoy.AddHours(19).AddMinutes(10) },
                // Días previos
                new AsistenciaDemo { SocioNombre = "Ana García",      FechaHora = hoy.AddDays(-1).AddHours(18).AddMinutes(5) },
                new AsistenciaDemo { SocioNombre = "Lucía Fernández", FechaHora = hoy.AddDays(-2).AddHours(9).AddMinutes(40) },
                new AsistenciaDemo { SocioNombre = "Martín López",    FechaHora = hoy.AddDays(-2).AddHours(16).AddMinutes(55) },
                new AsistenciaDemo { SocioNombre = "Joaquín Silva",   FechaHora = hoy.AddDays(-3).AddHours(10).AddMinutes(30) },
                new AsistenciaDemo { SocioNombre = "Sofía Ramírez",   FechaHora = hoy.AddDays(-4).AddHours(8).AddMinutes(50) }
            };
        }
    }
}