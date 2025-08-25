import { getUserIdOrRedirect } from './utils/auth.js';
import { showToast, ToastType } from './utils/ui.js';
import { fetchWithAuth } from './utils/api.js';

document.addEventListener('DOMContentLoaded', async () => {
    const userId = getUserIdOrRedirect();
    const form = document.getElementById('editForm');
    const mailInput = document.getElementById('mail');
    const phoneInput = document.getElementById('phone');
    const locationInput = document.getElementById('location');
    const oldPasswordInput = document.getElementById('oldPassword');
    const newPasswordInput = document.getElementById('newPassword');
    const confirmPasswordInput = document.getElementById('confirmPassword');
    const submitBtn = document.getElementById('submitBtn');
    const allInputs = [mailInput, phoneInput, locationInput, oldPasswordInput, newPasswordInput, confirmPasswordInput];

    const clearInputErrors = () => {
        allInputs.forEach(input => input.classList.remove('input-error'));
    };

    const isProfileDetailsValid = () => {
        const mail = mailInput.value.trim();
        if (!mail) {
            showToast('Email address cannot be empty.', ToastType.Error);
            mailInput.classList.add('input-error');
            return false;
        }
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(mail)) {
            showToast('Please enter a valid email address.', ToastType.Error);
            mailInput.classList.add('input-error');
            return false;
        }
        return true;
    };

    const isPasswordChangeValid = () => {
        const oldPass = oldPasswordInput.value;
        const newPass = newPasswordInput.value;
        const confirmPass = confirmPasswordInput.value;

        if (!oldPass && !newPass && !confirmPass) {
            return true;
        }

        if (!oldPass) {
            showToast('Please enter your current password to set a new one.', ToastType.Error);
            oldPasswordInput.classList.add('input-error');
            return false;
        }
        if (newPass === oldPass) {
            showToast('New password cannot be the same as the current password.', ToastType.Error);
            newPasswordInput.classList.add('input-error');
            return false;
        }
        if (newPass.length < 8) {
            showToast('New password must be at least 8 characters long.', ToastType.Error);
            newPasswordInput.classList.add('input-error');
            return false;
        }
        if (newPass !== confirmPass) {
            showToast('New passwords do not match.', ToastType.Error);
            newPasswordInput.classList.add('input-error');
            confirmPasswordInput.classList.add('input-error');
            return false;
        }
        return true;
    };

    try {
        const response = await fetchWithAuth(`/user/profile/${userId}`);
        if (!response.ok) throw new Error('Could not load profile.');
        const user = await response.json();
        
        mailInput.value = user.mail || '';
        phoneInput.value = user.phone || '';
        locationInput.value = user.userLocation || '';

        document.getElementById('loader-container').style.display = 'none';
        document.getElementById('form-content').style.display = 'block';
    } catch {
        showToast('Could not load your profile. Please try again later.', ToastType.Error);
        document.getElementById('loader-container').style.display = 'none';
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        clearInputErrors();

        if (!isProfileDetailsValid() || !isPasswordChangeValid()) {
            return;
        }

        const payload = {
            Mail: mailInput.value.trim(),
            Phone: phoneInput.value.trim(),
            UserLocation: locationInput.value.trim(),
            Password: newPasswordInput.value || null,
            OldPassword: oldPasswordInput.value || null,
        };

        submitBtn.disabled = true;
        submitBtn.textContent = 'Saving...';

        try {
            const response = await fetchWithAuth(`/user/edit/${userId}`, {
                method: 'PUT',
                body: JSON.stringify(payload)
            });
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || 'Failed to update profile.');
            }
            
            showToast('Profile updated successfully! Redirecting...', ToastType.Success);
            setTimeout(() => { window.location.href = 'myprofile.html'; }, 1500);
        } catch (error) {
            showToast(error.message, ToastType.Error);
            submitBtn.disabled = false;
            submitBtn.textContent = 'Save Changes';
        }
    });

    document.getElementById('cancelBtn').addEventListener('click', () => {
        window.location.href = 'myprofile.html';
    });
});