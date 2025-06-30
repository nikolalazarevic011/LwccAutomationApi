/**
 * Certificate Management JavaScript
 * Handles certificate generation, modal interactions, and CSV processing
 */

// Show single certificate modal
function showSingleCertificateModal() {
    const modal = new bootstrap.Modal(document.getElementById('singleCertificateModal'));
    
    // Reset form
    document.getElementById('singleCertificateForm').reset();
    document.getElementById('singleCertificateForm').classList.remove('was-validated');
    
    modal.show();
}

// Show CSV upload modal
function showCsvUploadModal() {
    const modal = new bootstrap.Modal(document.getElementById('csvUploadModal'));
    
    // Reset form and hide preview
    document.getElementById('csvUploadForm').reset();
    document.getElementById('csvUploadForm').classList.remove('was-validated');
    document.getElementById('csvPreview').classList.add('d-none');
    document.getElementById('uploadProgress').classList.add('d-none');
    
    modal.show();
}

// Submit single certificate generation
function submitSingleCertificate() {
    const form = document.getElementById('singleCertificateForm');
    
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    // Validate completion date is not in future
    const completionDate = new Date(document.getElementById('certificateCompletionDate').value);
    const today = new Date();
    today.setHours(23, 59, 59, 999); // End of today
    
    if (completionDate > today) {
        document.getElementById('certificateCompletionDate').setCustomValidity('Completion date cannot be in the future');
        form.classList.add('was-validated');
        return;
    } else {
        document.getElementById('certificateCompletionDate').setCustomValidity('');
    }

    const certificateData = {
        email: document.getElementById('certificateEmail').value.trim(),
        firstName: document.getElementById('certificateFirstName').value.trim(),
        lastName: document.getElementById('certificateLastName').value.trim(),
        completionDate: document.getElementById('certificateCompletionDate').value
    };

    // Close the certificate modal
    bootstrap.Modal.getInstance(document.getElementById('singleCertificateModal')).hide();
    
    // Show API response modal
    const responseModal = new bootstrap.Modal(document.getElementById('apiResponseModal'));
    responseModal.show();
    
    // Reset response content
    document.getElementById('apiResponseContent').innerHTML = `
        <div class="text-center p-4">
            <div class="spinner-border text-success" role="status">
                <span class="visually-hidden">Generating certificate...</span>
            </div>
            <div class="mt-2">Generating and emailing certificate...</div>
        </div>`;

    // Make API call
    fetch('/api/foundationscertificate/generate-and-email', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify(certificateData)
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        return response.json();
    })
    .then(data => {
        document.getElementById('apiResponseContent').innerHTML = `
            <div class="alert alert-success">
                <i class="bi bi-check-circle"></i> <strong>Success!</strong><br>
                Certificate generated and emailed successfully.
            </div>
            <h6>Response Details:</h6>
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

// Validate CSV file selection
function validateCsvFile(input) {
    const file = input.files[0];
    
    if (!file) return;
    
    // Check file type
    if (!file.name.toLowerCase().endsWith('.csv')) {
        input.setCustomValidity('Please select a CSV file');
        input.classList.add('is-invalid');
        return;
    }
    
    // Check file size (10MB limit)
    if (file.size > 10 * 1024 * 1024) {
        input.setCustomValidity('File size must be less than 10MB');
        input.classList.add('is-invalid');
        return;
    }
    
    input.setCustomValidity('');
    input.classList.remove('is-invalid');
    input.classList.add('is-valid');
    
    // Preview CSV
    previewCsvFile(file);
}

// Preview CSV file content
function previewCsvFile(file) {
    const reader = new FileReader();
    reader.onload = function(e) {
        const csv = e.target.result;
        const lines = csv.split('\n').slice(0, 6); // Header + 5 rows
        
        if (lines.length > 1) {
            const headers = lines[0].split(',').map(h => h.trim().replace(/"/g, ''));
            const previewTable = document.getElementById('csvPreviewTable');
            
            // Create header
            const thead = previewTable.querySelector('thead');
            thead.innerHTML = '<tr>' + headers.map(h => `<th>${h}</th>`).join('') + '</tr>';
            
            // Create preview rows
            const tbody = previewTable.querySelector('tbody');
            const rows = lines.slice(1, 6).filter(line => line.trim()).map(line => {
                const cells = line.split(',').map(c => c.trim().replace(/"/g, ''));
                return '<tr>' + cells.map(c => `<td>${c}</td>`).join('') + '</tr>';
            }).join('');
            
            tbody.innerHTML = rows;
            document.getElementById('csvPreview').classList.remove('d-none');
        }
    };
    reader.readAsText(file);
}

// Submit CSV upload
function submitCsvUpload() {
    const form = document.getElementById('csvUploadForm');
    const fileInput = document.getElementById('csvFile');
    
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }
    
    const file = fileInput.files[0];
    if (!file) {
        fileInput.setCustomValidity('Please select a file');
        form.classList.add('was-validated');
        return;
    }

    // Show progress
    document.getElementById('uploadProgress').classList.remove('d-none');
    document.getElementById('uploadButton').disabled = true;
    
    // Create FormData
    const formData = new FormData();
    formData.append('csvFile', file);

    // Make API call
    fetch('/api/foundationscertificate/upload', {
        method: 'POST',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: formData
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }
        return response.json();
    })
    .then(data => {
        // Close upload modal
        bootstrap.Modal.getInstance(document.getElementById('csvUploadModal')).hide();
        
        // Show results in API response modal
        const responseModal = new bootstrap.Modal(document.getElementById('apiResponseModal'));
        document.getElementById('apiResponseContent').innerHTML = `
            <div class="alert alert-success">
                <i class="bi bi-check-circle"></i> <strong>Processing Complete!</strong>
            </div>
            <div class="row">
                <div class="col-md-4">
                    <div class="text-center">
                        <h4 class="text-primary">${data.totalRecords}</h4>
                        <small>Total Records</small>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="text-center">
                        <h4 class="text-success">${data.successfullyProcessed}</h4>
                        <small>Successful</small>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="text-center">
                        <h4 class="text-danger">${data.failed}</h4>
                        <small>Failed</small>
                    </div>
                </div>
            </div>
            ${data.errors && data.errors.length > 0 ? `
                <hr>
                <h6>Errors:</h6>
                <div class="alert alert-warning">
                    ${data.errors.slice(0, 5).map(error => 
                        `<div><strong>Row ${error.rowNumber}:</strong> ${error.errorMessage}</div>`
                    ).join('')}
                    ${data.errors.length > 5 ? `<div><em>... and ${data.errors.length - 5} more errors</em></div>` : ''}
                </div>
            ` : ''}
            <h6>Full Response:</h6>
            <pre class="bg-light p-3 rounded" style="max-height: 300px; overflow-y: auto;"><code>${JSON.stringify(data, null, 2)}</code></pre>`;
        responseModal.show();
    })
    .catch(error => {
        document.getElementById('apiResponseContent').innerHTML = `
            <div class="alert alert-danger">
                <i class="bi bi-exclamation-triangle"></i> <strong>Upload Failed</strong><br>
                ${error.message}
            </div>`;
        
        // Show error in API response modal
        const responseModal = new bootstrap.Modal(document.getElementById('apiResponseModal'));
        responseModal.show();
    })
    .finally(() => {
        // Reset upload state
        document.getElementById('uploadProgress').classList.add('d-none');
        document.getElementById('uploadButton').disabled = false;
        bootstrap.Modal.getInstance(document.getElementById('csvUploadModal')).hide();
    });
}

// Download CSV template
function downloadCsvTemplate() {
    const csvContent = "Email,FirstName,LastName,CompletionDate\n" +
                      "john.doe@example.com,John,Doe,2024-01-15\n" +
                      "jane.smith@example.com,Jane,Smith,2024-01-20";
    
    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'certificate_template.csv';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
}
