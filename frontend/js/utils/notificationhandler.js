import { fetchWithAuth } from './api.js';

async function fetchJson(endpoint) {
    try {
        const res = await fetchWithAuth(endpoint);
        if (!res.ok) return null;
        return res.json();
    } catch {
        return null;
    }
}

export async function sendOfferResponseNotification({ action, updatedOffer, carNameMap, userId }) {
    if (!updatedOffer || !userId) return;

    const [car, owner] = await Promise.all([
        fetchJson(`/car/details/${updatedOffer.carId}`),
        fetchJson(`/user/profile/${userId}`)
    ]);

    const carName = car ? `${car.brand} ${car.model} (${car.year})` : carNameMap[updatedOffer.carId] || `Car #${updatedOffer.carId}`;
    const offerPrice = Number(updatedOffer.offerPrice).toLocaleString('tr-TR');

    const message = action === 'accept'
        ? `Your "₺${offerPrice}" offer for the "${carName}" has been accepted.\nContact Info:\nName: ${owner?.username}\nPhone: ${owner?.phone}\nEmail: ${owner?.mail}`
        : `Your ₺${offerPrice} offer for "${carName}" has been rejected by the owner.`;
    
    await fetchWithAuth(`/notification/send`, {
        method: 'POST',
        body: JSON.stringify({
            MessageText: message,
            ReceiverId: updatedOffer.senderId,
        })
    });
}