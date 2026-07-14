using FrontendAdministrativo.Services;

// ============================================================
// Program.cs — Punto de entrada del Frontend Administrativo
// UTN GolMundial 2026
// ============================================================

var builder = WebApplication.CreateBuilder(args);

// ── Servicios MVC con vistas ──────────────────────────────────
builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(); // Soporte para Newtonsoft.Json en los controllers

// ── HttpClient para el Servicio de Estadísticas ───────────────
builder.Services.AddHttpClient<IEstadisticasService, EstadisticasService>(client =>
{
    // URL base de la API de Estadísticas (configurable en appsettings.json)
    var baseUrl = builder.Configuration["ApiSettings:EstadisticasBaseUrl"]
                  ?? "https://localhost:7001";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ── Sesión para manejo de autenticación simple ─────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    var timeoutMinutes = builder.Configuration.GetValue<int>("Session:TimeoutMinutes", 60);
    options.IdleTimeout = TimeSpan.FromMinutes(timeoutMinutes);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".UTNGolMundial.Admin";
});

// ── Autenticación por cookie ───────────────────────────────────
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/Login";
        options.Cookie.Name = ".UTNGolMundial.Auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// ── HttpContextAccessor (útil para acceder a la sesión desde servicios) ──
builder.Services.AddHttpContextAccessor();

// ─────────────────────────────────────────────────────────────
var app = builder.Build();
// ─────────────────────────────────────────────────────────────

// ── Manejo de errores ─────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Archivos en wwwroot (CSS, JS, imágenes)

app.UseRouting();

app.UseSession(); // Habilitar sesión ANTES de auth
app.UseAuthentication();
app.UseAuthorization();

// ── Ruta por defecto → Dashboard ─────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
