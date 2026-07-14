// ============================================================
// DashboardStatsDTO.cs — Estadísticas para el panel de control
// ============================================================

namespace FrontendAdministrativo.DTOs
{
    /// <summary>
    /// Contiene los contadores de resumen mostrados en el Dashboard.
    /// </summary>
    public class DashboardStatsDTO
    {
        /// <summary>Total de partidos registrados</summary>
        public int TotalPartidos { get; set; }

        /// <summary>Partidos con estado "Finalizado"</summary>
        public int PartidosFinalizados { get; set; }

        /// <summary>Partidos con estado "Pendiente"</summary>
        public int PartidosPendientes { get; set; }

        /// <summary>Partidos con estado "EnCurso"</summary>
        public int PartidosEnCurso { get; set; }

        /// <summary>Total de selecciones registradas</summary>
        public int TotalSelecciones { get; set; }

        /// <summary>Total de usuarios registrados</summary>
        public int TotalUsuarios { get; set; }

        /// <summary>Total de usuarios activos</summary>
        public int UsuariosActivos { get; set; }

        /// <summary>Total de grupos del torneo</summary>
        public int TotalGrupos { get; set; }
    }
}
