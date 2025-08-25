import { getUserIdOrRedirect } from './utils/auth.js';
import { sendOfferResponseNotification } from './utils/notificationhandler.js';
import { showToast, ToastType } from './utils/ui.js';
import { fetchWithAuth } from './utils/api.js';

const userId = getUserIdOrRedirect();

async function fetchJson(endpoint, options) {
  try {
    const res = await fetchWithAuth(endpoint, options);
    if (!res.ok) throw new Error(`Request failed: ${res.status}`);
    return res.json();
  } catch (error) {
    console.error(`Fetch error for ${endpoint}:`, error);
    showToast(`Failed to load data. Please try again.`, ToastType.Error);
    return null;
  }
}

function createOfferRow(offer, type, carNameMap) {
    const row = document.createElement('tr');
    row.dataset.offerId = offer.offerId;
    const status = (offer.offerStatus || '').toLowerCase();
    const carName = carNameMap[offer.carId] || `Car #${offer.carId}`;
    const price = Number(offer.offerPrice).toLocaleString('tr-TR', { style: 'currency', currency: 'TRY', minimumFractionDigits: 0 });
    const date = new Date(offer.offerTime).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' });

    row.innerHTML = `
        <td><a href="details.html?carId=${offer.carId}">${carName}</a></td>
        <td>${price}</td>
        <td>${date}</td>
        <td><span class="status-badge ${status}">${status}</span></td>
        <td class="actions-cell"></td>
    `;

    if (type === 'received') {
        const actionCell = row.querySelector('.actions-cell');
        if (status === 'pending') {
            const actionGroup = document.createElement('div');
            actionGroup.className = 'action-group';

            const acceptBtn = document.createElement('button');
            acceptBtn.className = 'action-btn btn-accept';
            acceptBtn.dataset.action = 'accept';
            acceptBtn.textContent = 'Accept';

            const rejectBtn = document.createElement('button');
            rejectBtn.className = 'action-btn btn-reject';
            rejectBtn.dataset.action = 'reject';
            rejectBtn.textContent = 'Reject';
            
            actionGroup.append(acceptBtn, rejectBtn);
            actionCell.append(actionGroup);
        } else {
            actionCell.textContent = 'Responded';
        }
    } else {
        row.querySelector('.actions-cell').remove();
    }
    return row;
}

async function renderTable(type, containerId, carNameMap) {
    const container = document.getElementById(containerId);
    const data = await fetchJson(`/offer/${type}/${userId}`);
    
    container.innerHTML = '';
    if (!data || data.length === 0) {
        container.innerHTML = `<p class="status-message">No ${type} offers found.</p>`;
        return;
    }

    const table = document.createElement('table');
    table.className = 'offer-table';
    const isReceived = type === 'received';
    table.innerHTML = `
        <thead>
            <tr>
                <th>Offered Car</th>
                <th>Price</th>
                <th>Date</th>
                <th>Status</th>
                ${isReceived ? '<th class="actions-cell">Actions</th>' : ''}
            </tr>
        </thead>
        <tbody></tbody>
    `;
    const tbody = table.querySelector('tbody');
    data.sort((a, b) => new Date(b.offerTime) - new Date(a.offerTime));
    data.forEach(offer => tbody.appendChild(createOfferRow(offer, type, carNameMap)));
    
    container.appendChild(table);
}

async function respondToOffer(offerId, action, carNameMap) {
    try {
        const res = await fetchWithAuth(`/offer/${action}/${offerId}`, { method: 'PUT' });
        if (!res.ok) throw new Error('Offer update failed');

        const offers = await fetchJson(`/offer/received/${userId}`);
        const updatedOffer = (offers || []).find(o => o.offerId === offerId);
        if (!updatedOffer) throw new Error('Offer not found after update');

        await sendOfferResponseNotification({ action, updatedOffer, carNameMap, userId });
        
        showToast(`Offer ${action}ed and notification sent.`);
        return updatedOffer;

    } catch (err) {
        showToast(err.message || 'Something went wrong.', ToastType.Error);
        return null;
    }
}

document.addEventListener('DOMContentLoaded', async () => {
    const cars = await fetchJson('/car/all');
    const carNameMap = (cars || []).reduce((map, car) => {
        map[car.carId] = `${car.brand} ${car.model}`;
        return map;
    }, {});

    await renderTable('sent', 'sent-container', carNameMap);
    await renderTable('received', 'received-container', carNameMap);

    document.querySelectorAll('.tab-button').forEach(button => {
        button.addEventListener('click', () => {
            document.querySelectorAll('.tab-button').forEach(b => b.classList.remove('active'));
            document.querySelectorAll('.tab-content').forEach(tab => tab.classList.remove('active'));
            button.classList.add('active');
            document.getElementById(button.dataset.tab).classList.add('active');
        });
    });

    document.getElementById('received-container').addEventListener('click', async (e) => {
        const button = e.target.closest('.action-btn');
        if (!button) return;

        const row = button.closest('tr');
        const offerId = Number(row.dataset.offerId);
        const action = button.dataset.action;

        const actionGroup = button.parentElement;
        actionGroup.innerHTML = 'Processing...';

        const updatedOffer = await respondToOffer(offerId, action, carNameMap);
        if (updatedOffer) {
            const newRow = createOfferRow(updatedOffer, 'received', carNameMap);
            row.replaceWith(newRow);
        } else {
            await renderTable('received', 'received-container', carNameMap);
        }
    });
});