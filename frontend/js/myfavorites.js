import { getUserIdOrRedirect } from './utils/auth.js';
import { formatTimestamp } from './utils/timeutils.js';
import { fetchWithAuth } from './utils/api.js';

let allFavoriteCars = [];
const defaultImageUrl = '../images/default-car.png';

function renderTable(cars) {
    const container = document.getElementById('table-container');
    container.innerHTML = '';

    if (!cars || cars.length === 0) {
        container.innerHTML = `<p class="status-message">No favorite cars match the current filters.</p>`;
        return;
    }

    const table = document.createElement('table');
    table.innerHTML = `
        <thead>
            <tr>
                <th class="align-center">Image</th>
                <th>Brand/Model</th>
                <th class="align-right">Year</th>
                <th class="align-right">Kilometers</th>
                <th>Location</th>
                <th class="align-right">Price</th>
                <th class="align-right">List Date</th>
            </tr>
        </thead>
        <tbody></tbody>
    `;
    const tbody = table.querySelector('tbody');

    for (const car of cars) {
        const imageUrl = car.thumbnailUrl?.trim() || defaultImageUrl;
        const price = Number(car.price).toLocaleString('tr-TR', { style: 'currency', currency: 'TRY', minimumFractionDigits: 0 });
        const km = Number(car.km).toLocaleString('tr-TR');
        
        const listDate = new Date(car.listDate);
        const dd = String(listDate.getDate()).padStart(2, '0');
        const mm = String(listDate.getMonth() + 1).padStart(2, '0');
        const yyyy = listDate.getFullYear();
        const formattedDisplayDate = `${dd}.${mm}.${yyyy}`;
        const formattedTitleDate = formatTimestamp(car.listDate, false);

        const row = document.createElement('tr');
        row.addEventListener('click', () => {
            window.location.href = `details.html?carId=${car.carId}`;
        });
        
        row.innerHTML = `
            <td class="align-center">
                <img src="${imageUrl}" alt="${car.brand} ${car.model}" onerror="this.onerror=null;this.src='${defaultImageUrl}';">
            </td>
            <td>
                <div class="car-name">${car.brand}</div>
                <div class="car-model">${car.model}</div>
            </td>
            <td class="align-right">${car.year}</td>
            <td class="align-right">${km} km</td>
            <td>${car.ownerLocation}</td>
            <td class="align-right car-price">${price}</td>
            <td class="align-right car-listdate" title="${formattedTitleDate}">${formattedDisplayDate}</td>
        `;
        tbody.appendChild(row);
    }
    container.appendChild(table);
}

function applyFiltersAndSort() {
    const brandFilter = document.getElementById('brand-filter').value;
    const sortBy = document.getElementById('sort-by').value.split('-');
    const sortKey = sortBy[0];
    const sortDir = sortBy[1];

    let filteredCars = allFavoriteCars.filter(car => {
        const brandMatch = brandFilter === 'all' || car.brand === brandFilter;
        return brandMatch;
    });

    filteredCars.sort((a, b) => {
        let valA = a[sortKey];
        let valB = b[sortKey];

        if (sortKey === 'listDate') {
            valA = new Date(a.listDate).getTime();
            valB = new Date(b.listDate).getTime();
        }

        if (valA < valB) {
            return sortDir === 'asc' ? -1 : 1;
        }
        if (valA > valB) {
            return sortDir === 'asc' ? 1 : -1;
        }
        return 0;
    });

    renderTable(filteredCars);
}

function populateBrandFilter(cars) {
    const brandFilter = document.getElementById('brand-filter');
    const brands = [...new Set(cars.map(car => car.brand))];
    brands.sort();
    brands.forEach(brand => {
        const option = document.createElement('option');
        option.value = brand;
        option.textContent = brand;
        brandFilter.appendChild(option);
    });
}

document.addEventListener('DOMContentLoaded', async () => {
    const userId = getUserIdOrRedirect();
    const container = document.getElementById('table-container');

    try {
        const response = await fetchWithAuth(`/car/favorites/${userId}`);
        if (!response.ok) {
            throw new Error('Could not load your favorites.');
        }
        allFavoriteCars = await response.json();
        
        if (!allFavoriteCars || allFavoriteCars.length === 0) {
            container.innerHTML = `<p class="status-message">You haven't saved any favorite cars yet.</p>`;
            return;
        }
        
        populateBrandFilter(allFavoriteCars);
        applyFiltersAndSort();

        document.getElementById('brand-filter').addEventListener('change', applyFiltersAndSort);
        document.getElementById('sort-by').addEventListener('change', applyFiltersAndSort);

    } catch (error) {
        container.innerHTML = `<p class="status-message">Could not load your favorites. Please try again later.</p>`;
    }
});