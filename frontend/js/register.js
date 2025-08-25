import { redirectIfLoggedIn, login } from './utils/auth.js';
import { showToast, ToastType } from './utils/ui.js';
import { fetchWithAuth } from './utils/api.js';

redirectIfLoggedIn();

document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('registerForm');
    const submitBtn = document.getElementById('submitBtn');

    const validateForm = (user) => {
        if (!user.Username || !user.Password || !user.Mail || !user.Phone || !user.UserLocation) {
            return 'All fields are required.';
        }
        
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(user.Mail)) {
            return 'Please enter a valid email address.';
        }

        if (user.Password.length < 8) {
            return 'Password must be at least 8 characters long.';
        }
        return null;
    };

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        submitBtn.disabled = true;
        submitBtn.textContent = 'Creating Account...';

        const city = document.getElementById('city').value.trim();
        const country = document.getElementById('country').value.trim();

        const user = {
            Username: document.getElementById('username').value.trim(),
            Password: document.getElementById('password').value,
            Mail: document.getElementById('mail').value.trim(),
            Phone: document.getElementById('phone').value.trim(),
            UserLocation: city && country ? `${city}, ${country}` : '',
            IsAdmin: false
        };

        const validationError = validateForm(user);
        if (validationError) {
            showToast(validationError, ToastType.Error);
            submitBtn.disabled = false;
            submitBtn.textContent = 'Create Account';
            return;
        }

        try {
            const response = await fetchWithAuth('/user/register', {
                method: 'POST',
                body: JSON.stringify(user)
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText);
            }

            const data = await response.json();

            if (!data || !data.token) {
                throw new Error('Registration succeeded, but failed to log in.');
            }

            login(data.token);

        } catch (err) {
            const errorMessage = err.message.includes('ORA-') 
                ? 'Username, email, or phone number already in use.' 
                : err.message;
            showToast(errorMessage, ToastType.Error);
            submitBtn.disabled = false;
            submitBtn.textContent = 'Create Account';
        }
    });
});