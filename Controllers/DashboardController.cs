using FrontendAdministrativo.DTOs;
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
                _logger.LogInformation("Cargando Dashboard...");

                // Obtener estadísticas generales desde la API
                var stats = await _estadisticasService.GetDashboardStatsAsync();

                // Obtener datos adicionales para la vista
                var partidos = await _estadisticasService.GetPartidosAsync();
                var selecciones = await _estadisticasService.GetSeleccionesAsync();
                var grupos = await _estadisticasService.GetGruposAsync();

                // Pasar datos a la vista usando ViewBag
                ViewBag.Stats = stats;
                ViewBag.Partidos = partidos;
                ViewBag.Selecciones = selecciones;
                ViewBag.Grupos = grupos;
                ViewBag.TotalPartidos = partidos?.Count ?? 0;
                ViewBag.TotalSelecciones = selecciones?.Count ?? 0;
                ViewBag.TotalGrupos = grupos?.Count ?? 0;

                // Pasar nombre del admin a la vista
                var username = HttpContext.Session.GetString("Username") ?? "Administrador";
                ViewBag.AdminNombre = username;

                _logger.LogInformation("Dashboard cargado correctamente. Partidos: {Partidos}, Selecciones: {Selecciones}, Grupos: {Grupos}",
                    partidos?.Count ?? 0, selecciones?.Count ?? 0, grupos?.Count ?? 0);

                return View(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el Dashboard.");
                TempData["Error"] = "No se pudo conectar con el servicio de estadísticas. Verifique que la API esté activa.";
                return View(new DashboardStatsDTO());
            }
        }
    }
}