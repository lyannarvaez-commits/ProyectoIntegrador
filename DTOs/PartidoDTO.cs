using System.ComponentModel.DataAnnotations;

// ============================================================
// PartidoDTO.cs — Datos de un partido del Mundial
// ============================================================

namespace FrontendAdministrativo.DTOs
{
    /// <summary>
    /// Representa un partido del Mundial de Fútbol 2026.
    /// </summary>
    public class PartidoDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La selección local es obligatoria.")]
        [Display(Name = "Selección Local")]
        public string SeleccionLocal { get; set; } = string.Empty;

        [Required(ErrorMessage = "La selección visitante es obligatoria.")]
        [Display(Name = "Selección Visitante")]
        public string SeleccionVisitante { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fase es obligatoria.")]
        [Display(Name = "Fase")]
        public string Fase { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha y hora es obligatoria.")]
        [Display(Name = "Fecha y Hora")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime FechaHora { get; set; }

        [Required(ErrorMessage = "La sede es obligatoria.")]
        [Display(Name = "Sede")]
        public string Sede { get; set; } = string.Empty;

        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, EnCurso, Finalizado

        [Display(Name = "Goles Local")]
        [Range(0, 99, ErrorMessage = "Los goles deben ser entre 0 y 99.")]
        public int? GolesLocal { get; set; }

        [Display(Name = "Goles Visitante")]
        [Range(0, 99, ErrorMessage = "Los goles deben ser entre 0 y 99.")]
        public int? GolesVisitante { get; set; }

        // ── Propiedades calculadas (solo lectura) ─────────────
        /// <summary>Resultado formateado: "2 - 1" o "-"</summary>
        public string ResultadoFormateado =>
            (GolesLocal.HasValue && GolesVisitante.HasValue)
                ? $"{GolesLocal} - {GolesVisitante}"
                : "-";

        /// <summary>Fecha formateada: dd/MM/yyyy HH:mm</summary>
        public string FechaFormateada =>
            FechaHora.ToString("dd/MM/yyyy HH:mm");
    }
}
