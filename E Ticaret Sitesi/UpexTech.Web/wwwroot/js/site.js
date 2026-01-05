// UpexTech Custom JavaScript

// Quantity Controls for Product Cards
function decreaseQty(btn) {
    const productId = btn.dataset.productId;
    const input = document.querySelector(`.product-qty[data-product-id="${productId}"]`);
    let value = parseInt(input.value) - 1;
    if (value < 1) value = 1;
    input.value = value;
}

function increaseQty(btn) {
    const productId = btn.dataset.productId;
    const maxStock = parseInt(btn.dataset.maxStock);
    const input = document.querySelector(`.product-qty[data-product-id="${productId}"]`);
    let value = parseInt(input.value) + 1;
    if (value > maxStock) {
        value = maxStock;
        showToast(`Maksimum ${maxStock} adet ekleyebilirsiniz.`, 'warning');
    }
    input.value = value;
}

function validateQty(input) {
    const maxStock = parseInt(input.dataset.maxStock);
    let value = parseInt(input.value);
    if (isNaN(value) || value < 1) value = 1;
    if (value > maxStock) {
        value = maxStock;
        showToast(`Maksimum ${maxStock} adet ekleyebilirsiniz.`, 'warning');
    }
    input.value = value;
}

function addToCartWithQty(productId, btn) {
    const input = document.querySelector(`.product-qty[data-product-id="${productId}"]`);
    const quantity = parseInt(input.value) || 1;

    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Ekleniyor...';

    fetch('/Cart/Add', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `productId=${productId}&quantity=${quantity}`
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Update cart badge
                const badge = document.querySelector('.cart-badge');
                if (badge) {
                    badge.textContent = data.cartCount;
                    badge.style.display = data.cartCount > 0 ? 'inline' : 'none';
                }
                showToast(`${quantity} adet ürün sepete eklendi!`);
                input.value = 1; // Reset quantity
            } else {
                showToast(data.message || 'Bir hata oluştu', 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToast('Bir hata oluştu', 'error');
        })
        .finally(() => {
            btn.disabled = false;
            btn.innerHTML = '<i class="bi bi-cart-plus me-1"></i>Sepete Ekle';
        });
}

// Add to Cart (simple version)
function addToCart(productId) {
    fetch('/Cart/Add', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `productId=${productId}&quantity=1`
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                const badge = document.getElementById('cartBadge');
                if (badge) {
                    badge.textContent = data.cartCount;
                    badge.style.display = data.cartCount > 0 ? 'flex' : 'none';
                }
                showToast('Ürün sepete eklendi!');
            } else {
                showToast(data.message || 'Bir hata oluştu', 'error');
            }
        });
}

// Toggle Favorite
function toggleFavorite(btn, productId) {
    fetch('/Favorites/Toggle', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: 'productId=' + productId
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                if (data.isFavorite) {
                    btn.classList.add('active');
                    showToast('Favorilere eklendi!');
                } else {
                    btn.classList.remove('active');
                    showToast('Favorilerden çıkarıldı');
                }
                // Reinitialize icons
                if (typeof lucide !== 'undefined') {
                    lucide.createIcons();
                }
            }
        })
        .catch(err => {
            console.log('Favorite error:', err);
        });
}

// Toggle Compare
function toggleCompare(btn, productId) {
    btn.classList.toggle('active');
    if (btn.classList.contains('active')) {
        showToast('Karşılaştırma listesine eklendi');
    } else {
        showToast('Karşılaştırma listesinden çıkarıldı');
    }
    // Reinitialize icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
}

