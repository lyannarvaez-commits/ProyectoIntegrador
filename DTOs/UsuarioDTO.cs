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

        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Usuario")]
        public string Username { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        public int RolId { get; set; }

        // 🔥 OBJETO ROL ANIDADO (viene del backend)
        public RolDTO? Rol { get; set; }

        // 🔥 PROPIEDAD CALCULADA PARA MOSTRAR EL NOMBRE DEL ROL
        public string RolNombre => Rol?.Nombre ?? "Sin rol";

        public string Password { get; set; } = string.Empty;

        [Display(Name = "Fecha de Registro")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime FechaRegistro { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        [Display(Name = "Saldo UTNGolCoin")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal SaldoUTNGolCoin { get; set; }

        // ── Propiedades calculadas ─────────────────────────────
        public string BadgeEstado => Activo ? "badge bg-success" : "badge bg-danger";
        public string TextoEstado => Activo ? "Activo" : "Inactivo";
        public string BadgeRol => (RolNombre == "ADMINISTRADOR" || RolNombre == "Administrador")
            ? "badge bg-warning text-dark"
            : "badge bg-info";
    }

    /// <summary>
    /// DTO para el objeto Rol anidado que devuelve el backend
    /// </summary>
    public class RolDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}