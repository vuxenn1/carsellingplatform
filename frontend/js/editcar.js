import { getUserIdOrRedirect } from './utils/auth.js';
import { showToast, ToastType } from './utils/ui.js';
import { fetchWithAuth } from './utils/api.js';

async function fetchCarDetails(carId) {
    const response = await fetchWithAuth(`/car/details/${carId}`);
    if (!response.ok) {
        throw new Error('Failed to load car details.');
    }
    return await response.json();
}

function renderEditForm(car) {
    const container = document.getElementById('edit-car-container');
    const fuelTypes = ['Petrol', 'Diesel', 'Electric'];
    const transmissions = ['Manual', 'Automatic'];

    const createOptions = (options, selectedValue) => 
        options.map(opt => `<option value="${opt}" ${selectedValue === opt ? 'selected' : ''}>${opt}</option>`).join('');

    container.innerHTML = `
        <div class="page-header">
            <h1>Edit Listing</h1>
            <p>You are editing your: <strong>${car.brand} ${car.model} (${car.year})</strong></p>
        </div>
        <div class="form-box">
            <form id="editCarForm">
                <div class="form-grid">
                    <div class="form-group">
                        <label for="brand">Brand</label>
                        <input type="text" id="brand" value="${car.brand}" required />
                    </div>
                    <div class="form-group">
                        <label for="model">Model</label>
                        <input type="text" id="model" value="${car.model}" required />
                    </div>
                    <div class="form-group">
                        <label for="km">Kilometers</label>
                        <input type="text" id="km" value="${new Intl.NumberFormat('tr-TR').format(car.km)}" inputmode="numeric" required />
                    </div>
                    <div class="form-group">
                        <label for="price">Price (TL)</label>
                        <input type="text" id="price" value="${new Intl.NumberFormat('tr-TR').format(car.price)}" inputmode="numeric" required />
                    </div>
                    <div class="form-group">
                        <label for="fuelType">Fuel Type</label>
                        <select id="fuelType">${createOptions(fuelTypes, car.fuelType)}</select>
                    </div>
                    <div class="form-group">
                        <label for="transmission">Transmission</label>
                        <select id="transmission">${createOptions(transmissions, car.transmission)}</select>
                    </div>
                </div>
                <div class="form-group full-width">
                    <label for="description">Description</label>
                    <textarea id="description">${car.description || ''}</textarea>
                </div>
                <div class="button-group">
                    <button type="button" id="cancelBtn" class="button button-secondary">Cancel</button>
                    <button type="submit" id="submitBtn" class="button button-primary">Save Changes</button>
                </div>
            </form>
        </div>
    `;
}

document.addEventListener('DOMContentLoaded', async () => {
    const params = new URLSearchParams(window.location.search);
    const carId = params.get('carId');
    const container = document.getElementById('edit-car-container');
    const userId = getUserIdOrRedirect();

    if (!carId) {
        container.innerHTML = `<p class="error-message">No car ID provided.</p>`;
        return;
    }

    try {
        const car = await fetchCarDetails(carId);

        if (Number(car.ownerId) !== Number(userId)) {
            showToast('You are not authorized to edit this car.', ToastType.Error);
            setTimeout(() => { window.location.href = 'index.html'; }, 2000);
            return;
        }

        renderEditForm(car);

        const form = document.getElementById('editCarForm');
        const submitBtn = document.getElementById('submitBtn');
        const kmInput = document.getElementById('km');
        const priceInput = document.getElementById('price');

        const formatNumberInput = (e) => {
            const value = e.target.value.replace(/\D/g, '');
            if (value) {
                e.target.value = new Intl.NumberFormat('tr-TR').format(value);
            } else {
                e.target.value = '';
            }
        };

        kmInput.addEventListener('input', formatNumberInput);
        priceInput.addEventListener('input', formatNumberInput);

        form.addEventListener('submit', async (e) => {
            e.preventDefault();

            const brand = document.getElementById('brand').value.trim();
            const model = document.getElementById('model').value.trim();
            const kmStr = kmInput.value.replace(/\./g, '');
            const priceStr = priceInput.value.replace(/\./g, '');

            if (!brand) { showToast('Please enter a brand.', ToastType.Error); return; }
            if (!model) { showToast('Please enter a model.', ToastType.Error); return; }
            if (!kmStr) { showToast('Please enter the kilometers.', ToastType.Error); return; }
            if (!priceStr) { showToast('Please enter a price.', ToastType.Error); return; }
            
            const km = parseInt(kmStr, 10);
            const price = parseFloat(priceStr);

            if (isNaN(km) || km < 0) {
                showToast('Kilometers must be a number, 0 or more.', ToastType.Error);
                return;
            }
            if (isNaN(price) || price <= 0) {
                showToast('Price must be a number greater than 0.', ToastType.Error);
                return;
            }

            const payload = {
                brand,
                model,
                km,
                price,
                fuelType: document.getElementById('fuelType').value,
                transmission: document.getElementById('transmission').value,
                description: document.getElementById('description').value.trim()
            };

            submitBtn.disabled = true;
            submitBtn.textContent = 'Saving...';

            try {
                const response = await fetchWithAuth(`/car/update/${carId}`, {
                    method: 'PUT',
                    body: JSON.stringify(payload)
                });

                if (!response.ok) {
                    throw new Error('Failed to update the car.');
                }

                showToast('Car updated successfully!', ToastType.Success);
                setTimeout(() => {
                    window.location.href = `details.html?carId=${carId}`;
                }, 1500);

            } catch (err) {
                showToast(err.message, ToastType.Error);
                submitBtn.disabled = false;
                submitBtn.textContent = 'Save Changes';
            }
        });

        document.getElementById('cancelBtn').addEventListener('click', () => {
            window.location.href = `details.html?carId=${carId}`;
        });

    } catch (error) {
        container.innerHTML = `<p class="error-message">${error.message}</p>`;
    }
});