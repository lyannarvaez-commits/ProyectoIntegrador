using FrontendAdministrativo.DTOs;
using FrontendAdministrativo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

// ============================================================
// PartidosController.cs — Gestión de partidos del Mundial 2026
// ============================================================

namespace FrontendAdministrativo.Controllers
{
    /// <summary>
    /// CRUD completo de partidos y registro de resultados.
    /// Todos los datos se obtienen y guardan via API REST.
    /// </summary>
    [Authorize]
    public class PartidosController : Controller
    {
        private readonly IEstadisticasService _estadisticasService;
        private readonly ILogger<PartidosController> _logger;

        // Fases del torneo disponibles
        private static readonly List<string> _fases = new()
        {
            "Fase de Grupos", "Octavos de Final", "Cuartos de Final",
            "Semifinal", "Tercer Puesto", "Final"
        };

        // Sedes del Mundial 2026
        private static readonly List<string> _sedes = new()
        {
            "MetLife Stadium - Nueva York",
            "AT&T Stadium - Dallas",
            "SoFi Stadium - Los Ángeles",
            "Levi's Stadium - San Francisco",
            "Arrowhead Stadium - Kansas City",
            "Gillette Stadium - Boston",
            "Lincoln Financial Field - Filadelfia",
            "Hard Rock Stadium - Miami",
            "NRG Stadium - Houston",
            "Estadio Azteca - Ciudad de México",
            "Estadio BBVA - Monterrey",
            "Estadio Akron - Guadalajara",
            "BC Place - Vancouver",
            "BMO Field - Toronto",
            "Estadio Olímpico - Ciudad de México"
        };

        public PartidosController(
            IEstadisticasService estadisticasService,
            ILogger<PartidosController> logger)
        {
            _estadisticasService = estadisticasService;
            _logger = logger;
        }

        // ── GET: /Partidos ────────────────────────────────────
        public async Task<IActionResult> Index(string? filtroEstado, string? filtroFase)
        {
            try
            {
                var partidos = await _estadisticasService.GetPartidosAsync();

                // Aplicar filtros si se enviaron
                if (!string.IsNullOrEmpty(filtroEstado))
                    partidos = partidos.Where(p => p.Estado == filtroEstado).ToList();

                if (!string.IsNullOrEmpty(filtroFase))
                    partidos = partidos.Where(p => p.Fase == filtroFase).ToList();

                // Pasar datos de filtros a la vista
                ViewBag.FiltroEstado = filtroEstado;
                ViewBag.FiltroFase = filtroFase;
                ViewBag.Fases = _fases;

                return View(partidos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar partidos.");
                TempData["Error"] = "No se pudieron cargar los partidos.";
                return View(new List<PartidoDTO>());
            }
        }

        // ── GET: /Partidos/Details/5 ──────────────────────────
        public async Task<IActionResult> Details(int id)
        {
            var partido = await _estadisticasService.GetPartidoAsync(id);
            if (partido == null)
            {
                TempData["Error"] = "Partido no encontrado.";
                return RedirectToAction(nameof(Index));
            }
            return View(partido);
        }

        // ── GET: /Partidos/Create ─────────────────────────────
        public async Task<IActionResult> Create()
        {
            await CargarSelectLists();
            return View(new PartidoDTO { FechaHora = DateTime.Now.AddDays(7) });
        }

        // ── POST: /Partidos/Create ────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PartidoDTO partido)
        {
            if (!ModelState.IsValid)
            {
                await CargarSelectLists();
                return View(partido);
            }

            // Validar que local y visitante sean diferentes
            if (partido.SeleccionLocal == partido.SeleccionVisitante)
            {
                ModelState.AddModelError("SeleccionVisitante",
                    "La selección local y visitante no pueden ser la misma.");
                await CargarSelectLists();
                return View(partido);
            }

            var exito = await _estadisticasService.CreatePartidoAsync(partido);
            if (exito)
            {
                TempData["Exito"] = "Partido creado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "No se pudo crear el partido. Verifique que la API esté activa.";
            await CargarSelectLists();
            return View(partido);
        }

        // ── GET: /Partidos/Edit/5 ─────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            var partido = await _estadisticasService.GetPartidoAsync(id);
            if (partido == null)
            {
                TempData["Error"] = "Partido no encontrado.";
                return RedirectToAction(nameof(Index));
            }
            await CargarSelectLists(partido.SeleccionLocal, partido.SeleccionVisitante);
            return View(partido);
        }

        // ── POST: /Partidos/Edit/5 ────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PartidoDTO partido)
        {
            if (!ModelState.IsValid)
            {
                await CargarSelectLists(partido.SeleccionLocal, partido.SeleccionVisitante);
                return View(partido);
            }

            var exito = await _estadisticasService.UpdatePartidoAsync(id, partido);
            if (exito)
            {
                TempData["Exito"] = "Partido actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "No se pudo actualizar el partido.";
            await CargarSelectLists(partido.SeleccionLocal, partido.SeleccionVisitante);
            return View(partido);
        }

        // ── POST: /Partidos/Delete/5 ──────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var exito = await _estadisticasService.DeletePartidoAsync(id);
            if (exito)
                TempData["Exito"] = "Partido eliminado correctamente.";
            else
                TempData["Error"] = "No se pudo eliminar el partido.";

            return RedirectToAction(nameof(Index));
        }

        // ── GET: /Partidos/RegistrarResultado/5 ───────────────
        public async Task<IActionResult> RegistrarResultado(int id)
        {
            var partido = await _estadisticasService.GetPartidoAsync(id);
            if (partido == null)
            {
                TempData["Error"] = "Partido no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var resultado = new ResultadoDTO
            {
                PartidoId = id,
                GolesLocal = partido.GolesLocal ?? 0,
                GolesVisitante = partido.GolesVisitante ?? 0
            };

            ViewBag.Partido = partido;
            return View(resultado);
        }

        // ── POST: /Partidos/RegistrarResultado/5 ──────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarResultado(int id, ResultadoDTO resultado)
        {
            if (!ModelState.IsValid)
            {
                var partido = await _estadisticasService.GetPartidoAsync(id);
                ViewBag.Partido = partido;
                return View(resultado);
            }

            var exito = await _estadisticasService.RegistrarResultadoAsync(
                id, resultado.GolesLocal, resultado.GolesVisitante);

            if (exito)
            {
                TempData["Exito"] = $"Resultado registrado: {resultado.GolesLocal} - {resultado.GolesVisitante}";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "No se pudo registrar el resultado.";
            var p = await _estadisticasService.GetPartidoAsync(id);
            ViewBag.Partido = p;
            return View(resultado);
        }

        // ── HELPER: Cargar SelectLists para formularios ────────
        private async Task CargarSelectLists(string? localSeleccionado = null, string? visitanteSeleccionado = null)
        {
            var selecciones = await _estadisticasService.GetSeleccionesAsync();
            var nombresSelecciones = selecciones.Select(s => s.Nombre).OrderBy(n => n).ToList();

            ViewBag.Selecciones = new SelectList(nombresSelecciones, localSeleccionado);
            ViewBag.SeleccionesVisitante = new SelectList(nombresSelecciones, visitanteSeleccionado);
            ViewBag.Fases = new SelectList(_fases);
            ViewBag.Sedes = new SelectList(_sedes);
            ViewBag.Estados = new SelectList(new[] { "Pendiente", "EnCurso", "Finalizado" });
        }
    }
}
