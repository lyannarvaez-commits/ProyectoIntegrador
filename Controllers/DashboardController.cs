using FrontendAdministrativo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// ============================================================
// DashboardController.cs — Panel de control principal
// ============================================================

namespace FrontendAdministrativo.Controllers
{
    /// <summary>
    /// Muestra las estadísticas generales del Mundial 2026.
    /// Requiere autenticación de administrador.
    /// </summary>
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IEstadisticasService _estadisticasService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IEstadisticasService estadisticasService,
            ILogger<DashboardController> logger)
        {
            _estadisticasService = estadisticasService;
            _logger = logger;
        }

        // ── GET: /Dashboard ───────────────────────────────────
        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtener estadísticas generales desde la API
                var stats = await _estadisticasService.GetDashboardStatsAsync();

                // Pasar nombre del admin a la vista
                ViewData["AdminNombre"] = HttpContext.Session.GetString("AdminNombre") ?? "Administrador";

                return View(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el Dashboard.");
                TempData["Error"] = "No se pudo conectar con el servicio de estadísticas. Verifique que la API esté activa.";
                return View(new FrontendAdministrativo.DTOs.DashboardStatsDTO());
            }
        }
    }
}
