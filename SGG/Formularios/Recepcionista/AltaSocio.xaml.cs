using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using SGG.Dominio.Entidades;
using SGG.Logica.Servicios;

namespace SGG.Formularios.Recepcionista
{
    public partial class AltaSocio : Window
    {
        private readonly ServicioSocios _servicioSocios = new();
        private readonly ServicioMembresias _servicioMembresias = new();
        private List<MembresiaItem> _planes = new();

        private readonly Socio? _socioExistente;

        /// <summary>Modo alta.</summary>
        public AltaSocio() : this(null)
        {
        }

        /// <summary>Modo edición (RF-02): recibe el socio existente para precargar el formulario.</summary>
        public AltaSocio(Socio? socioExistente)
        {
            InitializeComponent();
            _socioExistente = socioExistente;

            Validadores.SoloNumeros(txtDni);
            Validadores.SoloNumeros(txtTelefono);
            Validadores.SoloLetras(txtNombre);
            Validadores.SoloLetras(txtApellido);

            CargarMembresias();
            CargarEntrenadores();

            if (_socioExistente != null)
                PrepararEdicion();
            else if (_planes.Count > 0)
                cmbMembresia.SelectedIndex = 0;
        }

        private void CargarMembresias()
        {
            // Los planes ahora son el catálogo real: membresías vigentes de la base.
            _planes = _servicioMembresias.ObtenerVigentes()
                .Select(m => new MembresiaItem
                {
                    Id = m.Id,
                    Nombre = m.TipoActividad.ToString(),
                    Precio = m.Precio,
                    Descripcion = $"{m.TipoActividad} — {m.Precio.ToString("C0", new CultureInfo("es-AR"))}"
                })
                .ToList();

            cmbMembresia.ItemsSource = _planes;
        }

        /// <summary>
        /// Carga los usuarios con rol Entrenador para asignar un socio (opcional).
        /// Solo lectura del backend: si no hay entrenadores activos el combo queda vacío
        /// y el alta funciona igual.
        /// </summary>
        private void CargarEntrenadores()
        {
            var reales = new ServicioUsuarios().ObtenerTodos()
                .Where(u => u.Activo && string.Equals(u.Rol?.Nombre, "Entrenador", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (reales.Count > 0)
            {
                cmbEntrenador.ItemsSource = reales
                    .Select(r => new EntrenadorItem { Id = r.Id, NombreCompleto = $"{r.Nombre} {r.Apellido}".Trim() })
                    .ToList();
            }
            else
            {
                cmbEntrenador.ItemsSource = new List<EntrenadorItem>();
            }
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

            var planActual = _planes.FirstOrDefault(p => p.Id == s.MembresiaId);
            if (planActual != null)
                cmbMembresia.SelectedItem = planActual;
            else if (_planes.Count > 0)
                cmbMembresia.SelectedIndex = 0;

            // Entrenador a cargo (opcional): preseleccionar el asignado, si hay.
            if (s.EntrenadorId is int entrenadorId)
                cmbEntrenador.SelectedValue = entrenadorId;
            else
                cmbEntrenador.SelectedIndex = -1;
        }

        private void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            OcultarError();

            string nombre = txtNombre.Text.Trim();
            string apellido = txtApellido.Text.Trim();
            // Se normalizan quitando no-dígitos (toleran teléfonos demo con guiones al editar).
            string dni = new string(txtDni.Text.Where(char.IsDigit).ToArray());
            string telefono = new string(txtTelefono.Text.Where(char.IsDigit).ToArray());
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido)
                || string.IsNullOrWhiteSpace(dni))
            {
                MostrarError("Debe completar los campos obligatorios (Nombre, Apellido y DNI).");
                return;
            }

            // Email opcional: si viene, debe tener formato válido.
            if (!string.IsNullOrWhiteSpace(email) && !EsEmailValido(email))
            {
                MostrarError("Ingrese un email válido.");
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

            int? entrenadorId = (cmbEntrenador.SelectedItem as EntrenadorItem)?.Id;

            if (_socioExistente != null)
            {
                GuardarCambios(nombre, apellido, fechaNacimiento, telefono, email, plan, entrenadorId);
                return;
            }

            // RF-01 (alta): el DNI no se puede repetir contra el padrón real.
            // Si el DNI ya existe pero el socio está inactivo, se ofrece reactivarlo
            // en lugar de volver a registrarlo (los datos ya están en el sistema).
            var existente = _servicioSocios.ObtenerTodos()
                .FirstOrDefault(s => s.Dni.Equals(dni, StringComparison.OrdinalIgnoreCase));

            if (existente != null)
            {
                if (existente.Activo)
                {
                    MostrarError("Ya existe un socio ACTIVO con ese DNI.");
                    return;
                }

                var confirmar = MessageBox.Show(
                    $"Ya existe un socio con el DNI {dni} ({existente.Nombre} {existente.Apellido}) y está INACTIVO.\n\n" +
                    "¿Querés reactivarlo en lugar de registrarlo de nuevo?",
                    "Socio existente",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmar == MessageBoxResult.Yes)
                {
                    var (reactivado, mensajeReactivacion) = _servicioSocios.Reactivar(existente.Id);
                    if (!reactivado)
                    {
                        MostrarError(mensajeReactivacion);
                        return;
                    }

                    DialogResult = true;
                    Close();
                }
                else
                {
                    MostrarError("Ese DNI ya está registrado en el sistema. No se puede volver a dar de alta.");
                }

                return;
            }

            var (exitoso, mensaje) = _servicioSocios.AltaSocio(
                nombre, apellido, dni, fechaNacimiento, telefono, email, plan.Id, entrenadorId);

            if (!exitoso)
            {
                MostrarError(mensaje);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void GuardarCambios(string nombre, string apellido, DateTime fechaNacimiento,
                                    string telefono, string email, MembresiaItem plan, int? entrenadorId)
        {
            var (exitoso, mensaje) = _servicioSocios.EditarSocio(
                _socioExistente!.Id, nombre, apellido, fechaNacimiento, telefono, email, plan.Id, entrenadorId);

            if (!exitoso)
            {
                MostrarError(mensaje);
                return;
            }

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

        /// <summary>Criterio demo: contiene "@" y un punto con al menos 2 caracteres después (p. ej. ".com").</summary>
        private static bool EsEmailValido(string email)
        {
            int arroba = email.IndexOf('@');
            int punto = email.LastIndexOf('.');
            return arroba > 0 && punto > arroba + 1 && email.Length - punto >= 2;
        }
    }

    public class MembresiaItem
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }

        public override string ToString() => Descripcion;
    }

    /// <summary>Item del combo de entrenadores (usuarios con rol Entrenador).</summary>
    public class EntrenadorItem
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;

        public override string ToString() => NombreCompleto;
    }
}