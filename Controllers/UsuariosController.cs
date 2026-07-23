using FrontendAdministrativo.DTOs;
using FrontendAdministrativo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

// ============================================================
// UsuariosController.cs — Gestión de usuarios
// ============================================================

namespace FrontendAdministrativo.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        private readonly IEstadisticasService _estadisticasService;
        private readonly IBilleteraService _billeteraService;
        private readonly ILogger<UsuariosController> _logger;

        public UsuariosController(
            IEstadisticasService estadisticasService,
            IBilleteraService billeteraService,
            ILogger<UsuariosController> logger)
        {
            _estadisticasService = estadisticasService;
            _billeteraService = billeteraService;
            _logger = logger;
        }

        // ── GET: /Usuarios ────────────────────────────────────
        public async Task<IActionResult> Index(string? filtroRol = null)
        {
            try
            {
                var usuarios = await _estadisticasService.GetUsuariosAsync();

                // Aplicar filtro por rol
                if (!string.IsNullOrEmpty(filtroRol))
                {
                    // 🔥 CORREGIDO: Usar usuario.Rol?.Nombre en lugar de usuario.Rol
                    usuarios = usuarios.Where(u => u.Rol?.Nombre == filtroRol).ToList();
                }

                // 🔥 OBTENER BILLETERAS PARA ASIGNAR EL SALDO
                var billeteras = await _billeteraService.GetBilleterasAsync();
                
                if (billeteras.Any())
                {
                    var dict = billeteras.ToDictionary(b => b.UsuarioId, b => b.Saldo);
                    foreach (var u in usuarios)
                    {
                        if (dict.TryGetValue(u.Id, out decimal s)) u.SaldoUTNGolCoin = s;
                    }
                }
                else
                {
                    // Fallback si no devuelve lista completa
                    var tasks = usuarios.Select(async u => 
                    {
                        var b = await _billeteraService.GetBilleteraByUsuarioAsync(u.Id);
                        if (b != null) u.SaldoUTNGolCoin = b.Saldo;
                    });
                    await Task.WhenAll(tasks);
                }

                // Pasar roles para el filtro
                ViewBag.Roles = new List<string> { "ADMINISTRADOR", "USUARIO" };
                ViewBag.FiltroRol = filtroRol;

                return View(usuarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuarios.");
                TempData["Error"] = "No se pudieron cargar los usuarios.";
                return View(new List<UsuarioDTO>());
            }
        }

        // ── GET: /Usuarios/Edit/5 ─────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            var usuarios = await _estadisticasService.GetUsuariosAsync();
            var usuario = usuarios.FirstOrDefault(u => u.Id == id);

            if (usuario == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // 🔥 CREAR SelectListItem PARA EL ROL
            ViewBag.Roles = new List<SelectListItem>
    {
        new SelectListItem { Value = "ADMINISTRADOR", Text = "Administrador" },
        new SelectListItem { Value = "USUARIO", Text = "Usuario" }
    };

            return View(usuario);
        }

        // ── POST: /Usuarios/Edit/5 ────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuarioDTO usuario)
        {
            if (id != usuario.Id)
            {
                TempData["Error"] = "El ID del usuario no coincide.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new List<string> { "ADMINISTRADOR", "USUARIO" };
                return View(usuario);
            }

            try
            {
                var exito = await _estadisticasService.UpdateUsuarioAsync(id, usuario);
                if (exito)
                {
                    TempData["Exito"] = "Usuario actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "No se pudo actualizar el usuario.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario {Id}.", id);
                TempData["Error"] = "Error al actualizar el usuario.";
            }

            ViewBag.Roles = new List<string> { "ADMINISTRADOR", "USUARIO" };
            return View(usuario);
        }

        // ── POST: /Usuarios/Delete/5 ──────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var exito = await _estadisticasService.DeleteUsuarioAsync(id);
                if (exito)
                {
                    TempData["Exito"] = "Usuario eliminado correctamente.";
                }
                else
                {
                    TempData["Error"] = "No se pudo eliminar el usuario.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario {Id}.", id);
                TempData["Error"] = "Error al eliminar el usuario.";
            }

            return RedirectToAction(nameof(Index));
        }

        // ── POST: /Usuarios/RecargarSaldo/5 ───────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecargarSaldo(int id)
        {
            var exito = await _billeteraService.RecargarSaldoAsync(id, 1.0m);
            if (exito)
            {
                TempData["Exito"] = "Se ha recargado 1 UTNGolCoin correctamente al usuario.";
            }
            else
            {
                TempData["Error"] = "No se pudo recargar el saldo. Verifique la conexión con la API de billeteras.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}