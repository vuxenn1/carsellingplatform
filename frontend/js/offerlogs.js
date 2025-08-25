import { renderLogTable } from './utils/log-renderer.js';
import { formatTimestamp } from './utils/timeutils.js';

const offerLogConfig = {
    containerId: 'log-container',
    endpoint: '/log/offer',
    pageTitle: 'Offer',
    columns: [
        { 
            header: 'Offer ID', 
            key: 'offerId' 
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
    renderLogTable(offerLogConfig);
});