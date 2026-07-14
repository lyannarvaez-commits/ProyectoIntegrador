using System.ComponentModel.DataAnnotations;

// ============================================================
// UsuarioDTO.cs — Datos de un usuario del sistema
// ============================================================

namespace FrontendAdministrativo.DTOs
{
    /// <summary>
    /// Representa un usuario registrado en el sistema UTN GolMundial.
    /// </summary>
    public class UsuarioDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [Display(Name = "Nombre")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio.")]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = "Usuario"; // Administrador, Usuario

        [Display(Name = "Fecha de Registro")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime FechaRegistro { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Saldo UTNGolCoin")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal SaldoUTNGolCoin { get; set; }

        // ── Propiedades calculadas ─────────────────────────────
        /// <summary>Badge CSS de estado activo/inactivo</summary>
        public string BadgeEstado => Activo ? "badge bg-success" : "badge bg-danger";

        /// <summary>Texto de estado</summary>
        public string TextoEstado => Activo ? "Activo" : "Inactivo";

        /// <summary>Badge CSS del rol</summary>
        public string BadgeRol => Rol == "Administrador" ? "badge bg-warning text-dark" : "badge bg-info";
    }
}
