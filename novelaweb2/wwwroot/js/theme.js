// ==========================
// 🌗 Theme toggle global
// ==========================
(function () {
    const body = document.body;
    let currentTheme = localStorage.getItem("theme") || "dark";

    // 1. Aplica la clase al <body> INMEDIATAMENTE (Corrige el FOUC)
    // Esto se ejecuta en el <head> antes de que la página se "pinte".
    if (currentTheme === "light") {
        body.classList.add("theme-light");
    }

    // 2. Define la función de cambio (será llamada por el botón)
    window.toggleTheme = function () {
        body.classList.toggle("theme-light");

        // Decide el nuevo tema y guárdalo
        currentTheme = body.classList.contains("theme-light") ? "light" : "dark";
        localStorage.setItem("theme", currentTheme);

        // Actualiza el ícono
        updateIcon();
    };

    // 3. Función para actualizar el ícono
    function updateIcon() {
        const icon = document.getElementById("theme-icon");
        if (icon) {
            if (currentTheme === "light") {
                icon.className = "fa-solid fa-sun"; // Ícono de Sol
            } else {
                icon.className = "fa-solid fa-moon"; // Ícono de Luna
            }
        }
    }

    // 4. Actualiza el ícono en la carga inicial
    // Usamos DOMContentLoaded para asegurar que el <i> (icono) exista
    // antes de intentar cambiarle la clase.
    document.addEventListener("DOMContentLoaded", function () {
        updateIcon();
    });
})();