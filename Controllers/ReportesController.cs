using FrontendAdministrativo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// ============================================================
// ReportesController.cs — Reportes del panel administrativo
// ============================================================

namespace FrontendAdministrativo.Controllers
{
    /// <summary>
    /// Genera reportes estadísticos del Mundial 2026.
    /// </summary>
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly IEstadisticasService _estadisticasService;
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(
            IEstadisticasService estadisticasService,
            ILogger<ReportesController> logger)
        {
            _estadisticasService = estadisticasService;
            _logger = logger;
        }

        // ── GET: /Reportes ────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtener todos los datos necesarios para los reportes
                var partidos = await _estadisticasService.GetPartidosAsync();
                var usuarios = await _estadisticasService.GetUsuariosAsync();
                var selecciones = await _estadisticasService.GetSeleccionesAsync();
                var grupos = await _estadisticasService.GetGruposAsync();

                // ── Reporte de partidos por fase ──────────────
                var partidosPorFase = partidos
                    .GroupBy(p => p.Fase)
                    .Select(g => new { Fase = g.Key, Total = g.Count(), Finalizados = g.Count(p => p.Estado == "Finalizado") })
                    .OrderBy(r => r.Fase)
                    .ToList();

                // ── Top goleadores (selecciones con más goles) ─
                var topGoleadores = partidos
                    .Where(p => p.GolesLocal.HasValue && p.GolesVisitante.HasValue)
                    .SelectMany(p => new[]
                    {
                        new { Seleccion = p.SeleccionLocal, Goles = p.GolesLocal!.Value },
                        new { Seleccion = p.SeleccionVisitante, Goles = p.GolesVisitante!.Value }
                    })
                    .GroupBy(x => x.Seleccion)
                    .Select(g => new { Seleccion = g.Key, TotalGoles = g.Sum(x => x.Goles) })
                    .OrderByDescending(x => x.TotalGoles)
                    .Take(10)
                    .ToList();

                // Pasar datos a la vista via ViewBag
                ViewBag.TotalPartidos = partidos.Count;
                ViewBag.TotalFinalizados = partidos.Count(p => p.Estado == "Finalizado");
                ViewBag.TotalPendientes = partidos.Count(p => p.Estado == "Pendiente");
                ViewBag.TotalEnCurso = partidos.Count(p => p.Estado == "EnCurso");
                ViewBag.TotalUsuarios = usuarios.Count;
                ViewBag.UsuariosActivos = usuarios.Count(u => u.Activo);
                ViewBag.TotalSelecciones = selecciones.Count;
                ViewBag.TotalGrupos = grupos.Count;
                ViewBag.PartidosPorFase = partidosPorFase;
                ViewBag.TopGoleadores = topGoleadores;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar reportes.");
                TempData["Error"] = "No se pudieron generar los reportes. Verifique la conexión con la API.";
                return View();
            }
        }
    }
}
