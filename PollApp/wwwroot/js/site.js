// wwwroot/js/site.js (JS для меню, модала без Bootstrap)
document.addEventListener('DOMContentLoaded', function () {
    const menuBtn = document.querySelector('.menu-btn');
    const sidebar = document.querySelector('.sidebar');
    const sidebarClose = document.querySelector('.sidebar-close');

    menuBtn.addEventListener('click', () => sidebar.classList.add('open'));
    sidebarClose.addEventListener('click', () => sidebar.classList.remove('open'));

    // Profile Modal
    const profileIcon = document.querySelector('.profile-icon');
    const modalOverlay = document.querySelector('.modal-overlay');
    const modalClose = document.querySelector('.close-btn');

    profileIcon.addEventListener('click', () => modalOverlay.classList.add('open'));
    modalClose.addEventListener('click', () => modalOverlay.classList.remove('open'));
    modalOverlay.addEventListener('click', (e) => {
        if (e.target === modalOverlay) modalOverlay.classList.remove('open');
    });
});

/* wwwroot/js/site.js (JS для меню, модала) */
function toggleSidebar() {
    document.getElementById('sidebar').classList.toggle('open');
}

function toggleProfileModal() {
    document.getElementById('profileModal').classList.toggle('open');
}

document.addEventListener('click', function (event) {
    const modal = document.getElementById('profileModal');
    if (!modal.contains(event.target) && !document.querySelector('.avatar').contains(event.target)) {
        modal.classList.remove('open');
    }
});