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
                var emptyContent = new StringContent("{}", Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"billeteras/usuario/{usuarioId}/recargar", emptyContent);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Recarga exitosa para usuario {usuarioId}");
                    return true;
                }
                _logger.LogWarning($"Error al recargar saldo para el usuario {usuarioId}. Status: {response.StatusCode}");
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
