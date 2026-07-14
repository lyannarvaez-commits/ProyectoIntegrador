using FrontendAdministrativo.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// ============================================================
// AuthController.cs — Autenticación del panel administrativo
// ============================================================

namespace FrontendAdministrativo.Controllers
{
    /// <summary>
    /// Gestiona el inicio y cierre de sesión del administrador.
    /// </summary>
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        // ── GET: /Auth/Login ──────────────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Si ya está autenticado, redirigir al Dashboard
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginDTO());
        }

        // ── POST: /Auth/Login ─────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDTO model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            // ─────────────────────────────────────────────────
            // NOTA: En producción, validar credenciales contra la API.
            // Por ahora se usa validación simple de ejemplo.
            // Reemplazar con llamada real a la API de autenticación.
            // ─────────────────────────────────────────────────
            bool credencialesValidas = ValidarCredenciales(model.Email, model.Password);

            if (!credencialesValidas)
            {
                ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
                _logger.LogWarning("Intento de login fallido para: {Email}", model.Email);
                return View(model);
            }

            // Crear claims del usuario autenticado
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Email),
                new Claim(ClaimTypes.Email, model.Email),
                new Claim(ClaimTypes.Role, "Administrador"),
                new Claim("LoginTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            // Guardar en sesión y cookie
            HttpContext.Session.SetString("AdminEmail", model.Email);
            HttpContext.Session.SetString("AdminNombre", ObtenerNombreAdmin(model.Email));

            await HttpContext.SignInAsync("CookieAuth", principal, new AuthenticationProperties
            {
                IsPersistent = model.Recordarme,
                ExpiresUtc = model.Recordarme
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : DateTimeOffset.UtcNow.AddHours(8)
            });

            _logger.LogInformation("Login exitoso: {Email}", model.Email);

            // Redirigir al returnUrl o al Dashboard
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        // ── POST: /Auth/Logout ────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var email = HttpContext.Session.GetString("AdminEmail");
            _logger.LogInformation("Logout: {Email}", email);

            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync("CookieAuth");

            return RedirectToAction("Login");
        }

        // ── HELPER: Validación de credenciales ────────────────
        /// <summary>
        /// Valida credenciales de administrador.
        /// TODO: Reemplazar con llamada a la API de autenticación.
        /// </summary>
        private static bool ValidarCredenciales(string email, string password)
        {
            // Credenciales de ejemplo — reemplazar con API real
            return email == "admin@utn.edu.ar" && password == "Admin123!";
        }

        private static string ObtenerNombreAdmin(string email)
        {
            return email == "admin@utn.edu.ar" ? "Administrador UTN" : email.Split('@')[0];
        }
    }
}
