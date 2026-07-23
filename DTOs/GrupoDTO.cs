// ============================================================
// GrupoDTO.cs — Datos de un grupo del Mundial
// ============================================================

namespace FrontendAdministrativo.DTOs
{
    /// <summary>
    /// Representa un grupo del torneo (Grupo A, B, C...).
    /// </summary>
    public class GrupoDTO
    {
        public int Id { get; set; }

        /// <summary>Código del grupo (ej: "A", "B", "C")</summary>
        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty; // "Grupo A", "Grupo B"...

        public List<SeleccionDTO> Selecciones { get; set; } = new();
    }
}