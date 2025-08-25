import { showOfferModal } from './offer-modal.js'
import { showToast, ToastType } from '../utils/ui.js'
import { getUserId } from '../utils/auth.js'
import { fetchWithAuth } from '../utils/api.js'

export function renderActionButtons({ car, isFavorite }) {
    const actionsContainer = document.getElementById('action-buttons')
    if (!actionsContainer) return

    let favState = !!isFavorite
    const userId = getUserId()

    const favBtn = document.createElement('button')
    favBtn.className = 'button'
    const updateFavButton = () => {
        favBtn.textContent = favState ? 'In Favorites' : 'Add to Favorites'
        favBtn.classList.toggle('remove-fav', favState)
        favBtn.classList.toggle('add-fav', !favState)
    }
    updateFavButton()

    favBtn.addEventListener('click', async () => {
        const currentUserId = getUserId();
        if (!currentUserId) {
            showToast('You must be logged in to add favorites.', ToastType.Error);
            window.location.href = 'login.html';
            return;
        }

        favBtn.disabled = true
        favBtn.textContent = 'Updating...'

        const endpoint = `/user/favorite/${favState ? 'remove' : 'add'}`
        const body = JSON.stringify({ UserId: Number(currentUserId), CarId: Number(car.carId) })

        try {
            const res = await fetchWithAuth(endpoint, { method: 'POST', body: body })
            if (!res.ok) throw new Error()
            favState = !favState
            showToast(favState ? 'Added to Favorites' : 'Removed from Favorites', ToastType.Success)
        } catch {
            showToast('Could not update favorites.', ToastType.Error)
        } finally {
            favBtn.disabled = false
            updateFavButton()
        }
    })

    const offerBtn = document.createElement('button')
    offerBtn.id = 'offer-btn'
    offerBtn.className = 'button'
    offerBtn.textContent = 'Make an Offer'
    offerBtn.addEventListener('click', () => {
        const currentUserId = getUserId();
        if (!currentUserId) {
            showToast('You must be logged in to make an offer.', ToastType.Error);
            window.location.href = 'login.html';
            return;
        }
        showOfferModal({ car, userId: currentUserId })
    })

    actionsContainer.appendChild(favBtn)
    actionsContainer.appendChild(offerBtn)
}