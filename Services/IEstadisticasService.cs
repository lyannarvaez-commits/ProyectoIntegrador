using FrontendAdministrativo.DTOs;

// ============================================================
// IEstadisticasService.cs — Contrato del servicio de Estadísticas
// ============================================================

namespace FrontendAdministrativo.Services
{
    /// <summary>
    /// Define todos los métodos disponibles para interactuar con
    /// la API del Servicio de Estadísticas.
    /// </summary>
    public interface IEstadisticasService
    {
        // ── Partidos ──────────────────────────────────────────

        /// <summary>Obtiene la lista completa de partidos.</summary>
        Task<List<PartidoDTO>> GetPartidosAsync();

        /// <summary>Obtiene un partido por su ID.</summary>
        Task<PartidoDTO?> GetPartidoAsync(int id);

        /// <summary>Crea un nuevo partido.</summary>
        Task<bool> CreatePartidoAsync(PartidoDTO partido);

        /// <summary>Actualiza los datos de un partido existente.</summary>
        Task<bool> UpdatePartidoAsync(int id, PartidoDTO partido);

        /// <summary>Elimina un partido por su ID.</summary>
        Task<bool> DeletePartidoAsync(int id);

        /// <summary>Registra el resultado final de un partido.</summary>
        Task<bool> RegistrarResultadoAsync(int partidoId, int golesLocal, int golesVisitante);

        // ── Selecciones ───────────────────────────────────────

        /// <summary>Obtiene la lista de selecciones participantes.</summary>
        Task<List<SeleccionDTO>> GetSeleccionesAsync();

        // ── Grupos ────────────────────────────────────────────

        /// <summary>Obtiene los grupos del torneo con sus selecciones.</summary>
        Task<List<GrupoDTO>> GetGruposAsync();

        // ── Usuarios ──────────────────────────────────────────

        /// <summary>Obtiene la lista de usuarios del sistema.</summary>
        Task<List<UsuarioDTO>> GetUsuariosAsync();

        /// <summary>Actualiza los datos de un usuario (rol, estado).</summary>
        Task<bool> UpdateUsuarioAsync(int id, UsuarioDTO usuario);

        /// <summary>Elimina un usuario por su ID.</summary>
        Task<bool> DeleteUsuarioAsync(int id);

        // ── Dashboard ─────────────────────────────────────────

        /// <summary>Obtiene las estadísticas generales para el dashboard.</summary>
        Task<DashboardStatsDTO> GetDashboardStatsAsync();
    }
}
