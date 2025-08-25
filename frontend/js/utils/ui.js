export const ToastType = {
    Success: 'success-toast',
    Error: 'error-toast',
    Information: 'info-toast'
};

export function showToast(message, type = ToastType.Information) {
    const container = document.getElementById('toast-container');
    if (!container) {
        console.error('Toast container not found in the DOM!');
        return;
    }

    const toast = document.createElement('div');
    toast.className = `toast-notification ${type}`;
    toast.textContent = message;

    container.appendChild(toast);

    setTimeout(() => {
        toast.remove();
    }, 4000);
}