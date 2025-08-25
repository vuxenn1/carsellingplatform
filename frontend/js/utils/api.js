import { getToken, logout } from './auth.js';

const API_BASE_URL = 'http://localhost:2525/api';

export async function fetchWithAuth(endpoint, options = {}) {
    const token = getToken();

    const headers = {
        'Content-Type': 'application/json',
        ...options.headers,
    };

    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        ...options,
        headers,
    });

    if (response.status === 401) {
        logout();
        window.location.href = 'login.html';
        throw new Error('Session expired. Please log in again.');
    }

    return response;
}