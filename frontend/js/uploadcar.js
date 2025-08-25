import { getUserIdOrRedirect } from './utils/auth.js';
import { showToast, ToastType } from './utils/ui.js';
import { fetchWithAuth } from './utils/api.js';

document.addEventListener('DOMContentLoaded', () => {
    const userId = getUserIdOrRedirect();
    const form = document.getElementById('uploadCarForm');
    const submitBtn = document.getElementById('submitBtn');

    form.addEventListener('submit', async e => {
        e.preventDefault();

        const brand = document.getElementById('brand').value.trim();
        const model = document.getElementById('model').value.trim();
        const yearStr = document.getElementById('year').value.trim();
        const kmStr = document.getElementById('km').value.trim();
        const priceStr = document.getElementById('price').value.trim();
        const fuelType = document.getElementById('fuelType').value;
        const transmission = document.getElementById('transmission').value;
        const description = document.getElementById('description').value.trim();

        if (!brand) { 
            showToast('Please enter a brand.', ToastType.Error); 
            return; 
        }
        if (!model) { 
            showToast('Please enter a model.', ToastType.Error); 
            return; 
        }
        if (!yearStr) { 
            showToast('Please enter a year.', ToastType.Error); 
            return; 
        }
        if (!kmStr) { 
            showToast('Please enter the kilometers.', ToastType.Error); 
            return; 
        }
        if (!priceStr) { 
            showToast('Please enter a price.', ToastType.Error); 
            return; 
        }
        if (!fuelType) { 
            showToast('Please select a fuel type.', ToastType.Error); 
            return; 
        }
        if (!transmission) { 
            showToast('Please select a transmission type.', ToastType.Error); 
            return; 
        }

        const year = parseInt(yearStr, 10);
        const km = parseInt(kmStr, 10);
        const price = parseFloat(priceStr);
        const nowYear = new Date().getFullYear();

        if (isNaN(year) || year < 1950 || year > nowYear + 1) {
            showToast('Please enter a valid year (e.g., 2025).', ToastType.Error);
            return;
        }
        if (isNaN(km) || km < 0) {
            showToast('Kilometers must be a number, 0 or more.', ToastType.Error);
            return;
        }
        if (isNaN(price) || price <= 0) {
            showToast('Price must be a number greater than 0.', ToastType.Error);
            return;
        }

        const car = {
            OwnerId: parseInt(userId, 10),
            Brand: brand,
            Model: model,
            Year: year,
            KM: km,
            FuelType: fuelType,
            Transmission: transmission,
            Price: price,
            Description: description
        };

        submitBtn.disabled = true;
        submitBtn.textContent = 'Listing Car...';

        try {
            const response = await fetchWithAuth('/car/upload', {
                method: 'POST',
                body: JSON.stringify(car)
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || 'Failed to list your car.');
            }

            await response.json();
            showToast('Car listed successfully! Now add some images.', ToastType.Success);
            setTimeout(() => {
                window.location.href = 'carimageupload.html';
            }, 2000);

        } catch (err) {
            showToast(err.message, ToastType.Error);
            submitBtn.disabled = false;
            submitBtn.textContent = 'List My Car';
        }
    });
});