// Toast Notification
function showToast(message, type = 'success') {
    // Remove existing toasts
    document.querySelectorAll('.custom-toast').forEach(t => t.remove());

    const toast = document.createElement('div');
    toast.className = `custom-toast alert alert-${type === 'error' ? 'danger' : type === 'warning' ? 'warning' : 'success'} position-fixed shadow`;
    toast.style.cssText = 'bottom: 20px; right: 20px; z-index: 9999; min-width: 250px; animation: slideIn 0.3s ease;';

    const icon = type === 'error' ? 'x-circle' : type === 'warning' ? 'exclamation-circle' : 'check-circle';
    toast.innerHTML = `<i class="bi bi-${icon} me-2"></i>${message}`;
    document.body.appendChild(toast);

    setTimeout(() => {
        toast.style.animation = 'slideOut 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 2500);
}

// Show Login Alert
function showLoginAlert() {
    showToast('Bu işlem için giriş yapmalısınız.', 'warning');
    setTimeout(() => {
        window.location.href = '/Account/Login';
    }, 1000);
}

// Document Ready
document.addEventListener('DOMContentLoaded', function () {
    console.log('UpexTech loaded');

    // Add CSS animations
    const style = document.createElement('style');
    style.textContent = `
        @keyframes slideIn { from { opacity: 0; transform: translateX(100px); } to { opacity: 1; transform: translateX(0); } }
        @keyframes slideOut { from { opacity: 1; transform: translateX(0); } to { opacity: 0; transform: translateX(100px); } }
    `;
    document.head.appendChild(style);

    // Initialize compare bar on page load
    updateCompareBar();

    // Update compare button states on page load
    updateCompareButtonStates();

    // Campaign Popup
    initCampaignPopup();
});

// ===== Campaign Popup =====
function initCampaignPopup() {
    const popup = document.getElementById('campaignPopup');
    if (!popup) return;

    // Check if popup was already shown in this session
    const popupShown = sessionStorage.getItem('campaignPopupShown');
    if (popupShown) {
        popup.classList.add('hidden');
        return;
    }

    // Show popup and mark as shown for this session
    popup.classList.remove('hidden');
    sessionStorage.setItem('campaignPopupShown', 'true');

    // Initialize icons in popup
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }

    // Start countdown timer
    startPopupTimer();
}

function closeCampaignPopup() {
    const popup = document.getElementById('campaignPopup');
    if (popup) {
        popup.style.animation = 'fadeOut 0.3s ease';
        setTimeout(() => {
            popup.classList.add('hidden');
            popup.style.animation = '';
        }, 300);

        // Save close time
        localStorage.setItem('campaignPopupClosed', Date.now().toString());
    }
}

function startPopupTimer() {
    // Set campaign end date (2 days from now for demo)
    const endDate = new Date();
    endDate.setDate(endDate.getDate() + 2);
    endDate.setHours(endDate.getHours() + 14);
    endDate.setMinutes(endDate.getMinutes() + 32);

    function updateTimer() {
        const now = new Date();
        const diff = endDate - now;

        if (diff <= 0) {
            document.getElementById('popup-days').textContent = '00';
            document.getElementById('popup-hours').textContent = '00';
            document.getElementById('popup-minutes').textContent = '00';
            document.getElementById('popup-seconds').textContent = '00';
            return;
        }

        const days = Math.floor(diff / (1000 * 60 * 60 * 24));
        const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((diff % (1000 * 60)) / 1000);

        const daysEl = document.getElementById('popup-days');
        const hoursEl = document.getElementById('popup-hours');
        const minutesEl = document.getElementById('popup-minutes');
        const secondsEl = document.getElementById('popup-seconds');

        if (daysEl) daysEl.textContent = days.toString().padStart(2, '0');
        if (hoursEl) hoursEl.textContent = hours.toString().padStart(2, '0');
        if (minutesEl) minutesEl.textContent = minutes.toString().padStart(2, '0');
        if (secondsEl) secondsEl.textContent = seconds.toString().padStart(2, '0');
    }

    updateTimer();
    setInterval(updateTimer, 1000);
}

// ===== Coming Soon Feature =====
function showComingSoon(featureName) {
    // Create overlay
    const overlay = document.createElement('div');
    overlay.className = 'coming-soon-overlay';
    overlay.onclick = closeComingSoon;

    // Create toast
    const toast = document.createElement('div');
    toast.className = 'coming-soon-toast';
    toast.innerHTML = `
        <div class="coming-soon-icon">
            <i data-lucide="clock" style="width: 30px; height: 30px;"></i>
        </div>
        <div class="coming-soon-title">${featureName}</div>
        <div class="coming-soon-text">Bu özellik çok yakında aktif olacak!</div>
        <button class="btn btn-primary btn-sm" onclick="closeComingSoon()">Tamam</button>
    `;

    document.body.appendChild(overlay);
    document.body.appendChild(toast);

    // Initialize lucide icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
}

function closeComingSoon() {
    document.querySelectorAll('.coming-soon-overlay, .coming-soon-toast').forEach(el => el.remove());
}

// ===== Product Compare Feature =====
const MAX_COMPARE_ITEMS = 4;

function getCompareList() {
    const list = localStorage.getItem('compareList');
    return list ? JSON.parse(list) : [];
}

function saveCompareList(list) {
    localStorage.setItem('compareList', JSON.stringify(list));
    updateCompareBar();
    updateCompareButtonStates();
}

function toggleCompare(btn, productId) {
    const compareList = getCompareList();
    const existingIndex = compareList.findIndex(item => item.id === productId);

    if (existingIndex > -1) {
        // Remove from list
        compareList.splice(existingIndex, 1);
        btn.classList.remove('active');
        showToast('Ürün karşılaştırma listesinden çıkarıldı');
    } else {
        // Add to list
        if (compareList.length >= MAX_COMPARE_ITEMS) {
            showToast(`En fazla ${MAX_COMPARE_ITEMS} ürün karşılaştırabilirsiniz`, 'warning');
            return;
        }

        // Get product info from card
        const card = btn.closest('.product-card');
        const productData = {
            id: productId,
            name: card?.querySelector('.product-card-title')?.textContent?.trim() || 'Ürün',
            image: card?.querySelector('.product-card-image img')?.src || '/images/phone1.png',
            price: card?.querySelector('.product-current-price')?.textContent?.trim() || 'Fiyat bilgisi yok',
            rating: card?.querySelector('.rating-value')?.textContent?.trim() || '0.0',
            // Extended product info for comparison
            brand: card?.dataset?.brand || 'Belirtilmemiş',
            category: card?.dataset?.category || 'Belirtilmemiş',
            stock: card?.dataset?.stock || 'Belirtilmemiş',
            sku: card?.dataset?.sku || '-',
            // Technical specs (will be fetched from server for details)
            specs: {
                weight: card?.dataset?.weight || '-',
                dimensions: card?.dataset?.dimensions || '-',
                power: card?.dataset?.power || '-',
                warranty: card?.dataset?.warranty || '24 Ay',
                batteryCapacity: card?.dataset?.battery || '-',
                screenSize: card?.dataset?.screen || '-',
                resolution: card?.dataset?.resolution || '-',
                material: card?.dataset?.material || '-'
            }
        };

        compareList.push(productData);
        btn.classList.add('active');
        showToast('Ürün karşılaştırma listesine eklendi');
    }

    saveCompareList(compareList);

    // Reinitialize icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
}

function updateCompareBar() {
    const compareList = getCompareList();
    const bar = document.getElementById('compareBar');
    const countEl = document.getElementById('compareCount');

    if (bar && countEl) {
        countEl.textContent = compareList.length;
        bar.style.display = compareList.length > 0 ? 'block' : 'none';

        // Reinitialize icons
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    }
}

function updateCompareButtonStates() {
    const compareList = getCompareList();
    const productIds = compareList.map(item => item.id);

    document.querySelectorAll('.product-action-btn.compare').forEach(btn => {
        const productId = parseInt(btn.getAttribute('onclick')?.match(/\d+/)?.[0]);
        if (productId && productIds.includes(productId)) {
            btn.classList.add('active');
        } else {
            btn.classList.remove('active');
        }
    });
}

function clearCompare() {
    localStorage.removeItem('compareList');
    updateCompareBar();
    updateCompareButtonStates();

    // Close modal if open
    const modal = document.getElementById('compareModal');
    if (modal) {
        const bsModal = bootstrap.Modal.getInstance(modal);
        if (bsModal) bsModal.hide();
    }

    showToast('Karşılaştırma listesi temizlendi');
}

function openCompareModal() {
    const compareList = getCompareList();
    const content = document.getElementById('compareContent');

    if (!content) return;

    if (compareList.length < 2) {
        content.innerHTML = `
            <div class="compare-empty-slot" style="margin: 2rem; text-align: center;">
                <i data-lucide="info" style="width: 48px; height: 48px; margin-bottom: 1rem; color: var(--gray-400);"></i>
                <h5>Karşılaştırma için en az 2 ürün ekleyin</h5>
                <p class="text-muted">Ürün kartlarındaki karşılaştır butonunu kullanarak ürün ekleyebilirsiniz.</p>
            </div>
        `;
    } else {
        // Helper function to check if values are different
        function areValuesDifferent(values) {
            const nonEmptyValues = values.filter(v => v && v !== '-' && v !== 'Belirtilmemiş');
            if (nonEmptyValues.length < 2) return false;
            return new Set(nonEmptyValues).size > 1;
        }

        // Helper function to get highlight class
        function getHighlightClass(values, currentValue) {
            if (!areValuesDifferent(values)) return '';
            return 'compare-highlight';
        }

        // Build comparison rows
        const comparisonSpecs = [
            { key: 'price', label: 'Fiyat', icon: 'tag', getValue: p => p.price },
            { key: 'rating', label: 'Puan', icon: 'star', getValue: p => p.rating, isRating: true },
            { key: 'brand', label: 'Marka', icon: 'briefcase', getValue: p => p.brand || 'Belirtilmemiş' },
            { key: 'category', label: 'Kategori', icon: 'folder', getValue: p => p.category || 'Belirtilmemiş' },
            { key: 'stock', label: 'Stok Durumu', icon: 'package', getValue: p => p.stock || 'Belirtilmemiş' },
            { key: 'warranty', label: 'Garanti', icon: 'shield', getValue: p => p.specs?.warranty || '24 Ay' },
            { key: 'weight', label: 'Ağırlık', icon: 'scale', getValue: p => p.specs?.weight || '-' },
            { key: 'dimensions', label: 'Boyutlar', icon: 'maximize', getValue: p => p.specs?.dimensions || '-' },
            { key: 'power', label: 'Güç', icon: 'zap', getValue: p => p.specs?.power || '-' },
            { key: 'battery', label: 'Batarya Kapasitesi', icon: 'battery-charging', getValue: p => p.specs?.batteryCapacity || '-' },
            { key: 'screen', label: 'Ekran Boyutu', icon: 'monitor', getValue: p => p.specs?.screenSize || '-' },
            { key: 'resolution', label: 'Çözünürlük', icon: 'grid', getValue: p => p.specs?.resolution || '-' },
            { key: 'material', label: 'Malzeme', icon: 'box', getValue: p => p.specs?.material || '-' }
        ];

        let rowsHTML = comparisonSpecs.map(spec => {
            const values = compareList.map(p => spec.getValue(p));
            const hasDifference = areValuesDifferent(values);

            return `
                <tr class="${hasDifference ? 'compare-row-different' : ''}">
                    <th>
                        <i data-lucide="${spec.icon}" style="width: 16px; height: 16px; margin-right: 8px;"></i>
                        ${spec.label}
                        ${hasDifference ? '<span class="compare-diff-badge">Farklı</span>' : ''}
                    </th>
                    ${compareList.map((product, idx) => {
                const value = spec.getValue(product);
                const highlightClass = hasDifference ? 'compare-cell-highlight' : '';

                if (spec.isRating) {
                    return `
                                <td class="${highlightClass}">
                                    <div class="d-flex align-items-center justify-content-center gap-1">
                                        <i data-lucide="star" style="width: 16px; height: 16px; color: #facc15;"></i>
                                        <span>${value}</span>
                                    </div>
                                </td>
                            `;
                }
                return `<td class="${highlightClass}">${value}</td>`;
            }).join('')}
                </tr>
            `;
        }).join('');

        let tableHTML = `
            <div class="compare-summary mb-3">
                <span class="badge bg-info me-2">${compareList.length} Ürün</span>
                <span class="text-muted small">
                    <i data-lucide="info" style="width: 14px; height: 14px;"></i>
                    Farklı değerler sarı ile vurgulanmıştır
                </span>
            </div>
            <div class="table-responsive">
                <table class="compare-table table table-bordered">
                    <thead class="table-light">
                        <tr>
                            <th style="width: 150px;">Özellik</th>
                            ${compareList.map(product => `
                                <td class="compare-cell text-center">
                                    <button class="compare-remove-btn" onclick="removeFromCompare(${product.id})" title="Kaldır">
                                        <i data-lucide="x" style="width: 12px; height: 12px;"></i>
                                    </button>
                                    <img src="${product.image}" alt="${product.name}" class="compare-product-image">
                                    <div class="compare-product-name">${product.name}</div>
                                </td>
                            `).join('')}
                        </tr>
                    </thead>
                    <tbody>
                        ${rowsHTML}
                        <tr>
                            <th>
                                <i data-lucide="external-link" style="width: 16px; height: 16px; margin-right: 8px;"></i>
                                İşlem
                            </th>
                            ${compareList.map(product => `
                                <td class="text-center">
                                    <a href="/Catalog/Details/${product.id}" class="btn btn-primary btn-sm">
                                        <i data-lucide="eye" style="width: 14px; height: 14px;"></i> Detayları Gör
                                    </a>
                                </td>
                            `).join('')}
                        </tr>
                    </tbody>
                </table>
            </div>
        `;
        content.innerHTML = tableHTML;
    }

    // Reinitialize icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }

    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('compareModal'));
    modal.show();
}

