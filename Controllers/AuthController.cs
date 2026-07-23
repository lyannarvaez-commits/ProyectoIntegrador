using FrontendAdministrativo.DTOs;
using FrontendAdministrativo.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;

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
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(
            ILogger<AuthController> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
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

            try
            {
                // Obtener la URL del backend desde appsettings.json
                var baseUrl = _configuration["ApiSettings:EstadisticasBaseUrl"] ?? "https://localhost:7186";
                var client = _httpClientFactory.CreateClient();

                // Crear el objeto para enviar al backend
                var loginData = new { username = model.Username, password = model.Password };
                var content = new StringContent(
                    JsonConvert.SerializeObject(loginData),
                    Encoding.UTF8,
                    "application/json"
                );

                // Enviar petición al backend
                var response = await client.PostAsync($"{baseUrl}/api/Auth/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Intento de login fallido para: {Username}, Status: {StatusCode}, Error: {Error}",
                        model.Username, response.StatusCode, error);

                    ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                    return View(model);
                }

                // Leer la respuesta del backend
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<LoginResponse>(json);

                if (result?.token == null)
                {
                    ModelState.AddModelError(string.Empty, "Error al obtener el token de autenticación.");
                    return View(model);
                }

                // Guardar token en sesión
                HttpContext.Session.SetString("Token", result.token);
                HttpContext.Session.SetString("Username", model.Username);

                //  LOG PARA VERIFICAR
                _logger.LogInformation("Token guardado en sesión: {Token}", result.token.Substring(0, Math.Min(20, result.token.Length)) + "...");


                // Crear claims del usuario autenticado
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim(ClaimTypes.Role, "Administrador"),
                    new Claim("LoginTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                };

                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("CookieAuth", principal, new AuthenticationProperties
                {
                    IsPersistent = model.Recordarme,
                    ExpiresUtc = model.Recordarme
                        ? DateTimeOffset.UtcNow.AddDays(7)
                        : DateTimeOffset.UtcNow.AddHours(8)
                });

                _logger.LogInformation("Login exitoso: {Username}", model.Username);

                // Redirigir al returnUrl o al Dashboard
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al conectar con el servidor de autenticación.");
                ModelState.AddModelError(string.Empty, "Error al conectar con el servidor. Intente nuevamente.");
                return View(model);
            }
        }

        // ── POST: /Auth/Logout ────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var username = HttpContext.Session.GetString("Username");
            _logger.LogInformation("Logout: {Username}", username);

            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync("CookieAuth");

            return RedirectToAction("Login");
        }
    }

    // ── DTO para la respuesta del backend ────────────────────
    public class LoginResponse
    {
        public string token { get; set; } = string.Empty;
    }
}