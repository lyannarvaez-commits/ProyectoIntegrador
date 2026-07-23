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
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        [Display(Name = "Usuario")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Recordarme")]
        public bool Recordarme { get; set; }
    }
}