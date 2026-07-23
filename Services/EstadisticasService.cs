using FrontendAdministrativo.DTOs;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

// ============================================================
// EstadisticasService.cs — Implementación del servicio de Estadísticas
// Consume la API REST del Servicio de Estadísticas via HttpClient
// ============================================================

namespace FrontendAdministrativo.Services
{
    public class EstadisticasService : IEstadisticasService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EstadisticasService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly JsonSerializerSettings _jsonSettings = new()
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public EstadisticasService(
            HttpClient httpClient,
            ILogger<EstadisticasService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        // ── HELPERS ──────────────────────────────────────────────────

        private void AddAuthorizationHeader()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");

                _logger.LogInformation("AddAuthorizationHeader - Token en sesión: {Status}",
                    string.IsNullOrEmpty(token) ? "NO HAY TOKEN" : "TOKEN PRESENTE");

                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                    _logger.LogInformation("✅ Token agregado al header correctamente.");
                }
                else
                {
                    _logger.LogWarning("❌ No se encontró token en la sesión.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar token al header");
            }
        }

        private static StringContent ToJsonContent(object obj)
        {
            var json = JsonConvert.SerializeObject(obj, _jsonSettings);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        private async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json, _jsonSettings);
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
                    return temp["nombre"]?.ToString() ?? string.Empty;
                if (temp != null && temp.ContainsKey("codigo"))
                    return temp["codigo"]?.ToString() ?? string.Empty;
            }
            catch { }

            return obj.ToString() ?? string.Empty;
        }

        private string ObtenerNombreSeleccion(Dictionary<string, object> item, string key)
        {
            if (!item.ContainsKey(key)) return string.Empty;

            var obj = item[key];
            if (obj == null) return string.Empty;

            if (obj is Newtonsoft.Json.Linq.JObject jObj)
            {
                var nombre = jObj["nombre"]?.ToString();
                if (!string.IsNullOrEmpty(nombre)) return nombre;
                var codigo = jObj["codigoFifa"]?.ToString();
                return codigo ?? string.Empty;
            }

            return obj.ToString() ?? string.Empty;
        }

        // ── PARTIDOS ──────────────────────────────────────────────────

        public async Task<List<PartidoDTO>> GetPartidosAsync()
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync("api/partidos");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GetPartidosAsync: API respondió con {StatusCode}", response.StatusCode);
                    return new List<PartidoDTO>();
                }

                var json = await response.Content.ReadAsStringAsync();
                var rawData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);

                if (rawData == null) return new List<PartidoDTO>();

                var partidos = new List<PartidoDTO>();
                foreach (var item in rawData)
                {
                    var partido = new PartidoDTO
                    {
                        Id = Convert.ToInt32(item["id"]),
                        SeleccionLocal = ObtenerNombreSeleccion(item, "equipoLocal"),
                        SeleccionVisitante = ObtenerNombreSeleccion(item, "equipoVisitante"),
                        //  USAR faseCodigo (igual que en GetPartidoAsync)
                        Fase = item.ContainsKey("faseCodigo") ? item["faseCodigo"]?.ToString() ?? string.Empty : ExtraerNombre(item["fase"]),
                        Sede = ExtraerNombre(item["sede"]),
                        Estado = item.ContainsKey("estado") ? item["estado"]?.ToString() ?? "PROGRAMADO" : "PROGRAMADO"
                    };

                    // Manejar IDs
                    if (item.ContainsKey("equipoLocalId"))
                        partido.EquipoLocalId = Convert.ToInt32(item["equipoLocalId"]);
                    else if (item.ContainsKey("equipoLocal") && item["equipoLocal"] is Newtonsoft.Json.Linq.JObject jLocal)
                        partido.EquipoLocalId = jLocal["id"]?.ToObject<int>() ?? 0;

                    if (item.ContainsKey("equipoVisitanteId"))
                        partido.EquipoVisitanteId = Convert.ToInt32(item["equipoVisitanteId"]);
                    else if (item.ContainsKey("equipoVisitante") && item["equipoVisitante"] is Newtonsoft.Json.Linq.JObject jVisitante)
                        partido.EquipoVisitanteId = jVisitante["id"]?.ToObject<int>() ?? 0;

                    if (item.ContainsKey("sedeId"))
                        partido.SedeId = Convert.ToInt32(item["sedeId"]);
                    else if (item.ContainsKey("sede") && item["sede"] is Newtonsoft.Json.Linq.JObject jSede)
                        partido.SedeId = jSede["id"]?.ToObject<int>() ?? 0;

                    // Manejar fecha
                    if (item.ContainsKey("fechaHoraUtc"))
                        partido.FechaHora = DateTime.Parse(item["fechaHoraUtc"]?.ToString() ?? DateTime.Now.ToString());
                    else if (item.ContainsKey("fechaHora"))
                        partido.FechaHora = DateTime.Parse(item["fechaHora"]?.ToString() ?? DateTime.Now.ToString());
                    else
                        partido.FechaHora = DateTime.Now;

                    // Manejar goles
                    if (item.ContainsKey("golesLocal") && item["golesLocal"] != null)
                        partido.GolesLocal = Convert.ToInt32(item["golesLocal"]);
                    if (item.ContainsKey("golesVisitante") && item["golesVisitante"] != null)
                        partido.GolesVisitante = Convert.ToInt32(item["golesVisitante"]);

                    partidos.Add(partido);
                }

                _logger.LogInformation("GetPartidosAsync: {Count} partidos obtenidos.", partidos.Count);
                return partidos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener partidos.");
                return new List<PartidoDTO>();
            }
        }

        public async Task<PartidoDTO?> GetPartidoAsync(int id)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync($"api/partidos/{id}");
                if (!response.IsSuccessStatusCode) return null;

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("GetPartidoAsync JSON: {Json}", json);

                var item = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (item == null) return null;

                var partido = new PartidoDTO
                {
                    Id = Convert.ToInt32(item["id"]),
                    SeleccionLocal = ObtenerNombreSeleccion(item, "equipoLocal"),
                    SeleccionVisitante = ObtenerNombreSeleccion(item, "equipoVisitante"),
                    Estado = item.ContainsKey("estado") ? item["estado"]?.ToString() ?? "PROGRAMADO" : "PROGRAMADO"
                };

                //  ASIGNAR FASE CORRECTAMENTE
                if (item.ContainsKey("faseCodigo"))
                {
                    partido.Fase = item["faseCodigo"]?.ToString() ?? string.Empty;
                }
                else if (item.ContainsKey("fase"))
                {
                    partido.Fase = ExtraerNombre(item["fase"]);
                }
                else
                {
                    partido.Fase = string.Empty;
                }

                // ASIGNAR SEDE
                if (item.ContainsKey("sede"))
                {
                    partido.Sede = ExtraerNombre(item["sede"]);
                }

                // ASIGNAR IDs
                if (item.ContainsKey("equipoLocalId"))
                {
                    partido.EquipoLocalId = Convert.ToInt32(item["equipoLocalId"]);
                }
                else if (item.ContainsKey("equipoLocal") && item["equipoLocal"] is Newtonsoft.Json.Linq.JObject jLocal)
                {
                    partido.EquipoLocalId = jLocal["id"]?.ToObject<int>() ?? 0;
                }

                if (item.ContainsKey("equipoVisitanteId"))
                {
                    partido.EquipoVisitanteId = Convert.ToInt32(item["equipoVisitanteId"]);
                }
                else if (item.ContainsKey("equipoVisitante") && item["equipoVisitante"] is Newtonsoft.Json.Linq.JObject jVisitante)
                {
                    partido.EquipoVisitanteId = jVisitante["id"]?.ToObject<int>() ?? 0;
                }

                if (item.ContainsKey("sedeId"))
                {
                    partido.SedeId = Convert.ToInt32(item["sedeId"]);
                }
                else if (item.ContainsKey("sede") && item["sede"] is Newtonsoft.Json.Linq.JObject jSede)
                {
                    partido.SedeId = jSede["id"]?.ToObject<int>() ?? 0;
                }

                // Manejar fecha
                if (item.ContainsKey("fechaHoraUtc"))
                    partido.FechaHora = DateTime.Parse(item["fechaHoraUtc"]?.ToString() ?? DateTime.Now.ToString());
                else if (item.ContainsKey("fechaHora"))
                    partido.FechaHora = DateTime.Parse(item["fechaHora"]?.ToString() ?? DateTime.Now.ToString());
                else
                    partido.FechaHora = DateTime.Now;

                // Manejar goles
                if (item.ContainsKey("golesLocal") && item["golesLocal"] != null)
                    partido.GolesLocal = Convert.ToInt32(item["golesLocal"]);
                if (item.ContainsKey("golesVisitante") && item["golesVisitante"] != null)
                    partido.GolesVisitante = Convert.ToInt32(item["golesVisitante"]);

                return partido;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener partido {Id}.", id);
                return null;
            }
        }

        public async Task<bool> CreatePartidoAsync(PartidoDTO partido)
        {
            try
            {
                _logger.LogInformation("CreatePartidoAsync - Iniciando...");

                //  VERIFICAR TOKEN ANTES DE ENVIAR
                var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");
                _logger.LogInformation("Token en sesión: {Status}", string.IsNullOrEmpty(token) ? "NO HAY TOKEN" : "TOKEN PRESENTE");

                AddAuthorizationHeader();

                var requestBody = new
                {
                    faseCodigo = partido.Fase,
                    grupoCodigo = (string?)null,
                    sedeId = partido.SedeId,
                    equipoLocalId = partido.EquipoLocalId,
                    equipoVisitanteId = partido.EquipoVisitanteId,
                    fechaHoraUtc = partido.FechaHora.ToString("yyyy-MM-ddTHH:mm:ss"),
                    estado = partido.Estado ?? "PROGRAMADO",
                    golesLocal = partido.GolesLocal,
                    golesVisitante = partido.GolesVisitante
                };

                var json = JsonConvert.SerializeObject(requestBody);
                _logger.LogInformation("CreatePartidoAsync - JSON: {Json}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("api/partidos", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("CreatePartidoAsync - Error {StatusCode}: {Error}", response.StatusCode, error);
                    return false;
                }

                _logger.LogInformation("CreatePartidoAsync - ¡PARTIDO CREADO EXITOSAMENTE!");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear partido.");
                return false;
            }
        }

        public async Task<bool> UpdatePartidoAsync(int id, PartidoDTO partido)
        {
            try
            {
                AddAuthorizationHeader();

                //  CREAR EL OBJETO EXACTO QUE ESPERA EL BACKEND
                var requestBody = new
                {
                    id = id,
                    faseCodigo = partido.Fase,
                    grupoCodigo = (string?)null,
                    sedeId = partido.SedeId,
                    equipoLocalId = partido.EquipoLocalId,
                    equipoVisitanteId = partido.EquipoVisitanteId,
                    fechaHoraUtc = partido.FechaHora.ToString("yyyy-MM-ddTHH:mm:ss"),
                    estado = partido.Estado ?? "PROGRAMADO",
                    golesLocal = partido.GolesLocal,
                    golesVisitante = partido.GolesVisitante
                };

                var json = JsonConvert.SerializeObject(requestBody);
                _logger.LogInformation("UpdatePartidoAsync - JSON enviado: {Json}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"api/partidos/{id}", content);

                //  LEER EL ERROR PARA DIAGNÓSTICO
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("UpdatePartidoAsync - Error {StatusCode}: {Error}",
                        response.StatusCode, errorContent);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar partido {Id}.", id);
                return false;
            }
        }

        public async Task<bool> DeletePartidoAsync(int id)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.DeleteAsync($"api/partidos/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar partido {Id}.", id);
                return false;
            }
        }

        public async Task<bool> RegistrarResultadoAsync(int partidoId, int golesLocal, int golesVisitante)
        {
            try
            {
                //  PRIMERO AGREGAR EL TOKEN
                AddAuthorizationHeader();

                //  VERIFICAR QUE EL TOKEN SE AGREGÓ
                var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");
                _logger.LogInformation("RegistrarResultadoAsync - Token presente: {Status}",
                    string.IsNullOrEmpty(token) ? "NO" : "SI");

                var requestBody = new
                {
                    golesLocal = golesLocal,
                    golesVisitante = golesVisitante
                };

                var json = JsonConvert.SerializeObject(requestBody);
                _logger.LogInformation("RegistrarResultadoAsync - JSON enviado: {Json}", json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                //  HACER LA PETICIÓN PUT
                var response = await _httpClient.PutAsync($"api/partidos/{partidoId}/resultado", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("RegistrarResultadoAsync - Error {StatusCode}: {Error}",
                        response.StatusCode, errorContent);
                    return false;
                }

                _logger.LogInformation("RegistrarResultadoAsync - Resultado registrado exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar resultado del partido ID {Id}.", partidoId);
                return false;
            }
        }

        // ── SELECCIONES ──────────────────────────────────────────────────

        public async Task<List<SeleccionDTO>> GetSeleccionesAsync()
        {
            try
            {
                AddAuthorizationHeader();

                var response = await _httpClient.GetAsync("api/selecciones");
                if (!response.IsSuccessStatusCode) return new List<SeleccionDTO>();

                var json = await response.Content.ReadAsStringAsync();
                var rawData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);

                if (rawData == null) return new List<SeleccionDTO>();

                var selecciones = new List<SeleccionDTO>();
                foreach (var item in rawData)
                {
                    var seleccion = new SeleccionDTO
                    {
                        Id = Convert.ToInt32(item["id"]),
                        Nombre = item["nombre"]?.ToString() ?? string.Empty,
                        CodigoFifa = item["codigoFifa"]?.ToString() ?? string.Empty,
                        Confederacion = item["confederacion"]?.ToString() ?? string.Empty,
                        Grupo = ExtraerNombre(item["grupo"]),
                        //  AGREGAR EL CAMPO ELIMINADO
                        Eliminado = item.ContainsKey("eliminado") && Convert.ToBoolean(item["eliminado"])
                    };
                    selecciones.Add(seleccion);
                }

                return selecciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener selecciones.");
                return new List<SeleccionDTO>();
            }
        }

        // ── GRUPOS ──────────────────────────────────────────────────

        public async Task<List<GrupoDTO>> GetGruposAsync()
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync("api/grupos");
                if (!response.IsSuccessStatusCode) return new List<GrupoDTO>();

                var json = await response.Content.ReadAsStringAsync();
                var rawData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
                if (rawData == null) return new List<GrupoDTO>();

                var grupos = new List<GrupoDTO>();
                foreach (var item in rawData)
                {
                    grupos.Add(new GrupoDTO
                    {
                        Id = item.ContainsKey("id") ? Convert.ToInt32(item["id"]) : 0,
                        Codigo = item["codigo"]?.ToString() ?? string.Empty,
                        Nombre = item["nombre"]?.ToString() ?? string.Empty
                    });
                }

                return grupos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener grupos.");
                return new List<GrupoDTO>();
            }
        }

        // ── SEDES ──────────────────────────────────────────────────

        public async Task<List<SedeDTO>> GetSedesAsync()
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync("api/sedes");
                if (!response.IsSuccessStatusCode) return new List<SedeDTO>();

                var json = await response.Content.ReadAsStringAsync();
                var rawData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
                if (rawData == null) return new List<SedeDTO>();

                var sedes = new List<SedeDTO>();
                foreach (var item in rawData)
                {
                    sedes.Add(new SedeDTO
                    {
                        Id = Convert.ToInt32(item["id"]),
                        Nombre = item["nombre"]?.ToString() ?? string.Empty,
                        Ciudad = item["ciudad"]?.ToString() ?? string.Empty,
                        Pais = item["pais"]?.ToString() ?? string.Empty,
                        CapacidadAprox = item.ContainsKey("capacidadAprox") ? Convert.ToInt32(item["capacidadAprox"]) : 0
                    });
                }

                return sedes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sedes.");
                return new List<SedeDTO>();
            }
        }

        // ── USUARIOS ──────────────────────────────────────────────────

        public async Task<List<UsuarioDTO>> GetUsuariosAsync()
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync("api/usuarios");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("GetUsuariosAsync: API respondió con {StatusCode}", response.StatusCode);
                    return new List<UsuarioDTO>();
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("GetUsuariosAsync - JSON recibido: {Json}", json);

                var usuarios = JsonConvert.DeserializeObject<List<UsuarioDTO>>(json, _jsonSettings);
                return usuarios ?? new List<UsuarioDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios.");
                return new List<UsuarioDTO>();
            }
        }

        public async Task<bool> UpdateUsuarioAsync(int id, UsuarioDTO usuario)
        {
            try
            {
                AddAuthorizationHeader();
                var content = ToJsonContent(usuario);
                var response = await _httpClient.PutAsync($"api/usuarios/{id}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario {Id}.", id);
                return false;
            }
        }

        public async Task<bool> DeleteUsuarioAsync(int id)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.DeleteAsync($"api/usuarios/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario {Id}.", id);
                return false;
            }
        }

        // ── DASHBOARD ──────────────────────────────────────────────────

        public async Task<DashboardStatsDTO> GetDashboardStatsAsync()
        {
            try
            {
                AddAuthorizationHeader();
                var statsResponse = await _httpClient.GetAsync("api/dashboard/stats");
                if (statsResponse.IsSuccessStatusCode)
                    return await DeserializeAsync<DashboardStatsDTO>(statsResponse) ?? new DashboardStatsDTO();

                var partidos = await GetPartidosAsync();
                var usuarios = await GetUsuariosAsync();
                var selecciones = await GetSeleccionesAsync();
                var grupos = await GetGruposAsync();

                return new DashboardStatsDTO
                {
                    TotalPartidos = partidos.Count,
                    PartidosFinalizados = partidos.Count(p => p.Estado == "FINALIZADO"),
                    PartidosPendientes = partidos.Count(p => p.Estado == "PROGRAMADO"),
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