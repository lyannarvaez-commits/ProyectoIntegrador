// ============================================================
// SeleccionDTO.cs — Datos de una selección nacional
// ============================================================

namespace FrontendAdministrativo.DTOs
{
    /// <summary>
    /// Representa una selección nacional participante en el Mundial 2026.
    /// </summary>
    public class SeleccionDTO
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        /// <summary>Código FIFA de la selección (ej: "ARG", "BRA")</summary>
        public string CodigoFifa { get; set; } = string.Empty;

        /// <summary>Confederación a la que pertenece (ej: "CONMEBOL", "UEFA")</summary>
        public string Confederacion { get; set; } = string.Empty;

        public string Grupo { get; set; } = string.Empty;

        /// <summary>Nombre del país (puede ser igual a Nombre o variante)</summary>
        public string Pais { get; set; } = string.Empty;

        /// <summary>Código ISO del país (para banderas, ej: "AR", "BR")</summary>
        public string CodigoPais { get; set; } = string.Empty;

        /// <summary>Indica si la selección ha sido eliminada del torneo</summary>
        public bool Eliminado { get; set; } = false;  // 🔥 NUEVO
    }
}