import { getUserIdOrRedirect } from './utils/auth.js';
import { fetchWithAuth } from './utils/api.js';
import { formatTimestamp } from './utils/timeutils.js';

function formatDisplayDate(date) {
    return new Date(date).toLocaleString('en-UK', {
        day: 'numeric',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: false
    });
}

async function markNotificationsAsRead(userId) {
    try {
        const response = await fetchWithAuth(`/notification/user/${userId}/mark-all-read`, {
            method: 'PUT'
        });

        if (response.ok) {
            const notificationsLink = document.getElementById('notifications-link');
            if (notificationsLink) {
                const badge = notificationsLink.querySelector('.notification-badge');
                if (badge) {
                    badge.remove();
                }
                notificationsLink.classList.remove('has-badge');
            }
        }
    } catch (error) {
        console.error('Failed to mark notifications as read:', error);
    }
}

async function loadNotifications() {
  const userId = getUserIdOrRedirect();
  const container = document.getElementById('table-container');
  
  try {
    const response = await fetchWithAuth(`/notification/user/${userId}`);
    if (!response.ok) throw new Error('Could not load your notifications.');
    const notifications = await response.json();

    if (notifications && notifications.length > 0) {
        markNotificationsAsRead(userId);
    }

    container.innerHTML = '';

    if (!notifications || notifications.length === 0) {
      container.innerHTML = `<p class="status-message">You have no notifications.</p>`;
      return;
    }

    const table = document.createElement('table');
    table.innerHTML = `
      <thead>
        <tr>
          <th>Message</th>
          <th style="text-align: right;">Date</th>
        </tr>
      </thead>
      <tbody></tbody>
    `;
    const tbody = table.querySelector('tbody');

    notifications.sort((a, b) => new Date(b.sentTime) - new Date(a.sentTime));

    for (const notification of notifications) {
      const row = document.createElement('tr');
      row.className = notification.isRead ? 'read' : 'unread';

      const messageCell = document.createElement('td');
      messageCell.className = 'message-cell';
      
      const pre = document.createElement('pre');
      pre.textContent = notification.messageText;
      messageCell.appendChild(pre);
      
      const timeCell = document.createElement('td');
      timeCell.className = 'time-cell';
      timeCell.textContent = formatDisplayDate(notification.sentTime);
      timeCell.title = formatTimestamp(notification.sentTime, true);
      
      row.appendChild(messageCell);
      row.appendChild(timeCell);
      tbody.appendChild(row);
    }
    container.appendChild(table);

  } catch (error) {
    container.innerHTML = `<p class="status-message">${error.message}</p>`;
  }
}

document.addEventListener('DOMContentLoaded', loadNotifications);