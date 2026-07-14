using System.ComponentModel.DataAnnotations;

// ============================================================
// ResultadoDTO.cs — Datos para registrar el resultado de un partido
// ============================================================

namespace FrontendAdministrativo.DTOs
{
    /// <summary>
    /// Usado para enviar el resultado de un partido a la API.
    /// </summary>
    public class ResultadoDTO
    {
        [Required]
        public int PartidoId { get; set; }

        [Required(ErrorMessage = "Los goles del equipo local son obligatorios.")]
        [Display(Name = "Goles Local")]
        [Range(0, 99, ErrorMessage = "Los goles deben ser entre 0 y 99.")]
        public int GolesLocal { get; set; }

        [Required(ErrorMessage = "Los goles del equipo visitante son obligatorios.")]
        [Display(Name = "Goles Visitante")]
        [Range(0, 99, ErrorMessage = "Los goles deben ser entre 0 y 99.")]
        public int GolesVisitante { get; set; }
    }
}
