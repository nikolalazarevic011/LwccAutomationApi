using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SixLabors.ImageSharp;
using WsRaisedHandsModern.Api.DTOs;
using WsRaisedHandsModern.Api.Helpers;
using WsRaisedHandsModern.Api.Interfaces;

namespace WsRaisedHandsModern.Api.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly CertificateSettings _certificateSettings;
        private readonly IEmailService _emailService;
        private readonly ILogger<CertificateService> _logger;

        public CertificateService(
            IOptions<CertificateSettings> certificateSettings,
            IEmailService emailService,
            ILogger<CertificateService> logger)
        {
            _certificateSettings = certificateSettings.Value;
            _emailService = emailService;
            _logger = logger;

            // Configure QuestPDF license (Community License for non-commercial use)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateCertificateAsync(FoundationsCertificateDTO certificateData)
        {
            try
            {
                _logger.LogInformation("Generating certificate for {FullName}", certificateData.FullName);

                if (!ValidateCertificateConfiguration())
                {
                    throw new InvalidOperationException("Certificate configuration is invalid");
                }

                var pdfBytes = await Task.Run(() => CreateCertificatePdf(certificateData));

                _logger.LogInformation("Successfully generated certificate PDF for {FullName}", certificateData.FullName);
                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating certificate for {FullName}", certificateData.FullName);
                throw;
            }
        }

        public async Task<bool> GenerateCertificateToFileAsync(FoundationsCertificateDTO certificateData, string outputPath)
        {
            try
            {
                var pdfBytes = await GenerateCertificateAsync(certificateData);
                await File.WriteAllBytesAsync(outputPath, pdfBytes);

                _logger.LogInformation("Certificate saved to {OutputPath}", outputPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving certificate to {OutputPath}", outputPath);
                return false;
            }
        }

        public async Task<CertificateProcessingResult> ProcessFoundationsCertificatesAsync(IEnumerable<FoundationsCertificateDTO> certificates)
        {
            var result = new CertificateProcessingResult
            {
                ProcessingStartTime = DateTime.Now,
                TotalRecords = certificates.Count()
            };

            _logger.LogInformation("Starting batch processing of {TotalRecords} certificates", result.TotalRecords);

            var certificateList = certificates.ToList();

            // Process in batches to avoid overwhelming the system
            var batchSize = Math.Min(_certificateSettings.MaxBatchSize, certificateList.Count);

            for (int i = 0; i < certificateList.Count; i += batchSize)
            {
                var batch = certificateList.Skip(i).Take(batchSize);
                await ProcessBatchAsync(batch, result);
            }

            result.ProcessingEndTime = DateTime.Now;

            _logger.LogInformation("Batch processing completed. Success: {SuccessCount}, Failed: {FailCount}, Total Time: {TotalTime}",
                result.SuccessfullyProcessed, result.Failed, result.TotalProcessingTime);

            return result;
        }

        public async Task<bool> GenerateAndEmailCertificateAsync(FoundationsCertificateDTO certificateData)
        {
            try
            {
                _logger.LogInformation("Generating and emailing certificate for {FullName} ({Email})",
                    certificateData.FullName, certificateData.Email);

                // Generate certificate PDF
                var pdfBytes = await GenerateCertificateAsync(certificateData);

                // Create temporary file for attachment
                var tempPath = Path.Combine(_certificateSettings.TempStoragePath,
                    $"Certificate_{certificateData.FirstName}_{certificateData.LastName}_{Guid.NewGuid()}.pdf");

                // Ensure temp directory exists
                Directory.CreateDirectory(_certificateSettings.TempStoragePath);

                await File.WriteAllBytesAsync(tempPath, pdfBytes);

                try
                {
                    // Create email attachment
                    var attachment = new Attachment(tempPath, "application/pdf")
                    {
                        Name = $"FoundationsTraining_Certificate_{certificateData.FirstName}_{certificateData.LastName}.pdf"
                    };

                    // Send email
                    await _emailService.SendEmailAsync(
                        certificateData.Email,
                        _certificateSettings.CertificateEmailSubject,
                        string.Format(_certificateSettings.CertificateEmailBody, certificateData.FullName),
                        new[] { attachment }
                    );

                    attachment.Dispose();
                    _logger.LogInformation("Certificate emailed successfully to {Email}", certificateData.Email);
                    return true;
                }
                finally
                {
                    // Clean up temporary file
                    try
                    {
                        if (File.Exists(tempPath))
                            File.Delete(tempPath);
                    }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogWarning(cleanupEx, "Failed to delete temporary file {TempPath}", tempPath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating and emailing certificate for {Email}", certificateData.Email);
                return false;
            }
        }

        public bool ValidateCertificateConfiguration()
        {
            var issues = new List<string>();

            if (string.IsNullOrEmpty(_certificateSettings.TemplateImagePath))
            {
                issues.Add("Template image path is not configured");
            }
            else if (!File.Exists(_certificateSettings.TemplateImagePath))
            {
                issues.Add($"Template image file not found: {_certificateSettings.TemplateImagePath}");
                _logger.LogWarning("Template image not found, will generate text-based certificate instead");
            }

            if (string.IsNullOrEmpty(_certificateSettings.TempStoragePath))
                issues.Add("Temporary storage path is not configured");
            else
            {
                try
                {
                    Directory.CreateDirectory(_certificateSettings.TempStoragePath);
                }
                catch (Exception ex)
                {
                    issues.Add($"Cannot create temporary storage directory: {ex.Message}");
                }
            }

            if (_certificateSettings.NameFontSize <= 0)
                issues.Add("Name font size must be greater than 0");

            if (_certificateSettings.DateFontSize <= 0)
                issues.Add("Date font size must be greater than 0");

            // Only log errors, not warnings about missing template
            var errors = issues.Where(i => !i.Contains("Template image file not found")).ToList();

            if (errors.Any())
            {
                foreach (var error in errors)
                {
                    _logger.LogError("Certificate configuration issue: {Issue}", error);
                }
                return false;
            }

            // Log template warning but still return true (we can work without template)
            if (issues.Any(i => i.Contains("Template image file not found")))
            {
                _logger.LogWarning("Template image not found - will generate text-based certificates");
            }

            return true;
        }

        private byte[] CreateCertificatePdf(FoundationsCertificateDTO certificateData)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    // page.Size(PageSizes.A4.Landscape());
                    page.Size(800, 600); // Custom size for certificate
                    page.Margin(0);

                    if (File.Exists(_certificateSettings.TemplateImagePath))
                    {
                        page.Content().Layers(layers =>
                        {
                            // Background image
                            layers.PrimaryLayer().Image(_certificateSettings.TemplateImagePath)
                                .FitArea();

                            // Text overlay with horizontal padding to center on 800px template
                            layers.Layer().AlignCenter().MaxWidth(600).Column(column =>
                            {
                                column.Item().PaddingTop(90)
                                    // .AlignCenter()
                                    // .PaddingLeft(-100)
                                    // .PaddingRight(-270)
                                    .Text("Certificate of Completion")

                                    .FontFamily(_certificateSettings.FontFamily)
                                    .FontSize(28)
                                    .FontColor(_certificateSettings.FontColor)
                                    .Bold();

                                column.Item().PaddingTop(15)
                                    .AlignCenter()
                                    .Text("This Certifies That")
                                    .FontFamily(_certificateSettings.FontFamily)
                                    .FontSize(16)
                                    .FontColor(_certificateSettings.FontColor);

                                column.Item().PaddingTop(20)
                                    // .AlignCenter()
                                    .AlignLeft()
                                    .Text(certificateData.FullName)
                                    .FontFamily(_certificateSettings.FontFamily)
                                    .FontSize(_certificateSettings.NameFontSize)
                                    .FontColor("#CC0000")
                                    .Bold();

                                column.Item().PaddingTop(20)
                                    // .AlignCenter()
                                    .Text("has satisfactorily completed")
                                    .FontFamily(_certificateSettings.FontFamily)
                                    .FontSize(16)
                                    .FontColor(_certificateSettings.FontColor);

                                column.Item().PaddingTop(10)
                                    // .AlignCenter()
                                    .Text("FOUNDATIONS TRAINING")
                                    .FontFamily(_certificateSettings.FontFamily)
                                    .FontSize(22)
                                    .FontColor(_certificateSettings.FontColor)
                                    .Bold();

                                column.Item().PaddingTop(20)
                                    // .AlignCenter()
                                    .Text("and hereby awarded this certificate by")
                                    .FontFamily(_certificateSettings.FontFamily)
                                    .FontSize(16)
                                    .FontColor(_certificateSettings.FontColor);

                                column.Item().PaddingTop(10)
                                    // .AlignCenter()
                                    .Text("Living Word Christian Center")
                                    .FontFamily(_certificateSettings.FontFamily)
                                    .FontSize(18)
                                    .FontColor(_certificateSettings.FontColor)
                                    .Bold();

                                column.Item().PaddingTop(20)
                                    // .AlignCenter()
                                    .Text(certificateData.FormattedCompletionDate)
                                    .FontFamily(_certificateSettings.FontFamily)
                                    .FontSize(_certificateSettings.DateFontSize)
                                    .FontColor(_certificateSettings.FontColor);
                            });
                        });
                    }
                    else
                    {
                        // Fallback text-based certificate
                        page.Content().Padding(50).Column(column =>
                        {
                            column.Spacing(30);

                            column.Item().AlignCenter().Text("CERTIFICATE OF COMPLETION")
                                .FontFamily(_certificateSettings.FontFamily)
                                .FontSize(36)
                                .FontColor(_certificateSettings.FontColor)
                                .Bold();

                            column.Item().AlignCenter().Text("This certifies that")
                                .FontFamily(_certificateSettings.FontFamily)
                                .FontSize(20)
                                .FontColor(_certificateSettings.FontColor);

                            column.Item().AlignCenter().Text(certificateData.FullName)
                                .FontFamily(_certificateSettings.FontFamily)
                                .FontSize(_certificateSettings.NameFontSize)
                                .FontColor(_certificateSettings.FontColor)
                                .Bold();

                            column.Item().AlignCenter().Text(_certificateSettings.CertificateText)
                                .FontFamily(_certificateSettings.FontFamily)
                                .FontSize(18)
                                .FontColor(_certificateSettings.FontColor);

                            column.Item().AlignCenter().Text(certificateData.FormattedCompletionDate)
                                .FontFamily(_certificateSettings.FontFamily)
                                .FontSize(_certificateSettings.DateFontSize)
                                .FontColor(_certificateSettings.FontColor);

                            column.Item().Height(100);

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().AlignLeft().Text("_______________________")
                                    .FontFamily(_certificateSettings.FontFamily)
                                    .FontSize(14);

                                row.RelativeItem().AlignRight().Text("_______________________")
                                    .FontFamily(_certificateSettings.FontFamily)
                                    .FontSize(14);
                            });

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().AlignLeft().Text("Instructor Signature")
                                    .FontFamily(_certificateSettings.FontFamily)
                                    .FontSize(12);

                                row.RelativeItem().AlignRight().Text("Date")
                                    .FontFamily(_certificateSettings.FontFamily)
                                    .FontSize(12);
                            });
                        });
                    }
                });
            }).GeneratePdf();
        }
        private async Task ProcessBatchAsync(IEnumerable<FoundationsCertificateDTO> batch, CertificateProcessingResult result)
        {
            var tasks = batch.Select(async certificate =>
            {
                try
                {
                    var success = await GenerateAndEmailCertificateAsync(certificate);
                    if (success)
                    {
                        result.SuccessfullyProcessed++;
                    }
                    else
                    {
                        result.Failed++;
                        result.Errors.Add(new CertificateProcessingError
                        {
                            Email = certificate.Email,
                            FirstName = certificate.FirstName,
                            LastName = certificate.LastName,
                            ErrorType = "CertificateGenerationError",
                            ErrorMessage = "Failed to generate or email certificate"
                        });
                    }
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add(new CertificateProcessingError
                    {
                        Email = certificate.Email,
                        FirstName = certificate.FirstName,
                        LastName = certificate.LastName,
                        ErrorType = "CertificateGenerationError",
                        ErrorMessage = ex.Message
                    });
                    _logger.LogError(ex, "Error processing certificate for {Email}", certificate.Email);
                }
            });

            await Task.WhenAll(tasks);
        }
    }
}