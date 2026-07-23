using FrontendAdministrativo.DTOs;
using Newtonsoft.Json;
using System.Text;

namespace FrontendAdministrativo.Services
{
    public class BilleteraService : IBilleteraService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BilleteraService> _logger;

        public BilleteraService(HttpClient httpClient, ILogger<BilleteraService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<BilleteraDTO>> GetBilleterasAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("billeteras");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<BilleteraDTO>>(content) ?? new List<BilleteraDTO>();
                }
                else
                {
                    _logger.LogWarning($"GetBilleterasAsync retornó status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las billeteras");
            }
            return new List<BilleteraDTO>();
        }

        public async Task<BilleteraDTO?> GetBilleteraByUsuarioAsync(int usuarioId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"billeteras/usuario/{usuarioId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<BilleteraDTO>(content);
                }
                else
                {
                    _logger.LogWarning($"GetBilleteraByUsuarioAsync retornó status: {response.StatusCode} para usuario {usuarioId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener billetera para usuario {usuarioId}");
            }
            return null;
        }

        public async Task<bool> RecargarSaldoAsync(int usuarioId, decimal cantidad)
        {
            try
            {
                // Obtenemos la billetera para saber el saldo actual y el ID
                var billetera = await GetBilleteraByUsuarioAsync(usuarioId);
                
                if (billetera == null)
                {
                    // Si no tiene billetera, intentamos crearla
                    billetera = new BilleteraDTO { UsuarioId = usuarioId, Saldo = cantidad };
                    var jsonCreate = JsonConvert.SerializeObject(billetera);
                    var contentCreate = new StringContent(jsonCreate, Encoding.UTF8, "application/json");
                    var resCreate = await _httpClient.PostAsync("billeteras", contentCreate);
                    return resCreate.IsSuccessStatusCode;
                }

                billetera.Saldo += cantidad;
                var json = JsonConvert.SerializeObject(billetera);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Intentar TODAS las rutas y verbos posibles para máxima compatibilidad
                var attempts = new List<Func<Task<HttpResponseMessage>>>
                {
                    () => _httpClient.PutAsync($"billeteras/{billetera.Id}", content),
                    () => _httpClient.PutAsync($"billeteras/usuario/{usuarioId}", content),
                    () => _httpClient.PutAsync("billeteras", content),

                    () => _httpClient.PostAsync($"billeteras/{billetera.Id}", content),
                    () => _httpClient.PostAsync($"billeteras/usuario/{usuarioId}", content),
                    () => _httpClient.PostAsync("billeteras", content),
                    
                    () => _httpClient.PatchAsync($"billeteras/{billetera.Id}", content),
                    
                    () => _httpClient.PutAsync($"billeteras/{billetera.Id}/saldo", content),
                    () => _httpClient.PostAsync($"billeteras/recargar/{billetera.Id}", content)
                };

                foreach (var attempt in attempts)
                {
                    try
                    {
                        var response = await attempt();
                        if (response.IsSuccessStatusCode)
                        {
                            _logger.LogInformation($"Recarga exitosa para usuario {usuarioId}");
                            return true;
                        }
                    }
                    catch { /* Ignorar errores de conexión por intento */ }
                }

                _logger.LogWarning($"Todas las rutas fallaron al recargar saldo para el usuario {usuarioId}.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al recargar saldo para usuario {usuarioId}");
                return false;
            }
        }
    }
}
