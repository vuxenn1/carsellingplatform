import { fetchUserProfile } from './auth.js';
import { fetchWithAuth } from './api.js';

async function checkAdmin() {
  try {
    const user = await fetchUserProfile();
    if (!user || !user.isAdmin) window.location.href = 'index.html';
  } catch {
    window.location.href = 'login.html';
  }
}

function getBadgeClass(actionType) {
    const type = actionType.toLowerCase();
    if (type.includes('create') || type.includes('upload') || type.includes('register')) return 'log-create';
    if (type.includes('update') || type.includes('edit')) return 'log-update';
    if (type.includes('delete') || type.includes('deactivate')) return 'log-delete';
    if (type.includes('login') || type.includes('sold') || type.includes('available')) return 'log-status';
    return 'log-info';
}

function showDetailsModal(title, details, actionType) {
    const modal = document.createElement('div');
    modal.className = 'modal-overlay';
    
    const themeClass = getBadgeClass(actionType);

    modal.innerHTML = `
        <div class="modal-content">
            <div class="modal-header ${themeClass}">
                <h2>${title}</h2>
                <button class="modal-close-btn">&times;</button>
            </div>
            <div class="modal-body">
                <pre>${details || 'No details available.'}</pre>
            </div>
        </div>
    `;
    document.body.appendChild(modal);

    const closeModal = () => modal.remove();
    modal.querySelector('.modal-close-btn').addEventListener('click', closeModal);
    modal.addEventListener('click', (e) => {
        if (e.target === modal) {
            closeModal();
        }
    });
}

export async function renderLogTable({ containerId, endpoint, columns, pageTitle }) {
    await checkAdmin();
    const container = document.getElementById(containerId);
    if (!container) return;

    container.innerHTML = `<div class="loader"></div>`;

    try {
        const response = await fetchWithAuth(endpoint);
        if (!response.ok) throw new Error(`Failed to load ${pageTitle} logs.`);
        const logs = await response.json();

        if (!logs || logs.length === 0) {
            container.innerHTML = `<p class="status-message">No ${pageTitle} logs found.</p>`;
            return;
        }

        const table = document.createElement('table');
        const thead = document.createElement('thead');
        const tbody = document.createElement('tbody');

        thead.innerHTML = `<tr>${columns.map(c => `<th class="${c.className || ''}">${c.header}</th>`).join('')}</tr>`;

        logs.forEach(log => {
            const row = document.createElement('tr');
            row.dataset.logId = log.logId;

            columns.forEach(col => {
                const cell = document.createElement('td');
                cell.className = col.className || '';
                let value = log[col.key];

                if (col.isBadge) {
                    cell.innerHTML = `<span class="log-type-badge ${getBadgeClass(value)}" data-action="view-details">${value}</span>`;
                } else if (col.formatter) {
                    const result = col.formatter(value, log);
                    if (typeof result === 'object' && result !== null) {
                        cell.innerHTML = result.html || '';
                        if (result.attributes) {
                            for (const attr in result.attributes) {
                                cell.setAttribute(attr, result.attributes[attr]);
                            }
                        }
                    } else {
                        cell.innerHTML = result;
                    }
                } else {
                    cell.textContent = value;
                }
                row.appendChild(cell);
            });
            tbody.appendChild(row);
        });

        tbody.addEventListener('click', (e) => {
            const badge = e.target.closest('[data-action="view-details"]');
            if (!badge) return;

            const row = badge.closest('tr');
            const logId = parseInt(row.dataset.logId, 10);
            const logData = logs.find(l => l.logId === logId);

            if (logData) {
                showDetailsModal(`Log #${logId} Summary`, logData.actionDetails, logData.actionType);
            }
        });

        table.appendChild(thead);
        table.appendChild(tbody);
        container.innerHTML = '';
        container.appendChild(table);

    } catch (error) {
        container.innerHTML = `<p class="status-message">${error.message}</p>`;
    }
}