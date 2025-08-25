import { showToast, ToastType } from '../utils/ui.js';
import { fetchWithAuth } from '../utils/api.js';

export function showOfferModal({ car, userId }) {
    const modal = document.createElement('div');
    modal.className = 'modal-overlay';
    modal.innerHTML = `
        <div class="modal-content">
            <h2>Make an Offer</h2>
            <div class="form-group">
                <label for="offer-price">Your Offer</label>
                <input type="text" id="offer-price" placeholder="Offer a price (TL)" inputmode="numeric">
            </div>
            <div class="modal-buttons">
                <button id="cancel-offer-btn">Cancel</button>
                <button id="submit-offer-btn">Submit Offer</button>
            </div>
        </div>
    `;
    document.body.appendChild(modal);

    const offerInput = modal.querySelector('#offer-price');
    offerInput.addEventListener('input', (e) => {
        const value = e.target.value.replace(/\D/g, '');
        if (value) {
            e.target.value = new Intl.NumberFormat('tr-TR').format(value);
        } else {
            e.target.value = '';
        }
    });

    const closeModal = () => document.body.removeChild(modal);

    modal.querySelector('#cancel-offer-btn').addEventListener('click', closeModal);
    modal.addEventListener('click', (e) => {
        if (e.target === modal) closeModal();
    });

    modal.querySelector('#submit-offer-btn').addEventListener('click', async () => {
        const cleanValue = offerInput.value.replace(/\./g, '');
        const offerPrice = parseFloat(cleanValue);

        if (isNaN(offerPrice) || offerPrice <= 0) {
            showToast('Please enter a valid offer amount.', ToastType.Error);
            return;
        }

        const body = {
            CarId: car.carId,
            SenderId: Number(userId),
            ReceiverId: car.ownerId,
            OfferPrice: offerPrice
        };

        try {
            const response = await fetchWithAuth('/offer/create', {
                method: 'POST',
                body: JSON.stringify(body)
            });

            if (response.ok) {
                showToast('Your offer has been submitted successfully.', ToastType.Success);
                closeModal();
            } else {
                const errText = await response.text();
                showToast(`Failed to submit offer: ${errText}`, ToastType.Error);
            }
        } catch (err) {
            showToast('An error occurred while submitting your offer.', ToastType.Error);
        }
    });
}