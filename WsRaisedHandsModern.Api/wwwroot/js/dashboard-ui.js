/**
 * Dashboard UI JavaScript
 * Handles UI interactions, animations, visual effects, and quick actions
 */

let quickActionsPanelCollapsed = false;

// Initialize UI interactions when DOM is loaded
document.addEventListener('DOMContentLoaded', function() {
    initializeEndpointCardEffects();
});

// Quick Actions Panel
function toggleQuickActions() {
    const panel = document.getElementById('quickActionsPanel');
    const body = document.getElementById('quickActionsBody');
    
    if (quickActionsPanelCollapsed) {
        body.style.display = 'block';
        quickActionsPanelCollapsed = false;
    } else {
        body.style.display = 'none';
        quickActionsPanelCollapsed = true;
    }
}

// Execute quick actions
function executeQuickAction(action) {
    console.log(`Executing quick action: ${action}`);
    
    const toast = document.createElement('div');
    toast.className = 'toast align-items-center text-white bg-success border-0 position-fixed bottom-0 start-0 m-3';
    toast.setAttribute('role', 'alert');
    toast.style.zIndex = '1060';
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                <i class="bi bi-check-circle me-2"></i>Quick action '${action}' executed successfully!
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>`;
    
    document.body.appendChild(toast);
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();
    
    toast.addEventListener('hidden.bs.toast', () => {
        document.body.removeChild(toast);
    });
}

// Initialize hover effects for endpoint cards
function initializeEndpointCardEffects() {
    document.querySelectorAll('.endpoint-card').forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-2px)';
            this.style.transition = 'transform 0.2s ease-in-out';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });
}

// Show toast notification
function showToast(message, type = 'success') {
    const toastColors = {
        success: 'bg-success',
        error: 'bg-danger',
        warning: 'bg-warning',
        info: 'bg-info'
    };

    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white ${toastColors[type]} border-0 position-fixed bottom-0 end-0 m-3`;
    toast.setAttribute('role', 'alert');
    toast.style.zIndex = '1060';
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>`;
    
    document.body.appendChild(toast);
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();
    
    toast.addEventListener('hidden.bs.toast', () => {
        document.body.removeChild(toast);
    });
}

// Animate button on click
function animateButton(button) {
    if (!button) return;
    
    button.style.transform = 'scale(0.95)';
    setTimeout(() => {
        button.style.transform = 'scale(1)';
    }, 150);
}

// Loading spinner utility
function showLoadingSpinner(elementId, message = 'Loading...') {
    const element = document.getElementById(elementId);
    if (!element) return;
    
    element.innerHTML = `
        <div class="text-center p-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">${message}</span>
            </div>
            <div class="mt-2">${message}</div>
        </div>`;
}

// Hide loading spinner
function hideLoadingSpinner(elementId) {
    const element = document.getElementById(elementId);
    if (!element) return;
    
    element.innerHTML = '';
}

// Smooth scroll to element
function smoothScrollTo(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ 
            behavior: 'smooth',
            block: 'start'
        });
    }
}

// Auto-hide alerts after specified time
function autoHideAlert(alertElement, delay = 5000) {
    if (!alertElement) return;
    
    setTimeout(() => {
        if (alertElement.parentNode) {
            alertElement.style.opacity = '0';
            alertElement.style.transition = 'opacity 0.5s ease-out';
            
            setTimeout(() => {
                if (alertElement.parentNode) {
                    alertElement.parentNode.removeChild(alertElement);
                }
            }, 500);
        }
    }, delay);
}

// Pulse effect for important elements
function pulseElement(elementId, duration = 1000) {
    const element = document.getElementById(elementId);
    if (!element) return;
    
    element.style.animation = `pulse ${duration}ms ease-in-out`;
    
    setTimeout(() => {
        element.style.animation = '';
    }, duration);
}

// Add CSS animation styles if not already present
function addAnimationStyles() {
    if (document.getElementById('dashboard-ui-styles')) return;
    
    const styles = document.createElement('style');
    styles.id = 'dashboard-ui-styles';
    styles.textContent = `
        @keyframes pulse {
            0% { transform: scale(1); }
            50% { transform: scale(1.05); }
            100% { transform: scale(1); }
        }
        
        .fade-in {
            animation: fadeIn 0.5s ease-in;
        }
        
        @keyframes fadeIn {
            from { opacity: 0; }
            to { opacity: 1; }
        }
        
        .slide-in {
            animation: slideIn 0.3s ease-out;
        }
        
        @keyframes slideIn {
            from { transform: translateY(-20px); opacity: 0; }
            to { transform: translateY(0); opacity: 1; }
        }
    `;
    
    document.head.appendChild(styles);
}

// Initialize animation styles when DOM is loaded
document.addEventListener('DOMContentLoaded', addAnimationStyles);