function removeFromCompare(productId) {
    let compareList = getCompareList();
    compareList = compareList.filter(item => item.id !== productId);
    saveCompareList(compareList);

    // Re-render modal content
    openCompareModal();

    if (compareList.length < 2) {
        showToast('Karşılaştırma için en az 2 ürün gerekli', 'warning');
    }
}

// Toggle Compare from Product Detail Page
function toggleCompareFromDetail(productId, name, image, price, rating, brand, category, stock) {
    const compareList = getCompareList();
    const existingIndex = compareList.findIndex(item => item.id === productId);
    const btn = document.getElementById('pdpCompareBtn');

    if (existingIndex > -1) {
        // Remove from list
        compareList.splice(existingIndex, 1);
        if (btn) {
            btn.classList.remove('active');
            btn.innerHTML = '<i class="bi bi-arrow-left-right me-1"></i>Karşılaştır';
        }
        showToast('Ürün karşılaştırma listesinden çıkarıldı');
    } else {
        // Add to list
        if (compareList.length >= MAX_COMPARE_ITEMS) {
            showToast(`En fazla ${MAX_COMPARE_ITEMS} ürün karşılaştırabilirsiniz`, 'warning');
            return;
        }

        const productData = {
            id: productId,
            name: name,
            image: image,
            price: price,
            rating: rating,
            brand: brand,
            category: category,
            stock: stock,
            sku: '-',
            specs: {
                weight: '-',
                dimensions: '-',
                power: '-',
                warranty: '24 Ay',
                batteryCapacity: '-',
                screenSize: '-',
                resolution: '-',
                material: '-'
            }
        };

        compareList.push(productData);
        if (btn) {
            btn.classList.add('active');
            btn.innerHTML = '<i class="bi bi-check-circle me-1"></i>Listede';
        }
        showToast('Ürün karşılaştırma listesine eklendi');
    }

    saveCompareList(compareList);
}

// Check and update PDP compare button state on page load
document.addEventListener('DOMContentLoaded', function () {
    const pdpCompareBtn = document.getElementById('pdpCompareBtn');
    if (pdpCompareBtn) {
        const productIdMatch = pdpCompareBtn.getAttribute('onclick')?.match(/toggleCompareFromDetail\((\d+)/);
        if (productIdMatch) {
            const productId = parseInt(productIdMatch[1]);
            const compareList = getCompareList();
            if (compareList.some(item => item.id === productId)) {
                pdpCompareBtn.classList.add('active');
                pdpCompareBtn.innerHTML = '<i class="bi bi-check-circle me-1"></i>Listede';
            }
        }
    }
});
