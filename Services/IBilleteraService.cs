using FrontendAdministrativo.DTOs;

namespace FrontendAdministrativo.Services
{
    public interface IBilleteraService
    {
        Task<BilleteraDTO?> GetBilleteraByUsuarioAsync(int usuarioId);
        Task<bool> RecargarSaldoAsync(int usuarioId, decimal cantidad);
        Task<List<BilleteraDTO>> GetBilleterasAsync();
    }
}
