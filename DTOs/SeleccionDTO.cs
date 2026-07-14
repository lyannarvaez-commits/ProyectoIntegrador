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

        public string Pais { get; set; } = string.Empty;

        public string Grupo { get; set; } = string.Empty;

        /// <summary>Código ISO del país (para banderas, ej: "AR", "BR")</summary>
        public string CodigoPais { get; set; } = string.Empty;
    }
}
