import { getUserIdOrRedirect } from './utils/auth.js';
import { showToast, ToastType } from './utils/ui.js';
import { formatTimestamp } from './utils/timeutils.js';
import { fetchWithAuth } from './utils/api.js';

let allMyCars = [];
const defaultImageUrl = '../images/default-car.png';

function renderTable(cars) {
    const container = document.getElementById('table-container');
    container.innerHTML = '';

    if (!cars || cars.length === 0) {
        container.innerHTML = `<p class="status-message">No cars match the current filters.</p>`;
        return;
    }

    const table = document.createElement('table');
    table.innerHTML = `
        <thead>
            <tr>
                <th class="align-center">Image</th>
                <th>Brand/Model</th>
                <th class="align-right">Price</th>
                <th class="align-center">Status</th>
                <th class="align-right">List Date</th>
                <th class="actions-cell">Actions</th>
            </tr>
        </thead>
        <tbody></tbody>
    `;
    const tbody = table.querySelector('tbody');
    tbody.addEventListener('click', handleTableClick);

    for (const car of cars) {
        const imageUrl = car.thumbnailUrl?.trim() || defaultImageUrl;
        const price = Number(car.price).toLocaleString('tr-TR', { style: 'currency', currency: 'TRY', minimumFractionDigits: 0 });
        const status = car.status === 'sold' ? 'sold' : 'available';

        const listDate = new Date(car.listDate);
        const dd = String(listDate.getDate()).padStart(2, '0');
        const mm = String(listDate.getMonth() + 1).padStart(2, '0');
        const yyyy = listDate.getFullYear();
        const formattedDisplayDate = `${dd}.${mm}.${yyyy}`;
        const formattedTitleDate = formatTimestamp(car.listDate, false);

        const row = document.createElement('tr');
        row.dataset.carid = car.carId;
        row.dataset.href = `details.html?carId=${car.carId}`;
        row.innerHTML = `
            <td class="align-center">
                <img src="${imageUrl}" alt="${car.brand} ${car.model}" onerror="this.onerror=null;this.src='${defaultImageUrl}';">
            </td>
            <td>
                <div class="car-name">${car.brand}</div>
                <div class="car-model">${car.model} (${car.year})</div>
            </td>
            <td class="align-right car-price">${price}</td>
            <td class="align-center">
                <span class="status-badge ${status}">${status}</span>
            </td>
            <td class="align-right" title="${formattedTitleDate}">${formattedDisplayDate}</td>
            <td class="actions-cell">
                <button class="toggle-status-btn" data-state="${status}">
                    ${status === 'available' ? 'Mark as Sold' : 'Mark as Available'}
                </button>
            </td>
        `;
        tbody.appendChild(row);
    }
    container.appendChild(table);
}

function applyFiltersAndSort() {
    const brandFilter = document.getElementById('brand-filter').value;
    const statusFilter = document.getElementById('status-filter').value;
    const sortBy = document.getElementById('sort-by').value.split('-');
    const sortKey = sortBy[0];
    const sortDir = sortBy[1];

    let filteredCars = allMyCars.filter(car => {
        const brandMatch = brandFilter === 'all' || car.brand === brandFilter;
        const statusMatch = statusFilter === 'all' || car.status === statusFilter;
        return brandMatch && statusMatch;
    });

    filteredCars.sort((a, b) => {
        let valA = a[sortKey];
        let valB = b[sortKey];
        if (sortKey === 'listDate') {
            valA = new Date(a.listDate).getTime();
            valB = new Date(b.listDate).getTime();
        }
        if (valA < valB) return sortDir === 'asc' ? -1 : 1;
        if (valA > valB) return sortDir === 'asc' ? 1 : -1;
        return 0;
    });

    renderTable(filteredCars);
}

function populateBrandFilter(cars) {
    const brandFilter = document.getElementById('brand-filter');
    const brands = [...new Set(cars.map(car => car.brand))].sort();
    brands.forEach(brand => {
        const option = document.createElement('option');
        option.value = brand;
        option.textContent = brand;
        brandFilter.appendChild(option);
    });
}

async function updateCarStatus(button) {
    const row = button.closest('tr');
    const carId = row.dataset.carid;
    const currentState = button.dataset.state;
    const newState = currentState === 'available' ? 'sold' : 'available';

    button.disabled = true;

    try {
        const response = await fetchWithAuth(`/car/mark/${newState}/${carId}`, {
            method: 'PUT'
        });
        if (!response.ok) throw new Error('Failed to update status.');
        
        const carInState = allMyCars.find(c => c.carId == carId);
        if (carInState) {
            carInState.status = newState;
        }

        applyFiltersAndSort();
        showToast(`Car successfully marked as ${newState}.`, ToastType.Success);
        
    } catch (error) {
        showToast(error.message, ToastType.Error);
        button.disabled = false;
    }
}

async function handleTableClick(e) {
    const statusButton = e.target.closest('.toggle-status-btn');
    if (statusButton) {
        e.stopPropagation();
        await updateCarStatus(statusButton);
        return;
    }

    const row = e.target.closest('tr');
    if (row && row.dataset.href) {
        window.location.href = row.dataset.href;
    }
}

document.addEventListener('DOMContentLoaded', async () => {
    const userId = getUserIdOrRedirect();
    const container = document.getElementById('table-container');
    
    try {
        const response = await fetchWithAuth(`/car/user/${userId}`);
        if (!response.ok) throw new Error('Could not load your cars.');
        allMyCars = await response.json();
        
        populateBrandFilter(allMyCars);
        applyFiltersAndSort();

        document.getElementById('brand-filter').addEventListener('change', applyFiltersAndSort);
        document.getElementById('status-filter').addEventListener('change', applyFiltersAndSort);
        document.getElementById('sort-by').addEventListener('change', applyFiltersAndSort);

    } catch (error) {
        container.innerHTML = `<p class="status-message">${error.message}</p>`;
    }
});