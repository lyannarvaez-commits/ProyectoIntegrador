// ============================================================
// admin.js — Funcionalidades del Panel Administrativo
// UTN GolMundial 2026
// Requiere: SweetAlert2 (cargado desde CDN en _Layout.cshtml)
// ============================================================

"use strict";

// ── Reloj en tiempo real (topbar) ────────────────────────────
function actualizarReloj() {
    const el = document.getElementById("topbar-clock");
    if (!el) return;

    const now = new Date();
    const opciones = {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit",
        hour12: false
    };
    el.textContent = now.toLocaleString("es-AR", opciones);
}

// ── Inicializar reloj ────────────────────────────────────────
actualizarReloj();
setInterval(actualizarReloj, 1000);

// ── Auto-ocultar alertas TempData después de 4 segundos ─────
document.addEventListener("DOMContentLoaded", function () {
    const alertas = document.querySelectorAll(".alert-admin[data-auto-hide]");
    alertas.forEach(function (alerta) {
        setTimeout(function () {
            alerta.style.transition = "opacity 0.5s ease";
            alerta.style.opacity = "0";
            setTimeout(function () { alerta.remove(); }, 500);
        }, 4000);
    });
});

// ── Confirmación de eliminación con SweetAlert2 ──────────────
/**
 * Muestra un diálogo de confirmación antes de eliminar.
 * Uso en HTML: onclick="confirmarEliminar(event, 'Partido #5')"
 */
function confirmarEliminar(event, nombreElemento) {
    event.preventDefault();
    const form = event.target.closest("form");
    if (!form) return;

    Swal.fire({
        title: "¿Eliminar elemento?",
        html: `¿Estás seguro que deseas eliminar <strong>${nombreElemento}</strong>?<br>
               <small class="text-danger">Esta acción no se puede deshacer.</small>`,
        icon: "warning",
        showCancelButton: true,
        confirmButtonText: "Sí, eliminar",
        cancelButtonText: "Cancelar",
        confirmButtonColor: "#e94560",
        cancelButtonColor: "#16213e",
        background: "#16213e",
        color: "#e8e8f0",
        iconColor: "#e94560",
        customClass: {
            popup:   "swal-admin-popup",
            title:   "swal-admin-title"
        }
    }).then(function (result) {
        if (result.isConfirmed) {
            form.submit();
        }
    });
}

// ── Confirmación de logout ───────────────────────────────────
function confirmarLogout(event) {
    event.preventDefault();
    const form = event.target.closest("form");
    if (!form) return;

    Swal.fire({
        title: "Cerrar sesión",
        text: "¿Deseas salir del panel administrativo?",
        icon: "question",
        showCancelButton: true,
        confirmButtonText: "Sí, cerrar sesión",
        cancelButtonText: "Cancelar",
        confirmButtonColor: "#e94560",
        cancelButtonColor: "#16213e",
        background: "#16213e",
        color: "#e8e8f0"
    }).then(function (result) {
        if (result.isConfirmed) {
            form.submit();
        }
    });
}

// ── Toast de notificación rápida ─────────────────────────────
/**
 * Muestra un toast no intrusivo.
 * @param {string} mensaje - Texto a mostrar
 * @param {string} tipo - "success" | "error" | "warning" | "info"
 */
function mostrarToast(mensaje, tipo = "success") {
    const Toast = Swal.mixin({
        toast: true,
        position: "top-end",
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true,
        background: "#16213e",
        color: "#e8e8f0",
        didOpen: (toast) => {
            toast.addEventListener("mouseenter", Swal.stopTimer);
            toast.addEventListener("mouseleave", Swal.resumeTimer);
        }
    });

    Toast.fire({ icon: tipo, title: mensaje });
}

// ── Sidebar toggle (responsive) ──────────────────────────────
function toggleSidebar() {
    const sidebar = document.getElementById("adminSidebar");
    if (sidebar) {
        sidebar.classList.toggle("sidebar-open");
    }
}

