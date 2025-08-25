import { formatTimestamp } from './utils/timeutils.js';
import { fetchWithAuth } from './utils/api.js';

let allCarsData = [];
let currentPage = 1;
let pageSize = 10;
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

function renderPagination(totalItems, totalPages) {
    const paginationContainer = document.getElementById('pagination-container');
    if (!paginationContainer) return;
    
    paginationContainer.innerHTML = `
        <div class="page-size-selector">
            <label for="page-size">Items per page:</label>
            <select id="page-size">
                <option value="5">5</option>
                <option value="10">10</option>
                <option value="20">20</option>
                <option value="50">50</option>
            </select>
        </div>
        <div class="page-info">
            Page ${currentPage} of ${totalPages} (${totalItems} total cars)
        </div>
        <div class="pagination-controls">
            <button id="prev-page" ${currentPage === 1 ? 'disabled' : ''}>Previous</button>
            <button id="next-page" ${currentPage >= totalPages ? 'disabled' : ''}>Next</button>
        </div>
    `;
    
    document.getElementById('page-size').value = pageSize;

    document.getElementById('prev-page').addEventListener('click', () => {
        if (currentPage > 1) {
            currentPage--;
            fetchAndRenderCars();
        }
    });

    document.getElementById('next-page').addEventListener('click', () => {
        if (currentPage < totalPages) {
            currentPage++;
            fetchAndRenderCars();
        }
    });

    document.getElementById('page-size').addEventListener('change', (e) => {
        pageSize = parseInt(e.target.value, 10);
        currentPage = 1;
        fetchAndRenderCars();
    });
}

async function fetchAndRenderCars() {
    const container = document.getElementById('table-container');
    container.innerHTML = `<div class="loader"></div>`;

    const brand = document.getElementById('brand-filter').value;
    const [sortBy, sortDir] = document.getElementById('sort-by').value.split('-');
    
    const url = `/car/available?pageNumber=${currentPage}&pageSize=${pageSize}&brand=${brand}&sortBy=${sortBy}&sortDirection=${sortDir}`;
    
    try {
        const response = await fetchWithAuth(url);
        if (!response.ok) {
            throw new Error('Failed to fetch car data.');
        }
        const data = await response.json();
        
        allCarsData = data.items;
        renderTable(allCarsData);
        renderPagination(data.totalItems, data.totalPages);
    } catch (error) {
        container.innerHTML = `<p class="status-message">Could not load car listings. Please try again later.</p>`;
    }
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

document.addEventListener('DOMContentLoaded', async () => {
    const container = document.getElementById('table-container');
    try {
        const initialResponse = await fetchWithAuth('/car/available');
        if (!initialResponse.ok) throw new Error('Failed to fetch initial car data.');
        const initialData = await initialResponse.json();
        populateBrandFilter(initialData.items);
        
        await fetchAndRenderCars();

        document.getElementById('brand-filter').addEventListener('change', () => {
            currentPage = 1;
            fetchAndRenderCars();
        });
        document.getElementById('sort-by').addEventListener('change', () => {
            currentPage = 1;
            fetchAndRenderCars();
        });

    } catch (error) {
        container.innerHTML = `<p class="status-message">${error.message}</p>`;
    }
});