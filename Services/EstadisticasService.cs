using FrontendAdministrativo.DTOs;
using Newtonsoft.Json;
using System.Text;

// ============================================================
// EstadisticasService.cs — Implementación del servicio de Estadísticas
// Consume la API REST del Servicio de Estadísticas via HttpClient
// ============================================================

namespace FrontendAdministrativo.Services
{
    /// <summary>
    /// Implementación concreta de IEstadisticasService.
    /// Realiza llamadas HTTP a la API de Estadísticas.
    /// </summary>
    public class EstadisticasService : IEstadisticasService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EstadisticasService> _logger;

        // Configuración JSON (Newtonsoft) — ignorar propiedades desconocidas
        private static readonly JsonSerializerSettings _jsonSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public EstadisticasService(HttpClient httpClient, ILogger<EstadisticasService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // ── HELPERS PRIVADOS ──────────────────────────────────────────────────

        /// <summary>Serializa un objeto como StringContent JSON (Newtonsoft).</summary>
        private static StringContent ToJsonContent(object obj)
        {
            var json = JsonConvert.SerializeObject(obj, _jsonSettings);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        /// <summary>Deserializa la respuesta HTTP como T usando Newtonsoft.</summary>
        private async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json, _jsonSettings);
        }

        // ── PARTIDOS ──────────────────────────────────────────────────────────

        /// <summary>Obtiene todos los partidos desde la API.</summary>
        public async Task<List<PartidoDTO>> GetPartidosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/partidos");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GetPartidosAsync: API respondió con {StatusCode}", response.StatusCode);
                    return new List<PartidoDTO>();
                }
                return await DeserializeAsync<List<PartidoDTO>>(response) ?? new List<PartidoDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener partidos desde la API.");
                return new List<PartidoDTO>();
            }
        }

        /// <summary>Obtiene un partido por ID.</summary>
        public async Task<PartidoDTO?> GetPartidoAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/partidos/{id}");
                if (!response.IsSuccessStatusCode) return null;
                return await DeserializeAsync<PartidoDTO>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el partido ID {Id}.", id);
                return null;
            }
        }

        /// <summary>Crea un nuevo partido en la API.</summary>
        public async Task<bool> CreatePartidoAsync(PartidoDTO partido)
        {
            try
            {
                var content = ToJsonContent(partido);
                var response = await _httpClient.PostAsync("api/partidos", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el partido.");
                return false;
            }
        }

        /// <summary>Actualiza un partido existente en la API.</summary>
        public async Task<bool> UpdatePartidoAsync(int id, PartidoDTO partido)
        {
            try
            {
                var content = ToJsonContent(partido);
                var response = await _httpClient.PutAsync($"api/partidos/{id}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el partido ID {Id}.", id);
                return false;
            }
        }

        /// <summary>Elimina un partido de la API.</summary>
        public async Task<bool> DeletePartidoAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/partidos/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el partido ID {Id}.", id);
                return false;
            }
        }

        /// <summary>Registra el resultado (goles) de un partido.</summary>
        public async Task<bool> RegistrarResultadoAsync(int partidoId, int golesLocal, int golesVisitante)
        {
            try
            {
                var resultado = new ResultadoDTO
                {
                    PartidoId = partidoId,
                    GolesLocal = golesLocal,
                    GolesVisitante = golesVisitante
                };
                var content = ToJsonContent(resultado);
                var response = await _httpClient.PostAsync($"api/partidos/{partidoId}/resultado", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar resultado del partido ID {Id}.", partidoId);
                return false;
            }
        }

        // ── SELECCIONES ───────────────────────────────────────────────────────

        /// <summary>Obtiene todas las selecciones nacionales.</summary>
        public async Task<List<SeleccionDTO>> GetSeleccionesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/selecciones");
                if (!response.IsSuccessStatusCode) return new List<SeleccionDTO>();
                return await DeserializeAsync<List<SeleccionDTO>>(response) ?? new List<SeleccionDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener selecciones.");
                return new List<SeleccionDTO>();
            }
        }

        // ── GRUPOS ────────────────────────────────────────────────────────────

        /// <summary>Obtiene los grupos del torneo.</summary>
        public async Task<List<GrupoDTO>> GetGruposAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/grupos");
                if (!response.IsSuccessStatusCode) return new List<GrupoDTO>();
                return await DeserializeAsync<List<GrupoDTO>>(response) ?? new List<GrupoDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener grupos.");
                return new List<GrupoDTO>();
            }
        }

        // ── USUARIOS ──────────────────────────────────────────────────────────

        /// <summary>Obtiene todos los usuarios del sistema.</summary>
        public async Task<List<UsuarioDTO>> GetUsuariosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/usuarios");
                if (!response.IsSuccessStatusCode) return new List<UsuarioDTO>();
                return await DeserializeAsync<List<UsuarioDTO>>(response) ?? new List<UsuarioDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios.");
                return new List<UsuarioDTO>();
            }
        }

        /// <summary>Actualiza los datos de un usuario (rol, estado activo).</summary>
        public async Task<bool> UpdateUsuarioAsync(int id, UsuarioDTO usuario)
        {
            try
            {
                var content = ToJsonContent(usuario);
                var response = await _httpClient.PutAsync($"api/usuarios/{id}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el usuario ID {Id}.", id);
                return false;
            }
        }

        /// <summary>Elimina un usuario del sistema.</summary>
        public async Task<bool> DeleteUsuarioAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/usuarios/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el usuario ID {Id}.", id);
                return false;
            }
        }

        // ── DASHBOARD ─────────────────────────────────────────────────────────

        /// <summary>
        /// Obtiene estadísticas generales calculando desde la API.
        /// Si la API expone un endpoint /dashboard/stats, se usa directamente.
        /// Si no, se calcula localmente a partir de partidos y usuarios.
        /// </summary>
        public async Task<DashboardStatsDTO> GetDashboardStatsAsync()
        {
            try
            {
                // Intentar endpoint dedicado de estadísticas
                var statsResponse = await _httpClient.GetAsync("api/dashboard/stats");
                if (statsResponse.IsSuccessStatusCode)
                {
                    return await DeserializeAsync<DashboardStatsDTO>(statsResponse) ?? new DashboardStatsDTO();
                }

                // Fallback: calcular localmente
                var partidos = await GetPartidosAsync();
                var usuarios = await GetUsuariosAsync();
                var selecciones = await GetSeleccionesAsync();
                var grupos = await GetGruposAsync();

                return new DashboardStatsDTO
                {
                    TotalPartidos = partidos.Count,
                    PartidosFinalizados = partidos.Count(p => p.Estado == "Finalizado"),
                    PartidosPendientes = partidos.Count(p => p.Estado == "Pendiente"),
                    PartidosEnCurso = partidos.Count(p => p.Estado == "EnCurso"),
                    TotalSelecciones = selecciones.Count,
                    TotalUsuarios = usuarios.Count,
                    UsuariosActivos = usuarios.Count(u => u.Activo),
                    TotalGrupos = grupos.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del dashboard.");
                return new DashboardStatsDTO();
            }
        }
    }
}
