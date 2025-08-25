import { fetchUserProfile as getUserProfile } from './utils/auth.js';
import { fetchWithAuth } from './utils/api.js';
import { showToast, ToastType } from './utils/ui.js';

const usersContainer = document.getElementById('users-table-container');
const carsContainer = document.getElementById('cars-table-container');

async function checkAdmin() {
  try {
    const user = await getUserProfile();
    if (!user || !user.isAdmin) window.location.href = 'index.html';
  } catch {
    window.location.href = 'login.html';
  }
}

function initTabs() {
  const tabButtons = document.querySelectorAll('.tab-btn');
  const tabPanels = document.querySelectorAll('.tab-panel');

  tabButtons.forEach(button => {
    button.addEventListener('click', () => {
      tabButtons.forEach(btn => btn.classList.remove('active'));
      tabPanels.forEach(panel => panel.classList.remove('active'));
      button.classList.add('active');
      document.getElementById(button.dataset.tab).classList.add('active');
    });
  });
}

function renderTable(container, headers, data, rowRenderer) {
    if (!data || data.length === 0) {
        container.innerHTML = `<p class="status-message">No data found.</p>`;
        return null;
    }

    const table = document.createElement('table');
    const thead = document.createElement('thead');
    const tbody = document.createElement('tbody');
    
    thead.innerHTML = `<tr>${headers.map(h => `<th>${h}</th>`).join('')}</tr>`;
    data.forEach(item => tbody.appendChild(rowRenderer(item)));
    
    table.appendChild(thead);
    table.appendChild(tbody);
    container.innerHTML = '';
    container.appendChild(table);
    
    return tbody;
}

function renderUserRow(user) {
    const row = document.createElement('tr');
    const isActive = user.isActive;
    row.innerHTML = `
        <td>${user.userId}</td>
        <td>${user.username}</td>
        <td>${user.mail || 'N/A'}</td>
        <td>${user.phone || 'N/A'}</td>
        <td>${user.userLocation || 'N/A'}</td>
        <td><span class="status-badge ${isActive ? 'status-active' : 'status-inactive'}">${isActive ? 'Active' : 'Inactive'}</span></td>
        <td>${user.isAdmin ? 'Yes' : 'No'}</td>
        <td><button class="action-btn" data-action="toggle-user" data-id="${user.userId}" data-state="${isActive ? 'active' : 'inactive'}">${isActive ? 'Deactivate' : 'Activate'}</button></td>
    `;
    return row;
}

function renderCarRow(car) {
    const row = document.createElement('tr');
    const isSold = car.status === 'sold';
    const price = Number(car.price).toLocaleString('tr-TR', { style: 'currency', currency: 'TRY', minimumFractionDigits: 0 });
    row.innerHTML = `
        <td>${car.carId}</td>
        <td>${car.ownerUsername}</td>
        <td>${car.brand} ${car.model}</td>
        <td>${car.year}</td>
        <td>${Number(car.km).toLocaleString('tr-TR')} km</td>
        <td>${price}</td>
        <td><span class="status-badge ${isSold ? 'status-sold' : 'status-available'}">${isSold ? 'Sold' : 'Available'}</span></td>
        <td><button class="action-btn" data-action="toggle-car" data-id="${car.carId}" data-state="${isSold ? 'sold' : 'available'}">${isSold ? 'Mark Available' : 'Mark Sold'}</button></td>
    `;
    return row;
}

async function handleUserAction(e) {
    const button = e.target.closest('[data-action="toggle-user"]');
    if (!button) return;

    const id = button.dataset.id;
    const currentState = button.dataset.state;
    const activate = currentState === 'inactive';
    
    button.disabled = true;
    try {
        const endpoint = activate ? `/user/activate/${id}` : `/user/deactivate/${id}`;
        const response = await fetchWithAuth(endpoint, { method: 'PUT' });
        if (!response.ok) throw new Error('Failed to update user status.');
        
        const newState = activate ? 'active' : 'inactive';
        const statusBadge = button.closest('tr').querySelector('.status-badge');

        button.dataset.state = newState;
        button.textContent = activate ? 'Deactivate' : 'Activate';
        statusBadge.textContent = activate ? 'Active' : 'Inactive';
        statusBadge.className = `status-badge status-${newState}`;
        showToast(`User status updated to ${newState}.`, ToastType.Success);

    } catch (error) {
        showToast(error.message, ToastType.Error);
    } finally {
        button.disabled = false;
    }
}

async function handleCarAction(e) {
    const button = e.target.closest('[data-action="toggle-car"]');
    if (!button) return;

    const id = button.dataset.id;
    const currentState = button.dataset.state;
    const makeAvailable = currentState === 'sold';
    
    button.disabled = true;
    try {
        const endpoint = makeAvailable ? `/car/mark/available/${id}` : `/car/mark/sold/${id}`;
        const response = await fetchWithAuth(endpoint, { method: 'PUT' });
        if (!response.ok) throw new Error('Failed to update car status.');

        const newState = makeAvailable ? 'available' : 'sold';
        const statusBadge = button.closest('tr').querySelector('.status-badge');

        button.dataset.state = newState;
        button.textContent = makeAvailable ? 'Mark Sold' : 'Mark Available';
        statusBadge.textContent = newState;
        statusBadge.className = `status-badge status-${newState}`;
        showToast(`Car status updated to ${newState}.`, ToastType.Success);

    } catch (error) {
        showToast(error.message, ToastType.Error);
    } finally {
        button.disabled = false;
    }
}

document.addEventListener('DOMContentLoaded', async () => {
    await checkAdmin();
    initTabs();

    try {
        const [usersRes, carsRes] = await Promise.all([
            fetchWithAuth('/user/all'),
            fetchWithAuth('/car/all')
        ]);

        if (!usersRes.ok || !carsRes.ok) throw new Error('Failed to load initial data.');

        const users = await usersRes.json();
        const cars = await carsRes.json();

        const userTableBody = renderTable(usersContainer, ['ID', 'Username', 'Email', 'Phone', 'Location', 'Status', 'Admin', 'Action'], users, renderUserRow);
        if (userTableBody) userTableBody.addEventListener('click', handleUserAction);

        const carTableBody = renderTable(carsContainer, ['ID', 'Owner', 'Car', 'Year', 'KM', 'Price', 'Status', 'Action'], cars, renderCarRow);
        if (carTableBody) carTableBody.addEventListener('click', handleCarAction);

    } catch (error) {
        usersContainer.innerHTML = `<p class="status-message">Could not load user data.</p>`;
        carsContainer.innerHTML = `<p class="status-message">Could not load car data.</p>`;
    }
});