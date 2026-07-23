using Newtonsoft.Json;
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

        // 🔥 AHORA USA EquipoLocalId
        [Required(ErrorMessage = "La selección local es obligatoria.")]
        [Display(Name = "Selección Local")]
        public int EquipoLocalId { get; set; }

        // 🔥 AHORA USA EquipoVisitanteId
        [Required(ErrorMessage = "La selección visitante es obligatoria.")]
        [Display(Name = "Selección Visitante")]
        public int EquipoVisitanteId { get; set; }

        // 🔥 AHORA USA SedeId
        [Required(ErrorMessage = "La sede es obligatoria.")]
        [Display(Name = "Sede")]
        public int SedeId { get; set; }

        [Required(ErrorMessage = "La fase es obligatoria.")]
        [Display(Name = "Fase")]
        public string Fase { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha y hora es obligatoria.")]
        [Display(Name = "Fecha y Hora")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime FechaHora { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; } = "PROGRAMADO";

        [Display(Name = "Goles Local")]
        [Range(0, 99, ErrorMessage = "Los goles deben ser entre 0 y 99.")]
        public int? GolesLocal { get; set; }

        [Display(Name = "Goles Visitante")]
        [Range(0, 99, ErrorMessage = "Los goles deben ser entre 0 y 99.")]
        public int? GolesVisitante { get; set; }

        // ── Propiedades calculadas (solo lectura) ─────────────
        public string ResultadoFormateado =>
            (GolesLocal.HasValue && GolesVisitante.HasValue)
                ? $"{GolesLocal} - {GolesVisitante}"
                : "-";

        public string FechaFormateada =>
            FechaHora.ToString("dd/MM/yyyy HH:mm");

        // ── Propiedades para mostrar en la vista (sin validación) ──
        public string SeleccionLocal { get; set; } = string.Empty;
        public string SeleccionVisitante { get; set; } = string.Empty;
        public string Sede { get; set; } = string.Empty;
    }

    // ── DTO para la respuesta del backend (con objetos anidados) ──
    public class PartidoResponseDTO
    {
        public int Id { get; set; }

        [JsonProperty("local")]
        public string Local { get; set; } = string.Empty;

        [JsonProperty("visitante")]
        public string Visitante { get; set; } = string.Empty;

        [JsonProperty("fase")]
        public object Fase { get; set; } = new object();

        [JsonProperty("grupo")]
        public object Grupo { get; set; } = new object();

        [JsonProperty("sede")]
        public object Sede { get; set; } = new object();

        [JsonProperty("fechaHoraUtc")]
        public DateTime FechaHoraUtc { get; set; }

        [JsonProperty("estado")]
        public string Estado { get; set; } = string.Empty;

        [JsonProperty("golesLocal")]
        public int? GolesLocal { get; set; }

        [JsonProperty("golesVisitante")]
        public int? GolesVisitante { get; set; }

        // ── Método para convertir a PartidoDTO ────────────────
        public PartidoDTO ToPartidoDTO()
        {
            return new PartidoDTO
            {
                Id = this.Id,
                SeleccionLocal = this.Local,
                SeleccionVisitante = this.Visitante,
                Fase = ExtraerNombre(this.Fase),
                Sede = ExtraerNombre(this.Sede),
                FechaHora = this.FechaHoraUtc,
                Estado = this.Estado,
                GolesLocal = this.GolesLocal,
                GolesVisitante = this.GolesVisitante
            };
        }

        private string ExtraerNombre(object obj)
        {
            if (obj == null) return string.Empty;
            if (obj is string str) return str;

            try
            {
                var json = JsonConvert.SerializeObject(obj);
                var temp = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (temp != null && temp.ContainsKey("nombre"))
                {
                    return temp["nombre"]?.ToString() ?? string.Empty;
                }
                if (temp != null && temp.ContainsKey("codigo"))
                {
                    return temp["codigo"]?.ToString() ?? string.Empty;
                }
            }
            catch { }

            return obj.ToString() ?? string.Empty;
        }
    }
}