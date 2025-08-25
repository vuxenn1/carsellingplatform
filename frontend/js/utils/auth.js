import { showToast, ToastType } from './ui.js';
import { fetchWithAuth } from './api.js';

const TOKEN_KEY = 'userToken';
let profilePromise = null;

export function getToken() {
    return localStorage.getItem(TOKEN_KEY);
}

function saveToken(token) {
    localStorage.setItem(TOKEN_KEY, token);
}

function removeToken() {
    localStorage.removeItem(TOKEN_KEY);
}

function parseJwt(token) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
    } catch (e) {
        return null;
    }
}

export function getUserId() {
    const token = getToken();
    if (!token) return null;
    const decoded = parseJwt(token);
    return decoded?.userId || null;
}

export function getCurrentUser() {
    const token = getToken();
    if (!token) return null;
    const decoded = parseJwt(token);
    if (!decoded) return null;

    return {
        username: decoded.sub,
        userId: decoded.userId,
        isAdmin: decoded.role === 'Admin'
    };
}

export function fetchUserProfile(forceRefresh = false) {
    if (profilePromise && !forceRefresh) {
        return profilePromise;
    }

    profilePromise = (async () => {
        const userId = getUserId();
        if (!userId) {
            throw new Error("User not logged in.");
        }
        try {
            const response = await fetchWithAuth(`/user/profile/${userId}`);
            if (!response.ok) {
                throw new Error("Failed to fetch user profile.");
            }
            return await response.json();
        } catch (error) {
            profilePromise = null;
            throw error;
        }
    })();
    return profilePromise;
}

export function logout() {
    removeToken();
    profilePromise = null;
}

export function getUserIdOrRedirect(isLoginRequired = true) {
    const id = getUserId();
    if (!id && isLoginRequired) {
        window.location.href = 'login.html';
        return null;
    }
    return id;
}

export function redirectIfLoggedIn(toPage = 'index.html') {
    if (getToken()) {
        window.location.href = toPage;
    }
}

export function login(token) {
    saveToken(token);
    showToast('Login successful! Welcome.', ToastType.Success);
    setTimeout(() => {
        window.location.href = 'index.html';
    }, 1000);
}
