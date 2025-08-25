import { redirectIfLoggedIn, login } from './utils/auth.js';
import { showToast, ToastType } from './utils/ui.js';
import { fetchWithAuth } from './utils/api.js';

redirectIfLoggedIn();

document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('loginForm');
    const submitBtn = document.getElementById('submitBtn');

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        submitBtn.disabled = true;
        submitBtn.textContent = 'Logging in...';

        const username = document.getElementById('username').value.trim();
        const password = document.getElementById('password').value;

        try {
            const response = await fetchWithAuth('/user/login', {
                method: 'POST',
                body: JSON.stringify({ Username: username, Password: password })
            });

            if (!response.ok) {
                throw new Error('Invalid username or password.');
            }
            
            const data = await response.json();
            login(data.token);

        } catch (err) {
            showToast(err.message, ToastType.Error);
            submitBtn.disabled = false;
            submitBtn.textContent = 'Login';
        }
    });
});