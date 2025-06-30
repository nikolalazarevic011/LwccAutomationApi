/**
 * Dashboard Core JavaScript
 * Handles main dashboard functionality, API calls, and system interactions
 */

// Dashboard initialization
document.addEventListener('DOMContentLoaded', function() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Auto-refresh system status every 30 seconds
    setInterval(refreshSystemStatus, 30000);
    
    // Set max date for completion date to today
    const today = new Date().toISOString().split('T')[0];
    const completionDateField = document.getElementById('certificateCompletionDate');
    if (completionDateField) {
        completionDateField.setAttribute('max', today);
    }
});

// Enhanced endpoint execution handler
function handleEndpointExecution(endpointId, url, method, isExternal) {
    // Handle certificate-specific endpoints
    if (endpointId === 'generate-single-certificate') {
        showSingleCertificateModal();
        return;
    }
    
    if (endpointId === 'upload-csv-certificates') {
        showCsvUploadModal();
        return;
    }
    
    // Handle validation endpoint directly
    if (endpointId === 'validate-certificate-config') {
        executeApiCall(endpointId, url, method, isExternal);
        return;
    }
    
    // Default handling for other endpoints
    executeApiCall(endpointId, url, method, isExternal);
}

// Execute API calls
function executeApiCall(endpointId, url, method, isExternal) {
    const modal = new bootstrap.Modal(document.getElementById('apiResponseModal'));
    modal.show();

    if (isExternal) {
        document.getElementById('apiResponseContent').innerHTML = `
            <div class="alert alert-info">
                <i class="bi bi-info-circle"></i>
                <strong>External API Call</strong><br>
                Opening: <code>${url}</code><br>
                Method: <code>${method}</code>
            </div>`;
        window.open(url, '_blank');
        return;
    }

    const options = {
        method: method,
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    };

    fetch(url, options)
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }
            return response.json();
        })
        .then(data => {
            document.getElementById('apiResponseContent').innerHTML = `
                <div class="alert alert-success">
                    <i class="bi bi-check-circle"></i> <strong>Success</strong>
                </div>
                <h6>Response:</h6>
                <pre class="bg-light p-3 rounded"><code>${JSON.stringify(data, null, 2)}</code></pre>`;
        })
        .catch(error => {
            document.getElementById('apiResponseContent').innerHTML = `
                <div class="alert alert-danger">
                    <i class="bi bi-exclamation-triangle"></i> <strong>Error</strong><br>
                    ${error.message}
                </div>`;
        });
}

// Show endpoint details
function showEndpointDetails(endpointId) {
    // This would show detailed information about the endpoint
    alert(`Details for endpoint: ${endpointId}`);
}

// Refresh dashboard
function refreshDashboard() {
    const refreshBtn = document.querySelector('[onclick="refreshDashboard()"] i');
    if (refreshBtn) {
        refreshBtn.classList.add('fa-spin');
        
        setTimeout(() => {
            refreshBtn.classList.remove('fa-spin');
            location.reload();
        }, 1000);
    }
}

// Toggle fullscreen
function toggleFullscreen() {
    if (!document.fullscreenElement) {
        document.documentElement.requestFullscreen();
    } else {
        document.exitFullscreen();
    }
}

// Refresh system status
function refreshSystemStatus() {
    console.log('Refreshing system status...');
    // TODO: Implement actual system status refresh logic
}
