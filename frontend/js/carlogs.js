import { renderLogTable } from './utils/log-renderer.js';
import { formatTimestamp } from './utils/timeutils.js';

const carLogConfig = {
    containerId: 'log-container',
    endpoint: '/log/car',
    pageTitle: 'Car',
    columns: [
        { 
            header: 'Car ID', 
            key: 'carId',
            formatter: (val) => `<a href="details.html?carId=${val}">${val}</a>`
        },
        { 
            header: 'Action', 
            key: 'actionType', 
            isBadge: true
        },
        { 
            header: 'Timestamp', 
            key: 'actionTime', 
            className: 'align-right',
            formatter: (val) => {
                const date = new Date(val);
                return {
                    html: date.toLocaleString('en-GB', { dateStyle: 'medium', timeStyle: 'short' }),
                    attributes: {
                        title: formatTimestamp(date, true)
                    }
                };
            }
        }
    ]
};

document.addEventListener('DOMContentLoaded', () => {
    renderLogTable(carLogConfig);
});