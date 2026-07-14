using FrontendAdministrativo.DTOs;
using FrontendAdministrativo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

// ============================================================
// UsuariosController.cs — Gestión de usuarios del sistema
// ============================================================

namespace FrontendAdministrativo.Controllers
{
    /// <summary>
    /// Permite al administrador ver, editar y eliminar usuarios.
    /// No permite crear usuarios (el registro es responsabilidad del usuario).
    /// </summary>
    [Authorize]
    public class UsuariosController : Controller
    {
        private readonly IEstadisticasService _estadisticasService;
        private readonly ILogger<UsuariosController> _logger;

        // Roles disponibles en el sistema
        private static readonly List<string> _roles = new() { "Administrador", "Usuario" };

        public UsuariosController(
            IEstadisticasService estadisticasService,
            ILogger<UsuariosController> logger)
        {
            _estadisticasService = estadisticasService;
            _logger = logger;
        }

        // ── GET: /Usuarios ────────────────────────────────────
        public async Task<IActionResult> Index(string? filtroRol, string? filtroBusqueda)
        {
            try
            {
                var usuarios = await _estadisticasService.GetUsuariosAsync();

                // Filtro por rol
                if (!string.IsNullOrEmpty(filtroRol))
                    usuarios = usuarios.Where(u => u.Rol == filtroRol).ToList();

                // Filtro por nombre o email (búsqueda de texto)
                if (!string.IsNullOrEmpty(filtroBusqueda))
                {
                    var busqueda = filtroBusqueda.ToLower();
                    usuarios = usuarios.Where(u =>
                        u.Nombre.ToLower().Contains(busqueda) ||
                        u.Email.ToLower().Contains(busqueda)).ToList();
                }

                ViewBag.FiltroRol = filtroRol;
                ViewBag.FiltroBusqueda = filtroBusqueda;
                ViewBag.Roles = _roles;

                return View(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar usuarios.");
                TempData["Error"] = "No se pudieron cargar los usuarios.";
                return View(new List<UsuarioDTO>());
            }
        }

        // ── GET: /Usuarios/Edit/5 ─────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            // Buscar usuario en la lista (la API de detalle puede variar)
            var usuarios = await _estadisticasService.GetUsuariosAsync();
            var usuario = usuarios.FirstOrDefault(u => u.Id == id);

            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new SelectList(_roles, usuario.Rol);
            return View(usuario);
        }

        // ── POST: /Usuarios/Edit/5 ────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuarioDTO usuario)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(_roles, usuario.Rol);
                return View(usuario);
            }

            var exito = await _estadisticasService.UpdateUsuarioAsync(id, usuario);
            if (exito)
            {
                TempData["Exito"] = $"Usuario '{usuario.Nombre}' actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "No se pudo actualizar el usuario.";
            ViewBag.Roles = new SelectList(_roles, usuario.Rol);
            return View(usuario);
        }

        // ── POST: /Usuarios/Delete/5 ──────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var exito = await _estadisticasService.DeleteUsuarioAsync(id);
            if (exito)
                TempData["Exito"] = "Usuario eliminado correctamente.";
            else
                TempData["Error"] = "No se pudo eliminar el usuario.";

            return RedirectToAction(nameof(Index));
        }
    }
}
