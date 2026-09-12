using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SGG.Formularios.Recepcionista
{
    public partial class AltaSocio : Window
    {
        // Planes ficticios de membresía (RF-01).
        // TODO integración BD: reemplazar por ServicioMembresias.ObtenerVigentes() (SGG.Logica / SGG.Datos).
        private static readonly List<MembresiaItem> Planes = new()
        {
            new MembresiaItem { Id = 1, Nombre = "Musculación", Precio = 18000m, DuracionDias = 30, Descripcion = "Musculación - $18.000 (30 días)" },
            new MembresiaItem { Id = 2, Nombre = "Funcional",   Precio = 15000m, DuracionDias = 30, Descripcion = "Funcional - $15.000 (30 días)" },
            new MembresiaItem { Id = 3, Nombre = "Combinado",   Precio = 22000m, DuracionDias = 30, Descripcion = "Combinado - $22.000 (30 días)" }
        };

        private readonly SocioDemo? _socioExistente;

        /// <summary>Modo alta.</summary>
        public AltaSocio() : this(null)
        {
        }

        /// <summary>Modo edición (RF-02): recibe el socio existente para precargar el formulario.</summary>
        public AltaSocio(SocioDemo? socioExistente)
        {
            InitializeComponent();
            _socioExistente = socioExistente;

            CargarMembresias();

            if (_socioExistente != null)
                PrepararEdicion();
            else if (Planes.Count > 0)
                cmbMembresia.SelectedIndex = 0;
        }

        private void CargarMembresias()
        {
            cmbMembresia.ItemsSource = Planes;
        }

        private void PrepararEdicion()
        {
            txtTitulo.Text = "EDITAR SOCIO";
            Title = "SGG - Editar Socio";
            btnRegistrar.Content = "💾 GUARDAR CAMBIOS";
            txtDniAyuda.Visibility = Visibility.Visible;

            var s = _socioExistente!;
            txtNombre.Text = s.Nombre;
            txtApellido.Text = s.Apellido;
            txtDni.Text = s.Dni;
            txtDni.IsEnabled = false; // RF-02: el DNI es inalterable
            dpNacimiento.SelectedDate = s.FechaNacimiento;
            txtTelefono.Text = s.Telefono;
            txtEmail.Text = s.Email;

            var planActual = Planes.FirstOrDefault(p => p.Nombre == s.TipoMembresia);
            if (planActual != null)
                cmbMembresia.SelectedItem = planActual;
            else if (Planes.Count > 0)
                cmbMembresia.SelectedIndex = 0;
        }

        private void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            OcultarError();

            string nombre = txtNombre.Text.Trim();
            string apellido = txtApellido.Text.Trim();
            string dni = txtDni.Text.Trim();
            string telefono = txtTelefono.Text.Trim();
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido)
                || string.IsNullOrWhiteSpace(dni))
            {
                MostrarError("Debe completar los campos obligatorios (Nombre, Apellido y DNI).");
                return;
            }

            if (dpNacimiento.SelectedDate is not DateTime fechaNacimiento)
            {
                MostrarError("Debe seleccionar la fecha de nacimiento.");
                return;
            }

            if (fechaNacimiento > DateTime.Today)
            {
                MostrarError("La fecha de nacimiento no puede ser futura.");
                return;
            }

            if (cmbMembresia.SelectedItem is not MembresiaItem plan)
            {
                MostrarError("Debe seleccionar un plan de membresía.");
                return;
            }

            if (_socioExistente != null)
            {
                GuardarCambios(nombre, apellido, fechaNacimiento, telefono, email, plan);
                return;
            }

            // RF-01 (alta): el DNI no se puede repetir contra el padrón demo
            if (DatosRecepDemo.Socios.Any(s => s.Dni.Equals(dni, StringComparison.OrdinalIgnoreCase)))
            {
                MostrarError("Ya existe un socio con ese DNI.");
                return;
            }

            var nuevo = new SocioDemo
            {
                Id = DatosRecepDemo.Socios.Count > 0 ? DatosRecepDemo.Socios.Max(s => s.Id) + 1 : 1,
                Nombre = nombre,
                Apellido = apellido,
                Dni = dni,
                FechaNacimiento = fechaNacimiento,
                Telefono = telefono,
                Email = email,
                Activo = true,
                TipoMembresia = plan.Nombre,
                Precio = plan.Precio,
                FechaInicio = DateTime.Today,
                FechaVencimiento = DateTime.Today.AddDays(plan.DuracionDias),
                UltimoPagoMonto = null,
                UltimoPagoFecha = null
            };

            DatosRecepDemo.Socios.Add(nuevo);

            DialogResult = true;
            Close();
        }

        private void GuardarCambios(string nombre, string apellido, DateTime fechaNacimiento,
                                    string telefono, string email, MembresiaItem plan)
        {
            var original = _socioExistente!;

            // RF-02: el DNI queda inalterable, se conserva del registro original.
            var actualizado = new SocioDemo
            {
                Id = original.Id,
                Nombre = nombre,
                Apellido = apellido,
                Dni = original.Dni,
                FechaNacimiento = fechaNacimiento,
                Telefono = telefono,
                Email = email,
                Activo = original.Activo,
                TipoMembresia = plan.Nombre,
                Precio = plan.Precio,
                FechaInicio = original.FechaInicio,
                // La cuota vigente no se modifica al editar los datos del socio.
                FechaVencimiento = original.FechaVencimiento,
                UltimoPagoMonto = original.UltimoPagoMonto,
                UltimoPagoFecha = original.UltimoPagoFecha
            };

            int indice = DatosRecepDemo.Socios.FindIndex(s => s.Id == original.Id);
            if (indice >= 0)
                DatosRecepDemo.Socios[indice] = actualizado;

            DialogResult = true;
            Close();
        }

        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            txtError.Visibility = Visibility.Visible;
        }

        private void OcultarError()
        {
            txtError.Visibility = Visibility.Collapsed;
        }
    }

    public class MembresiaItem
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int DuracionDias { get; set; } = 30;
    }
}