import { fetchUserProfile, logout } from './utils/auth.js';
import { showToast, ToastType } from './utils/ui.js';

document.addEventListener('DOMContentLoaded', async () => {
    const profileBox = document.getElementById('profile-box');
    const mainContainer = document.querySelector('.main-container');

    const renderProfile = (user) => {
        const joinedDate = new Date(user.recordTime);
        const displayDate = joinedDate.toLocaleDateString('en-GB', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
        const titleDate = joinedDate.toLocaleString('en-GB', {
            dateStyle: 'full',
            timeStyle: 'medium'
        });

        profileBox.innerHTML = `
            <div class="profile-section">
                <h3>Contact Information</h3>
                <div class="info-list">
                    <div class="info-row">
                        <span class="label">Username</span>
                        <span class="value">${user.username}</span>
                    </div>
                    <div class="info-row">
                        <span class="label">Email</span>
                        <span class="value">${user.mail}</span>
                    </div>
                    <div class="info-row">
                        <span class="label">Phone</span>
                        <span class="value">${user.phone || 'Not provided'}</span>
                    </div>
                    <div class="info-row">
                        <span class="label">Location</span>
                        <span class="value">${user.userLocation || 'Not provided'}</span>
                    </div>
                </div>
            </div>
            <div class="profile-section">
                <h3>Account Information</h3>
                <div class="info-list">
                    <div class="info-row">
                        <span class="label">Status</span>
                        <span class="value">${user.isActive ? 'Active' : 'Inactive'}</span>
                    </div>
                    <div class="info-row">
                        <span class="label">Member Since</span>
                        <span class="value" title="${titleDate}">${displayDate}</span>
                    </div>
                    ${user.isAdmin ? `
                        <div class="info-row">
                            <span class="label">Role</span>
                            <span class="value">Administrator</span>
                        </div>
                    ` : ''}
                </div>
            </div>
        `;
    };

    mainContainer.addEventListener('click', (e) => {
        const editBtn = e.target.closest('#edit-btn');
        const logoutBtn = e.target.closest('#logout-btn');

        if (editBtn) {
            window.location.href = 'editprofile.html';
        }

        if (logoutBtn) {
            showToast('You have been logged out.', ToastType.Information);
            logout();
            setTimeout(() => {
                window.location.href = 'login.html';
            }, 1500);
        }
    });

    try {
        const user = await fetchUserProfile();
        renderProfile(user);
    } catch (error) {
        if (!localStorage.getItem('authToken')) {
            window.location.href = 'login.html';
        } else {
            profileBox.innerHTML = `<p class="status-message">Could not load your profile. Please try refreshing the page.</p>`;
        }
    }
});