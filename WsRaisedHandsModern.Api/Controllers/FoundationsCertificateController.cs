using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WsRaisedHandsModern.Api.DTOs;
using WsRaisedHandsModern.Api.Interfaces;
using WsRaisedHandsModern.Api.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace WsRaisedHandsModern.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FoundationsCertificateController : ControllerBase
    {
        private readonly ICertificateService _certificateService;
        private readonly ICsvProcessingService _csvProcessingService;
        private readonly ILogger<FoundationsCertificateController> _logger;
        private readonly CertificateSettings _certificateSettings;

        public FoundationsCertificateController(
            ICertificateService certificateService,
            ICsvProcessingService csvProcessingService,
            ILogger<FoundationsCertificateController> logger,
            IOptions<CertificateSettings> certificateSettings)
        {
            _certificateService = certificateService;
            _csvProcessingService = csvProcessingService;
            _logger = logger;
            _certificateSettings = certificateSettings.Value;
        }

        /// <summary>
        /// Upload CSV file and process foundation certificates
        /// </summary>
        /// <param name="csvFile">CSV file with Email, FirstName, LastName, CompletionDate columns</param>
        /// <returns>Processing result with success/failure statistics</returns>
        [HttpPost("upload")]
        public async Task<ActionResult<CertificateProcessingResult>> UploadAndProcessCertificates(IFormFile csvFile)
        {
            try
            {
                // Validate file
                var validationResult = ValidateUploadedFile(csvFile);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.ErrorMessage);
                }

                _logger.LogInformation("Processing uploaded CSV file: {FileName} ({FileSize} bytes)", 
                    csvFile.FileName, csvFile.Length);

                // Parse CSV
                List<FoundationsCertificateDTO> certificates;
                using (var stream = csvFile.OpenReadStream())
                {
                    // Validate headers first
                    var headersValid = await _csvProcessingService.ValidateCsvHeadersAsync(stream);
                    if (!headersValid)
                    {
                        return BadRequest("CSV file does not have the required headers. Expected: Email, FirstName, LastName, CompletionDate");
                    }

                    certificates = await _csvProcessingService.ParseFoundationsCsvAsync(stream);
                }

                if (!certificates.Any())
                {
                    return BadRequest("No valid certificate data found in the uploaded file");
                }

                // Process certificates
                var result = await _certificateService.ProcessFoundationsCertificatesAsync(certificates);

                _logger.LogInformation("CSV processing completed. Total: {Total}, Success: {Success}, Failed: {Failed}",
                    result.TotalRecords, result.SuccessfullyProcessed, result.Failed);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing CSV upload");
                return StatusCode(500, "Internal server error while processing the CSV file");
            }
        }

        /// <summary>
        /// Generate a single certificate for testing purposes
        /// </summary>
        /// <param name="certificateData">Certificate data</param>
        /// <returns>PDF certificate file</returns>
        [HttpPost("generate-single")]
        public async Task<IActionResult> GenerateSingleCertificate([FromBody] FoundationsCertificateDTO certificateData)
        {
            try
            {
                _logger.LogInformation("Generating single certificate for {FullName}", certificateData.FullName);

                var pdfBytes = await _certificateService.GenerateCertificateAsync(certificateData);
                var fileName = $"Certificate_{certificateData.FirstName}_{certificateData.LastName}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating single certificate for {FullName}", certificateData.FullName);
                return StatusCode(500, "Internal server error while generating the certificate");
            }
        }

        /// <summary>
        /// Generate and email a single certificate
        /// </summary>
        /// <param name="certificateData">Certificate data</param>
        /// <returns>Success status</returns>
        [HttpPost("generate-and-email")]
        public async Task<IActionResult> GenerateAndEmailSingleCertificate([FromBody] FoundationsCertificateDTO certificateData)
        {
            try
            {
                _logger.LogInformation("Generating and emailing certificate for {FullName} ({Email})", 
                    certificateData.FullName, certificateData.Email);

                var success = await _certificateService.GenerateAndEmailCertificateAsync(certificateData);

                if (success)
                {
                    return Ok(new { message = $"Certificate successfully generated and emailed to {certificateData.Email}" });
                }
                else
                {
                    return StatusCode(500, "Failed to generate or email the certificate");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating and emailing certificate for {Email}", certificateData.Email);
                return StatusCode(500, "Internal server error while processing the certificate");
            }
        }

        /// <summary>
        /// Preview CSV data without processing certificates
        /// </summary>
        /// <param name="csvFile">CSV file to preview</param>
        /// <returns>Sample of first 5 rows</returns>
        [HttpPost("preview")]
        public async Task<ActionResult<object>> PreviewCsvData(IFormFile csvFile)
        {
            try
            {
                var validationResult = ValidateUploadedFile(csvFile);
                if (!validationResult.IsValid)
                {
                    return BadRequest(validationResult.ErrorMessage);
                }

                using var stream = csvFile.OpenReadStream();
                
                // Validate headers
                var headersValid = await _csvProcessingService.ValidateCsvHeadersAsync(stream);
                if (!headersValid)
                {
                    return BadRequest("CSV file does not have the required headers. Expected: Email, FirstName, LastName, CompletionDate");
                }

                // Get preview data
                var preview = await _csvProcessingService.GetCsvPreviewAsync(stream);
                
                return Ok(new
                {
                    message = "CSV preview (first 5 rows)",
                    expectedHeaders = new[] { "Email", "FirstName", "LastName", "CompletionDate" },
                    sampleData = preview
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error previewing CSV file");
                return StatusCode(500, "Internal server error while previewing the CSV file");
            }
        }

        /// <summary>
        /// Validate certificate configuration
        /// </summary>
        /// <returns>Configuration status</returns>
        [HttpGet("validate-config")]
        public IActionResult ValidateConfiguration()
        {
            try
            {
                var isValid = _certificateService.ValidateCertificateConfiguration();
                
                var configInfo = new
                {
                    isValid = isValid,
                    templateImagePath = _certificateSettings.TemplateImagePath,
                    tempStoragePath = _certificateSettings.TempStoragePath,
                    templateExists = !string.IsNullOrEmpty(_certificateSettings.TemplateImagePath) && 
                                   System.IO.File.Exists(_certificateSettings.TemplateImagePath),
                    tempStorageExists = !string.IsNullOrEmpty(_certificateSettings.TempStoragePath) &&
                                      Directory.Exists(_certificateSettings.TempStoragePath),
                    settings = new
                    {
                        fontFamily = _certificateSettings.FontFamily,
                        nameFontSize = _certificateSettings.NameFontSize,
                        dateFontSize = _certificateSettings.DateFontSize,
                        certificateText = _certificateSettings.CertificateText,
                        maxBatchSize = _certificateSettings.MaxBatchSize
                    }
                };

                if (isValid)
                {
                    return Ok(configInfo);
                }
                else
                {
                    return BadRequest(configInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating certificate configuration");
                return StatusCode(500, "Internal server error while validating configuration");
            }
        }

        /// <summary>
        /// Get certificate generation statistics
        /// </summary>
        /// <returns>Basic stats about the service</returns>
        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            return Ok(new
            {
                serviceStatus = "Running",
                configurationValid = _certificateService.ValidateCertificateConfiguration(),
                maxBatchSize = _certificateSettings.MaxBatchSize,
                maxFileSizeMB = _certificateSettings.MaxFileSizeMB,
                supportedFormats = new[] { "CSV" },
                requiredHeaders = new[] { "Email", "FirstName", "LastName", "CompletionDate" }
            });
        }

        private (bool IsValid, string ErrorMessage) ValidateUploadedFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "No file uploaded or file is empty");
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Only CSV files are supported");
            }

            // Check file size (convert MB to bytes)
            var maxFileSizeBytes = _certificateSettings.MaxFileSizeMB * 1024 * 1024;
            if (file.Length > maxFileSizeBytes)
            {
                return (false, $"File size exceeds maximum limit of {_certificateSettings.MaxFileSizeMB}MB");
            }

            return (true, string.Empty);
        }
    }
}