using System.ComponentModel.DataAnnotations;

// ============================================================
// LoginDTO.cs — Credenciales de acceso al panel administrativo
// ============================================================

namespace FrontendAdministrativo.DTOs
{
    /// <summary>
    /// Credenciales enviadas en el formulario de inicio de sesión.
    /// </summary>
    public class LoginDTO
    {
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }
    }
}