// ── Marcar ítem activo del sidebar ──────────────────────────
document.addEventListener("DOMContentLoaded", function () {
    const currentPath = window.location.pathname.toLowerCase();
    const navLinks = document.querySelectorAll(".sidebar-item[href]");

    navLinks.forEach(function (link) {
        const href = link.getAttribute("href").toLowerCase();
        if (href !== "/" && currentPath.startsWith(href)) {
            link.classList.add("active");
        } else if (href === "/" && currentPath === "/") {
            link.classList.add("active");
        }
    });
});

// ── Validación en tiempo real del formulario de resultado ────
document.addEventListener("DOMContentLoaded", function () {
    const inputsGoles = document.querySelectorAll("input[data-tipo='goles']");

    inputsGoles.forEach(function (input) {
        input.addEventListener("input", function () {
            let val = parseInt(this.value);

            // Asegurar valor entre 0 y 99
            if (isNaN(val) || val < 0) this.value = 0;
            if (val > 99) this.value = 99;

            // Actualizar preview del resultado
            actualizarPreviewResultado();
        });
    });

    actualizarPreviewResultado();
});

// ── Preview del resultado en tiempo real ─────────────────────
function actualizarPreviewResultado() {
    const inputLocal = document.getElementById("golesLocalInput");
    const inputVisitante = document.getElementById("golesVisitanteInput");
    const preview = document.getElementById("resultadoPreview");

    if (!inputLocal || !inputVisitante || !preview) return;

    const gl = parseInt(inputLocal.value) || 0;
    const gv = parseInt(inputVisitante.value) || 0;

    preview.textContent = `${gl} - ${gv}`;

    // Colorear según resultado
    if (gl > gv) {
        preview.style.color = "#28a745";
    } else if (gv > gl) {
        preview.style.color = "#dc3545";
    } else {
        preview.style.color = "#ffc107";
    }
}

// ── Filtro de tabla en tiempo real ───────────────────────────
/**
 * Filtra las filas de una tabla por texto en tiempo real.
 * @param {string} inputId - ID del input de búsqueda
 * @param {string} tableId - ID de la tabla a filtrar
 */
function filtrarTabla(inputId, tableId) {
    const input = document.getElementById(inputId);
    const tabla = document.getElementById(tableId);

    if (!input || !tabla) return;

    input.addEventListener("keyup", function () {
        const filtro = this.value.toLowerCase();
        const filas = tabla.querySelectorAll("tbody tr");

        let visibles = 0;
        filas.forEach(function (fila) {
            const texto = fila.textContent.toLowerCase();
            if (texto.includes(filtro)) {
                fila.style.display = "";
                visibles++;
            } else {
                fila.style.display = "none";
            }
        });

        // Mostrar "sin resultados" si no hay coincidencias
        const sinResultados = document.getElementById("sinResultados");
        if (sinResultados) {
            sinResultados.style.display = visibles === 0 ? "block" : "none";
        }
    });
}

// Inicializar filtros de tabla si existen
document.addEventListener("DOMContentLoaded", function () {
    filtrarTabla("busquedaTabla", "tablaPartidos");
    filtrarTabla("busquedaUsuarios", "tablaUsuarios");
});

// ── Animación de contadores en el Dashboard ──────────────────
function animarContador(elementId, valorFinal, duracion = 1500) {
    const el = document.getElementById(elementId);
    if (!el) return;

    const inicio = 0;
    const incremento = valorFinal / (duracion / 16);
    let actual = inicio;

    const timer = setInterval(function () {
        actual += incremento;
        if (actual >= valorFinal) {
            actual = valorFinal;
            clearInterval(timer);
        }
        el.textContent = Math.floor(actual).toLocaleString("es-AR");
    }, 16);
}

// Inicializar contadores del Dashboard
document.addEventListener("DOMContentLoaded", function () {
    const contadores = document.querySelectorAll("[data-counter]");
    contadores.forEach(function (el) {
        const valor = parseInt(el.getAttribute("data-counter")) || 0;
        const id = el.getAttribute("id");
        if (id) animarContador(id, valor);
    });
});
