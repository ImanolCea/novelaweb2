// ==========================
// 🌗 Theme toggle global
// ==========================
(function () {
    const body = document.body;
    let currentTheme = localStorage.getItem("theme") || "dark";

    // 1. Aplica la clase al <body> INMEDIATAMENTE
    if (currentTheme === "light") {
        body.classList.add("theme-light");
    }

    // 2. Define la función de cambio (hacerla window.toggleTheme para que sea global)
    window.toggleTheme = function () {
        body.classList.toggle("theme-light");

        // Decide el nuevo tema y guárdalo
        currentTheme = body.classList.contains("theme-light") ? "light" : "dark";
        localStorage.setItem("theme", currentTheme);

        // Llama a la función de actualización de íconos para TODOS los botones
        updateAllIcons();
    };

    // 3. Función para actualizar TODOS los íconos (usa una clase en lugar de IDs fijos)
    function updateAllIcons() {
        // Selecciona todos los elementos que deberían ser íconos de tema
        const iconElements = document.querySelectorAll(".theme-icon-dynamic");

        iconElements.forEach(icon => {
            if (currentTheme === "light") {
                icon.className = "fa-solid fa-sun theme-icon-dynamic"; // Ícono de Sol
            } else {
                icon.className = "fa-solid fa-moon theme-icon-dynamic"; // Ícono de Luna
            }
        });
    }

    // 4. Actualiza el ícono en la carga inicial
    document.addEventListener("DOMContentLoaded", function () {
        updateAllIcons();
    });
})();