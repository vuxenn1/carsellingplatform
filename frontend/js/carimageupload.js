import { getUserIdOrRedirect } from './utils/auth.js';
import { showToast, ToastType } from './utils/ui.js';
import { fetchWithAuth } from './utils/api.js';

document.addEventListener('DOMContentLoaded', () => {
    const userId = getUserIdOrRedirect();
    const carSelect = document.getElementById('carId');
    const imageFieldsContainer = document.getElementById('imageFieldsContainer');
    const addImageBtn = document.getElementById('addImageBtn');
    const form = document.getElementById('imageUploadForm');
    const submitBtn = document.getElementById('submitBtn');

    const fetchUserCars = async () => {
        try {
            const res = await fetchWithAuth(`/car/user/${userId}`);
            if (!res.ok) throw new Error('Could not fetch your cars.');
            const cars = await res.json();
            
            if (!cars || cars.length === 0) {
                showToast('You must list a car before adding images. Redirecting...', ToastType.Information);
                setTimeout(() => { window.location.href = 'uploadcar.html'; }, 2500);
                return;
            }
            
            carSelect.disabled = false;
            carSelect.innerHTML = `<option value="" disabled selected>Select a car</option>`;
            cars.forEach(car => {
                const opt = document.createElement('option');
                opt.value = car.carId;
                opt.textContent = `${car.brand} ${car.model} (${car.year})`;
                carSelect.appendChild(opt);
            });
        } catch (err) {
            showToast(err.message, ToastType.Error);
        }
    };

    const addImageField = () => {
        const fieldId = `file-${Date.now()}`;
        const div = document.createElement('div');
        div.classList.add('image-field');
        div.innerHTML = `
            <div class="image-preview">
                <span class="image-preview-placeholder">Image Preview</span>
            </div>
            <div class="file-and-alt">
                <label for="${fieldId}" class="custom-file-input">
                    <span>Select a file...</span>
                    <input type="file" id="${fieldId}" class="image-file" accept="image/jpeg, image/png, image/jpg" required />
                </label>
                <input type="text" placeholder="(Optional) Description of the image" class="image-alt" />
            </div>
            <button type="button" class="remove-btn" title="Remove image">&times;</button>
        `;
        imageFieldsContainer.appendChild(div);

        const fileInput = div.querySelector('.image-file');
        const fileLabel = div.querySelector('.custom-file-input span');
        const previewContainer = div.querySelector('.image-preview');

        fileInput.addEventListener('change', () => {
            const file = fileInput.files[0];
            if (file) {
                fileLabel.textContent = file.name;
                const reader = new FileReader();
                reader.onload = (e) => {
                    previewContainer.innerHTML = `<img src="${e.target.result}" alt="Image preview"/>`;
                };
                reader.readAsDataURL(file);
            } else {
                fileLabel.textContent = 'Select a file...';
                previewContainer.innerHTML = `<span class="image-preview-placeholder">Image Preview</span>`;
            }
        });

        div.querySelector('.remove-btn').addEventListener('click', () => {
            div.remove();
        });
    };

    addImageBtn.addEventListener('click', addImageField);

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const carId = parseInt(carSelect.value, 10);
        if (isNaN(carId)) {
            showToast('Please select a car first.', ToastType.Error);
            return;
        }

        const imageFields = imageFieldsContainer.querySelectorAll('.image-field');
        if (imageFields.length === 0) {
            showToast('Please add at least one image.', ToastType.Error);
            return;
        }

        const formData = new FormData();
        formData.append('carId', carId);

        let fileCount = 0;
        for (const field of imageFields) {
            const fileInput = field.querySelector('.image-file');
            const altText = field.querySelector('.image-alt').value.trim();
            
            if (fileInput.files.length > 0) {
                formData.append('images', fileInput.files[0]);
                formData.append('altTexts', altText || 'Car image');
                fileCount++;
            }
        }

        if (fileCount === 0) {
            showToast('Please select at least one image file to upload.', ToastType.Error);
            return;
        }

        submitBtn.disabled = true;
        submitBtn.textContent = 'Uploading...';

        try {
            const response = await fetchWithAuth('/carimage/upload', {
                method: 'POST',
                body: formData,
            });

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: 'An unknown error occurred.' }));
                throw new Error(errorData.message || 'An error occurred during upload.');
            }

            const successMessage = fileCount > 1 
                ? `${fileCount} images uploaded successfully!` 
                : 'Image uploaded successfully!';
            showToast(successMessage, ToastType.Success);
            
            setTimeout(() => {
                window.location.href = `details.html?carId=${carId}`;
            }, 1500);

        } catch (err) {
            showToast(err.message, ToastType.Error);
            submitBtn.disabled = false;
            submitBtn.textContent = 'Upload Images';
        }
    });

    fetchUserCars();
    addImageField();
});