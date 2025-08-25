import { getCurrentUser, logout } from '../js/utils/auth.js';
import { fetchWithAuth } from '../js/utils/api.js';
import { showToast, ToastType } from '../js/utils/ui.js';

function renderLoggedOut(container) {
  container.innerHTML = `
    <a href="login.html" class="button button-secondary">Login</a>
    <a href="register.html" class="button button-primary">Register</a>
  `;
}

function renderLoggedIn(container, user) {
  container.innerHTML = `
    <div class="dropdown">
      <button class="button button-success" type="button" title="List a Car">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" width="22" height="22" fill="white"><path d="M18.92 6.01C18.72 5.42 18.16 5 17.5 5h-11c-.66 0-1.21.42-1.42 1.01L3 12v8c0 .55.45 1 1 1h1c.55 0 1-.45 1-1v-1h12v1c0 .55.45 1 1 1h1c.55 0 1-.45 1-1v-8l-2.08-5.99zM6.5 16c-.83 0-1.5-.67-1.5-1.5S5.67 13 6.5 13s1.5.67 1.5 1.5S7.33 16 6.5 16zm11 0c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5 1.5zM5 11l1.5-4.5h11L19 11H5z"/></svg>
      </button>
      <div class="dropdown-content">
        <a href="uploadcar.html">List Your Car</a>
        <a href="carimageupload.html">Add Car Images</a>
      </div>
    </div>

    <div class="dropdown">
      <button class="button button-primary" type="button">Welcome, ${user.username} ▾</button>
      <div class="dropdown-content">
        <a href="myprofile.html">My Profile</a>
        <a href="mycars.html">My Cars</a>
        <button class="logout-link" id="logoutLink">Logout</button>
      </div>
    </div>
    
    <a href="myoffers.html" class="offers-icon" id="offers-link" title="My Offers">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path d="M21.41 11.58l-9-9C12.05 2.22 11.55 2 11 2H4c-1.1 0-2 .9-2 2v7c0 .55.22 1.05.59 1.42l9 9c.36.36.86.58 1.41.58.55 0 1.05-.22 1.41-.59l7-7c.37-.36.59-.86.59-1.41 0-.55-.23-1.06-.59-1.42zM5.5 7C4.67 7 4 6.33 4 5.5S4.67 4 5.5 4 7 4.67 7 5.5 6.33 7 5.5 7z"/></svg>
        <span class="notification-badge" style="display: none;"></span>
    </a>
    <a href="myfavorites.html" class="favorites-icon" id="favorites-link" title="My Favorites">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path d="M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z"/></svg>
        <span class="notification-badge" style="display: none;"></span>
    </a>
    <a href="mynotifications.html" class="notification-bell" id="notifications-link" title="My Notifications">
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path d="M12 22c1.1 0 2-.9 2-2h-4c0 1.1.9 2 2 2zm6-6v-5c0-3.07-1.63-5.64-4.5-6.32V4c0-.83-.67-1.5-1.5-1.5s-1.5.67-1.5 1.5v.68C7.64 5.36 6 7.92 6 11v5l-2 2v1h16v-1l-2-2zm-2 1H8v-6c0-2.48 1.51-4.5 4-4.5s4 2.02 4 4.5v6z"/></svg>
        <span class="notification-badge" style="display: none;"></span>
    </a>
  `;
  document.getElementById('logoutLink').addEventListener('click', () => {
      showToast('You have been logged out.', ToastType.Information);
      logout();
      setTimeout(() => {
          window.location.href = 'login.html';
      }, 1500);
  });
}

async function updateBadge(endpoint, linkId, singular, plural, defaultTitle) {
    try {
        const response = await fetchWithAuth(endpoint);
        if (!response.ok) return;
        const count = parseInt(await response.text(), 10);
        
        const link = document.getElementById(linkId);
        if (!link) return;
        const badge = link.querySelector('.notification-badge');

        if (count > 0) {
            badge.textContent = count;
            badge.style.display = 'inline-flex';
            link.classList.add('has-unread');
            const itemText = count === 1 ? singular : plural;
            link.title = `You have ${count} ${itemText}.`;
        } else {
            badge.style.display = 'none';
            link.classList.remove('has-unread');
            link.title = defaultTitle;
        }
    } catch (error) {
        console.error(`Failed to fetch count for ${linkId}:`, error);
    }
}

function showAdminElements(elements) {
  elements.forEach(el => { if (el) el.style.display = 'inline-block'; });
}

function hideAdminElements(elements) {
  elements.forEach(el => { if (el) el.style.display = 'none'; });
}

function setActiveLink() {
    const currentPath = window.location.pathname.split('/').pop();
    const navLinks = document.querySelectorAll('.topbar-left a, .dropdown-content a, .notification-bell, .offers-icon, .favorites-icon, .home-icon');
    
    navLinks.forEach(link => {
        link.classList.remove('active');
        const linkPath = link.getAttribute('href')?.split('/').pop();
        if (linkPath === currentPath) {
            link.classList.add('active');
            const dropdown = link.closest('.dropdown');
            if (dropdown) {
                dropdown.querySelector('.nav-button, .button')?.classList.add('active');
            }
        }
    });
}

function wireDropdowns() {
    const closeAllDropdowns = () => {
        document.querySelectorAll('.dropdown.open').forEach(d => d.classList.remove('open'));
    };

    document.querySelectorAll('.dropdown').forEach(d => {
        const trigger = d.querySelector('button');
        if (!trigger) return;
        trigger.addEventListener('click', (e) => {
            e.stopPropagation();
            const wasOpen = d.classList.contains('open');
            closeAllDropdowns();
            if (!wasOpen) d.classList.add('open');
        });
    });

    document.addEventListener('click', closeAllDropdowns);
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') closeAllDropdowns();
    });
}

function initHeader() {
  const rightContainer = document.getElementById('topbar-content');
  const adminElements = [
    document.getElementById('logs-dropdown'),
    document.querySelector(".topbar-left a[href*='adminpanel.html']")
  ];
  
  const user = getCurrentUser();

  if (user) {
    renderLoggedIn(rightContainer, user);
    updateBadge(`/notification/user/${user.userId}/unread`, 'notifications-link', 'unread notification', 'unread notifications', 'My Notifications');
    updateBadge(`/offer/received/${user.userId}/pending`, 'offers-link', 'pending offer', 'pending offers', 'My Offers');
    updateBadge(`/user/favorite/count/${user.userId}`, 'favorites-link', 'favorite car', 'favorite cars', 'My Favorites');
    
    if (user.isAdmin) {
        showAdminElements(adminElements);
    } else {
        hideAdminElements(adminElements);
    }
  } else {
    renderLoggedOut(rightContainer);
    hideAdminElements(adminElements);
  }
  
  setActiveLink();
  wireDropdowns();
}

document.addEventListener('DOMContentLoaded', () => {
  const host = document.getElementById('topbar-container');
  if (!host) return;

  fetch('../components/header.html')
    .then(res => res.text())
    .then(html => {
      host.innerHTML = html;
      initHeader();
    })
    .catch(console.error);
});