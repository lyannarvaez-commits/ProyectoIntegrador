using FrontendAdministrativo.DTOs;
using FrontendAdministrativo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrontendAdministrativo.Controllers
{
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

        public async Task<IActionResult> Index()
        {
            try
            {
                var partidos = await _estadisticasService.GetPartidosAsync();
                var selecciones = await _estadisticasService.GetSeleccionesAsync();
                var grupos = await _estadisticasService.GetGruposAsync();
                var usuarios = await _estadisticasService.GetUsuariosAsync();

                // ── Estadísticas por estado ──────────────────────
                var totalPartidos = partidos.Count;
                var finalizados = partidos.Count(p => p.Estado == "FINALIZADO");
                var enCurso = partidos.Count(p => p.Estado == "EnCurso");
                var programados = partidos.Count(p => p.Estado == "PROGRAMADO");

                // ── Estadísticas por fase ────────────────────────
                var fases = partidos
                    .GroupBy(p => p.Fase ?? "Sin fase")
                    .Select(g => new FaseReporteDTO
                    {
                        Fase = g.Key,
                        Total = g.Count(),
                        Finalizados = g.Count(p => p.Estado == "FINALIZADO"),
                        Porcentaje = g.Count() > 0
                            ? (int)Math.Round((double)g.Count(p => p.Estado == "FINALIZADO") / g.Count() * 100)
                            : 0
                    })
                    .ToList();

                // ── Top Goleadores ──────────────────────────────
                var goleadores = partidos
                    .Where(p => p.GolesLocal.HasValue && p.GolesVisitante.HasValue)
                    .GroupBy(p => p.SeleccionLocal)
                    .Select(g => new GoleadorDTO
                    {
                        Seleccion = g.Key,
                        TotalGoles = g.Sum(p => p.GolesLocal ?? 0)
                    })
                    .Union(
                        partidos
                            .Where(p => p.GolesLocal.HasValue && p.GolesVisitante.HasValue)
                            .GroupBy(p => p.SeleccionVisitante)
                            .Select(g => new GoleadorDTO
                            {
                                Seleccion = g.Key,
                                TotalGoles = g.Sum(p => p.GolesVisitante ?? 0)
                            })
                    )
                    .GroupBy(g => g.Seleccion)
                    .Select(g => new GoleadorDTO
                    {
                        Seleccion = g.Key,
                        TotalGoles = g.Sum(x => x.TotalGoles)
                    })
                    .OrderByDescending(g => g.TotalGoles)
                    .Take(10)
                    .ToList();

                // ── Pasar datos a la vista ──────────────────────
                ViewBag.TotalPartidos = totalPartidos;
                ViewBag.Finalizados = finalizados;
                ViewBag.EnCurso = enCurso;
                ViewBag.Programados = programados;
                ViewBag.TotalSelecciones = selecciones.Count;
                ViewBag.TotalGrupos = grupos.Count;
                ViewBag.TotalUsuarios = usuarios.Count;
                ViewBag.UsuariosActivos = usuarios.Count(u => u.Activo);
                ViewBag.Fases = fases;
                ViewBag.TopGoleadores = goleadores;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar reportes.");
                ViewBag.TotalPartidos = 0;
                ViewBag.Finalizados = 0;
                ViewBag.EnCurso = 0;
                ViewBag.Programados = 0;
                ViewBag.TotalSelecciones = 0;
                ViewBag.TotalGrupos = 0;
                ViewBag.TotalUsuarios = 0;
                ViewBag.UsuariosActivos = 0;
                ViewBag.Fases = new List<FaseReporteDTO>();
                ViewBag.TopGoleadores = new List<GoleadorDTO>();
                ViewBag.Error = "No se pudieron cargar los datos.";
                return View();
            }
        }
    }
}