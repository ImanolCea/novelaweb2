// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.querySelectorAll('.nav-item.dropdown').forEach(function (dropdown) {
    dropdown.addEventListener('mouseenter', function () {
        const menu = this.querySelector('.dropdown-menu');
        menu.classList.add('show');
    });
    dropdown.addEventListener('mouseleave', function () {
        const menu = this.querySelector('.dropdown-menu');
        menu.classList.remove('show');
    });
});
