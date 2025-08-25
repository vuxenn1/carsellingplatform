export function initializeCarousel({ images }) {
  const carousel = document.getElementById('carousel');
  const thumbnails = document.getElementById('thumbnail-strip');
  const defaultImageUrl = '../images/default-car.png';
  
  if (!carousel || !thumbnails) return;

  if (!images || images.length === 0) {
    carousel.innerHTML = `<img src="${defaultImageUrl}" alt="Default Car Image" class="active">`;
    return;
  }
  
  let currentImageIndex = 0;
  
  images.forEach((img, index) => {
    const imageEl = document.createElement('img');
    imageEl.src = img.imageUrl || defaultImageUrl;
    imageEl.alt = img.altText || `Car image ${index + 1}`;
    imageEl.onerror = () => { imageEl.src = defaultImageUrl; };
    if (index === 0) imageEl.classList.add('active');
    carousel.appendChild(imageEl);
    
    const thumbEl = imageEl.cloneNode();
    thumbEl.addEventListener('click', () => showImage(index));
    thumbnails.appendChild(thumbEl);
  });
  
  const allImages = carousel.querySelectorAll('img');
  const allThumbs = thumbnails.querySelectorAll('img');

  function showImage(index) {
    allImages.forEach(img => img.classList.remove('active'));
    allThumbs.forEach(thumb => thumb.classList.remove('active'));
    allImages[index].classList.add('active');
    allThumbs[index].classList.add('active');
    currentImageIndex = index;
  }
  showImage(0);

  if (images.length > 1) {
    const controls = document.createElement('div');
    controls.className = 'carousel-controls';
    controls.innerHTML = `<button id="prevBtn">&#10094;</button><button id="nextBtn">&#10095;</button>`;
    carousel.appendChild(controls);
    
    document.getElementById('prevBtn').addEventListener('click', () => {
      const newIndex = (currentImageIndex - 1 + images.length) % images.length;
      showImage(newIndex);
    });
    
    document.getElementById('nextBtn').addEventListener('click', () => {
      const newIndex = (currentImageIndex + 1) % images.length;
      showImage(newIndex);
    });
  }
}