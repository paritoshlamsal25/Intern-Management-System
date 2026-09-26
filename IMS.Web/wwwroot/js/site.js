// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function () {
    // show bootstrap toasts populated by server (if any)
    var toastContainer = document.querySelector('.toast-container');
    if (!toastContainer) return;

    // Server may inject a data attribute with messages later; keep placeholder
    // Initialize any toasts in the container
    var toastElList = [].slice.call(toastContainer.querySelectorAll('.toast'));
    toastElList.forEach(function (toastEl) {
        var toast = new bootstrap.Toast(toastEl);
        toast.show();
    });

// Toggle password visibility on login
function togglePassword(id, toggleId) {
    var input = document.getElementById(id);
    var btn = document.getElementById(toggleId);
    if (!input || !btn) return;
    if (input.type === 'password') {
        input.type = 'text';
        btn.innerText = 'Hide';
    } else {
        input.type = 'password';
        btn.innerText = 'Show';
    }
}

// Sidebar toggle for small screens
document.addEventListener('DOMContentLoaded', function () {
    var toggle = document.getElementById('sidebarToggle');
    var sidebar = document.querySelector('.sidebar');
    var main = document.querySelector('.main-content');
    if (toggle && sidebar) {
        toggle.addEventListener('click', function () {
            sidebar.classList.toggle('d-none');
        });
    }
});
});
