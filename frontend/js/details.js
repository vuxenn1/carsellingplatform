import { getUserIdOrRedirect } from './utils/auth.js';
import { initializeCarousel } from './ui/carousel.js';
import { renderActionButtons } from './ui/actions.js';
import { formatTimestamp } from './utils/timeutils.js';
import { fetchWithAuth } from './utils/api.js';

function renderPage(car) {
  const container = document.getElementById('car-details-container');
  const price = Number(car.price).toLocaleString('tr-TR', { style: 'currency', currency: 'TRY', minimumFractionDigits: 0 });
  const km = Number(car.km).toLocaleString('tr-TR');
  const listDate = new Date(car.listDate);
  const displayDate = listDate.toLocaleDateString('en-GB', { dateStyle: 'long' });
  const titleDate = formatTimestamp(car.listDate, false);

  container.innerHTML = `
    <div class="details-layout">
      <div class="gallery-column">
        <div class="image-carousel" id="carousel"></div>
        <div class="thumbnail-strip" id="thumbnail-strip"></div>
      </div>
      <div class="info-column">
        <div id="owner-actions-container"></div>
        <h1>${car.brand} ${car.model}</h1>
        <p class="price-highlight">${price}</p>
        <div class="details-section">
          <h2>Specifications</h2>
          <div class="specs-grid">
            <div class="spec-item"><span>Year</span><span>${car.year}</span></div>
            <div class="spec-item"><span>Kilometers</span><span>${km} km</span></div>
            <div class="spec-item"><span>Fuel</span><span>${car.fuelType}</span></div>
            <div class="spec-item"><span>Transmission</span><span>${car.transmission}</span></div>
            <div class="spec-item" title="${titleDate}"><span>List Date</span><span>${displayDate}</span></div>
          </div>
        </div>
        <div class="details-section">
          <h2>Description</h2>
          <div class="description-box"><p>${car.description || 'No description provided.'}</p></div>
        </div>
        <div class="details-section">
          <h2>Seller Information</h2>
          <div class="specs-grid">
            <div class="spec-item"><span>Username</span><span>${car.ownerUsername}</span></div>
            <div class="spec-item"><span>Location</span><span>${car.ownerLocation || 'Not specified'}</span></div>
          </div>
        </div>
        <div class="action-buttons" id="action-buttons"></div>
      </div>
    </div>
  `;
}

function renderOwnerActions(carId) {
  const container = document.getElementById('owner-actions-container');
  if (!container) return;
  container.innerHTML = `
    <a href="editcar.html?carId=${carId}" class="button button-edit">Edit Car Listing</a>
  `;
}

document.addEventListener('DOMContentLoaded', async () => {
  const params = new URLSearchParams(window.location.search);
  const carId = params.get('carId');
  const detailsContainer = document.getElementById('car-details-container');
  
  if (!carId) {
    detailsContainer.innerHTML = `<p class="error-message">No car ID provided.</p>`;
    return;
  }

  const userId = getUserIdOrRedirect(false);

  try {
    const [carDetailsResponse, imagesResponse, isFavoriteResponse] = await Promise.all([
      fetchWithAuth(`/car/details/${carId}`),
      fetchWithAuth(`/carimage/${carId}`),
      userId ? fetchWithAuth(`/user/favorite/check?userId=${userId}&carId=${carId}`) : Promise.resolve(null)
    ]);

    if (!carDetailsResponse.ok) throw new Error('Failed to load car details.');

    const car = await carDetailsResponse.json();
    const images = imagesResponse.ok ? await imagesResponse.json() : [];
    const favJson = isFavoriteResponse && isFavoriteResponse.ok ? await isFavoriteResponse.json() : null;
    const isFavorite = favJson ? !!favJson.isFavorited : false;

    renderPage(car);
    initializeCarousel({ images });

    const userIdNum = Number(userId);
    const ownerIdNum = Number(car.ownerId);

    if (userId && userIdNum === ownerIdNum) {
      renderOwnerActions(car.carId);
    } else if (userId) {
      renderActionButtons({ car, isFavorite });
    }
  } catch (error) {
    detailsContainer.innerHTML = `<p class="error-message">${error.message}</p>`;
  }
});