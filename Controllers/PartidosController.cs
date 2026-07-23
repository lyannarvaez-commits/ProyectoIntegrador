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
        private static readonly List<string> _fasesList = new()
        {
            "GRUPOS",
            "DIECISEISAVOS",
            "OCTAVOS",
            "CUARTOS",
            "SEMIFINAL",
            "TERCER_PUESTO",
            "FINAL"
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

                ViewBag.FiltroEstado = filtroEstado;
                ViewBag.FiltroFase = filtroFase;

                //  LISTA DE FASES CON VALORES QUE COINCIDEN CON EL BACKEND
                var fases = new List<SelectListItem>
        {
            new SelectListItem { Value = "GRUPOS", Text = "Fase de grupos" },
            new SelectListItem { Value = "DIECISEISAVOS", Text = "Dieciseisavos de final" },
            new SelectListItem { Value = "OCTAVOS", Text = "Octavos de final" },
            new SelectListItem { Value = "CUARTOS", Text = "Cuartos de final" },
            new SelectListItem { Value = "SEMIFINAL", Text = "Semifinales" },
            new SelectListItem { Value = "TERCER_PUESTO", Text = "Tercer puesto" },
            new SelectListItem { Value = "FINAL", Text = "Final" }
        };
                ViewBag.Fases = fases;

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

            //  CARGAR EQUIPOS ELIMINADOS PARA MOSTRAR EN LA VISTA
            var todasLasSelecciones = await _estadisticasService.GetSeleccionesAsync();
            ViewBag.EquiposEliminados = todasLasSelecciones?
                .Where(s => s.Eliminado)
                .OrderBy(s => s.Nombre)
                .ToList() ?? new List<SeleccionDTO>();

            return View(new PartidoDTO
            {
                FechaHora = DateTime.Now.AddDays(7),
                Estado = "PROGRAMADO"
            });
        }

        // ── POST: /Partidos/Create ────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PartidoDTO partido)
        {
            _logger.LogInformation("Create POST - Iniciando creación de partido");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Create POST - ModelState inválido");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogWarning("Error de validación: {Error}", error.ErrorMessage);
                }
                await CargarSelectLists();
                return View(partido);
            }

            // Validar que local y visitante sean diferentes
            if (partido.EquipoLocalId == partido.EquipoVisitanteId)
            {
                _logger.LogWarning("Create POST - Local y visitante son iguales");
                ModelState.AddModelError("EquipoVisitanteId",
                    "La selección local y visitante no pueden ser la misma.");
                await CargarSelectLists();
                return View(partido);
            }

            // Validar que los IDs sean válidos
            if (partido.EquipoLocalId == 0 || partido.EquipoVisitanteId == 0 || partido.SedeId == 0)
            {
                _logger.LogWarning("Create POST - IDs inválidos: LocalId={LocalId}, VisitanteId={VisitanteId}, SedeId={SedeId}",
                    partido.EquipoLocalId, partido.EquipoVisitanteId, partido.SedeId);
                ModelState.AddModelError("", "Debe seleccionar todos los campos obligatorios.");
                await CargarSelectLists();
                return View(partido);
            }

            _logger.LogInformation("Create POST - Datos recibidos: LocalId={LocalId}, VisitanteId={VisitanteId}, SedeId={SedeId}, Fase={Fase}",
                partido.EquipoLocalId, partido.EquipoVisitanteId, partido.SedeId, partido.Fase);

            var exito = await _estadisticasService.CreatePartidoAsync(partido);
            if (exito)
            {
                _logger.LogInformation("Create POST - Partido creado exitosamente");
                TempData["Exito"] = "Partido creado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Create POST - Falló la creación del partido");
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

            //  BLOQUEAR EDICIÓN SI EL PARTIDO ESTÁ FINALIZADO
            if (partido.Estado == "FINALIZADO")
            {
                TempData["Error"] = "❌ No se puede editar un partido ya finalizado.";
                return RedirectToAction(nameof(Index));
            }

            //  CARGAR DATOS PARA LOS COMBOBOXES (SOLO SELECCIONES ACTIVAS)
            await CargarSelectLists();

            //  PRESELECCIONAR LOS VALORES DEL PARTIDO EN LOS COMBOBOXES
            ViewBag.SeleccionLocalId = partido.EquipoLocalId;
            ViewBag.SeleccionVisitanteId = partido.EquipoVisitanteId;
            ViewBag.SedeId = partido.SedeId;

            return View(partido);
        }

        // ── POST: /Partidos/Edit/5 ────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PartidoDTO partido)
        {
            //  ASEGURAR QUE EL ID DEL MODELO SEA EL CORRECTO
            partido.Id = id;

            _logger.LogInformation("Edit POST - ID: {Id}, LocalId: {LocalId}, VisitanteId: {VisitanteId}, SedeId: {SedeId}, Fase: {Fase}",
                id, partido.EquipoLocalId, partido.EquipoVisitanteId, partido.SedeId, partido.Fase);

            if (!ModelState.IsValid)
            {
                await CargarSelectLists();
                return View(partido);
            }

            // Validar que local y visitante sean diferentes
            if (partido.EquipoLocalId == partido.EquipoVisitanteId)
            {
                ModelState.AddModelError("EquipoVisitanteId",
                    "La selección local y visitante no pueden ser la misma.");
                await CargarSelectLists();
                return View(partido);
            }

            var exito = await _estadisticasService.UpdatePartidoAsync(id, partido);
            if (exito)
            {
                TempData["Exito"] = "Partido actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "No se pudo actualizar el partido.";
            await CargarSelectLists();
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
        private async Task CargarSelectLists()
        {
            try
            {
                //  OBTENER SELECCIONES Y FILTRAR ELIMINADAS
                var selecciones = await _estadisticasService.GetSeleccionesAsync();
                var seleccionesActivas = selecciones?.Where(s => !s.Eliminado).ToList() ?? new List<SeleccionDTO>();

                var sedes = await _estadisticasService.GetSedesAsync();

                _logger.LogInformation("CargarSelectLists - Selecciones activas: {Count}, Sedes: {Count2}",
                    seleccionesActivas.Count, sedes?.Count ?? 0);

                // Convertir a SelectListItem (SOLO SELECCIONES ACTIVAS)
                ViewBag.Selecciones = seleccionesActivas.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Nombre
                }).OrderBy(s => s.Text).ToList() ?? new List<SelectListItem>();

                ViewBag.Sedes = sedes?.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.Nombre} - {s.Ciudad}"
                }).OrderBy(s => s.Text).ToList() ?? new List<SelectListItem>();

                ViewBag.Fases = _fasesList.Select(f => new SelectListItem
                {
                    Value = f,
                    Text = ObtenerNombreFase(f)
                }).ToList();

                ViewBag.Estados = new List<SelectListItem>
        {
            new SelectListItem { Value = "PROGRAMADO", Text = "Programado" },
            new SelectListItem { Value = "EnCurso", Text = "En Curso" },
            new SelectListItem { Value = "FINALIZADO", Text = "Finalizado" }
        };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar SelectLists");
                ViewBag.Selecciones = new List<SelectListItem>();
                ViewBag.Sedes = new List<SelectListItem>();
                ViewBag.Fases = new List<SelectListItem>();
                ViewBag.Estados = new List<SelectListItem>();
            }
        }

        private string ObtenerNombreFase(string codigo)
        {
            return codigo switch
            {
                "GRUPOS" => "Fase de grupos",
                "DIECISEISAVOS" => "Dieciseisavos de final",
                "OCTAVOS" => "Octavos de final",
                "CUARTOS" => "Cuartos de final",
                "SEMIFINAL" => "Semifinales",
                "TERCER_PUESTO" => "Tercer puesto",
                "FINAL" => "Final",
                _ => codigo
            };
        }
    }
}