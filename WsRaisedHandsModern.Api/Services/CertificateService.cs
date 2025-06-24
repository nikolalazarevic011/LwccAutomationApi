using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using iTextSharp.text;
using iTextSharp.text.pdf;
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
                var tempStoragePath = Path.IsPathRooted(_certificateSettings.TempStoragePath)
                    ? _certificateSettings.TempStoragePath
                    : Path.Combine(Directory.GetCurrentDirectory(), _certificateSettings.TempStoragePath);

                var tempPath = Path.Combine(tempStoragePath,
                    $"Certificate_{certificateData.FirstName}_{certificateData.LastName}_{Guid.NewGuid()}.pdf");

                // Ensure temp directory exists
                Directory.CreateDirectory(tempStoragePath);

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
            else 
            {
                // Resolve relative path to absolute path
                var templatePath = Path.IsPathRooted(_certificateSettings.TemplateImagePath) 
                    ? _certificateSettings.TemplateImagePath 
                    : Path.Combine(Directory.GetCurrentDirectory(), _certificateSettings.TemplateImagePath);

                _logger.LogInformation("Looking for template at: {TemplatePath}", templatePath);

                if (!File.Exists(templatePath))
                {
                    issues.Add($"Template image file not found: {templatePath}");
                    _logger.LogWarning("Template image not found at {TemplatePath}, will generate text-based certificate instead", templatePath);
                }
            }

            if (string.IsNullOrEmpty(_certificateSettings.TempStoragePath))
            {
                issues.Add("Temporary storage path is not configured");
            }
            else
            {
                try
                {
                    // Resolve relative path to absolute path
                    var tempPath = Path.IsPathRooted(_certificateSettings.TempStoragePath)
                        ? _certificateSettings.TempStoragePath
                        : Path.Combine(Directory.GetCurrentDirectory(), _certificateSettings.TempStoragePath);

                    _logger.LogInformation("Creating temp directory at: {TempPath}", tempPath);
                    Directory.CreateDirectory(tempPath);
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
            using var memoryStream = new MemoryStream();
            
            // Create document in landscape orientation like the Intercessory project
            var document = new Document(PageSize.A4.Rotate());
            var writer = PdfWriter.GetInstance(document, memoryStream);
            
            document.Open();

            // Create fonts exactly like Intercessory project
            BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

            iTextSharp.text.Font titleFont = new iTextSharp.text.Font(bf, 25, iTextSharp.text.Font.BOLD);
            iTextSharp.text.Font textFont = new iTextSharp.text.Font(bf, 18, iTextSharp.text.Font.NORMAL);
            iTextSharp.text.Font nameFont = new iTextSharp.text.Font(bf, 24, iTextSharp.text.Font.BOLD); // Just make it bold for now
            iTextSharp.text.Font courseFont = new iTextSharp.text.Font(bf, 21, iTextSharp.text.Font.BOLD);
            iTextSharp.text.Font dateFont = new iTextSharp.text.Font(bf, 16, iTextSharp.text.Font.NORMAL);

            // Check if template image exists and add it as background
            var templatePath = Path.IsPathRooted(_certificateSettings.TemplateImagePath)
                ? _certificateSettings.TemplateImagePath
                : Path.Combine(Directory.GetCurrentDirectory(), _certificateSettings.TemplateImagePath);

            _logger.LogInformation("Attempting to load template from: {TemplatePath}", templatePath);

            if (!string.IsNullOrEmpty(_certificateSettings.TemplateImagePath) && File.Exists(templatePath))
            {
                try
                {
                    var backgroundImage = Image.GetInstance(templatePath);
                    backgroundImage.ScaleAbsolute(PageSize.A4.Height, PageSize.A4.Width);
                    backgroundImage.SetAbsolutePosition(0, 0);
                    document.Add(backgroundImage);
                    _logger.LogInformation("Successfully loaded template image");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not load template image, generating text-only certificate");
                }
            }
            else
            {
                _logger.LogInformation("No template image found at {TemplatePath}, generating text-only certificate", templatePath);
            }

            // Create main content table with single column for centering
            var mainTable = new PdfPTable(1)
            {
                WidthPercentage = 100,
                HorizontalAlignment = Element.ALIGN_CENTER
            };

            // Add content sections
            AddCertificateContent(mainTable, certificateData, titleFont, textFont, nameFont, courseFont, dateFont);

            document.Add(mainTable);
            document.Close();

            return memoryStream.ToArray();
        }

        private void AddCertificateContent(PdfPTable mainTable, FoundationsCertificateDTO certificateData,
            iTextSharp.text.Font titleFont, iTextSharp.text.Font textFont, iTextSharp.text.Font nameFont, 
            iTextSharp.text.Font courseFont, iTextSharp.text.Font dateFont)
        {
            // Certificate of Completion title
            var titleParagraph = new Paragraph("Certificate of Completion", titleFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            titleParagraph.SetLeading(0, 0.5f);
            
            var titleCell = CreateBorderlessCell();
            titleCell.AddElement(titleParagraph);
            titleCell.PaddingTop = 60f; // Adjust vertical positioning
            mainTable.AddCell(titleCell);

            // "This Certifies That" text
            var certifiesParagraph = new Paragraph("This Certifies That", textFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            certifiesParagraph.SetLeading(0, 1f);
            
            var certifiesCell = CreateBorderlessCell();
            certifiesCell.AddElement(certifiesParagraph);
            certifiesCell.PaddingTop = 20f;
            mainTable.AddCell(certifiesCell);

            // Student Name (highlighted in red)
            var nameParagraph = new Paragraph(certificateData.FullName, nameFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            nameParagraph.SetLeading(0, 1f);
            
            var nameCell = CreateBorderlessCell();
            nameCell.AddElement(nameParagraph);
            nameCell.PaddingTop = 10f;
            mainTable.AddCell(nameCell);

            // "has satisfactorily completed" text
            var completedParagraph = new Paragraph("has satisfactorily completed", textFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            completedParagraph.SetLeading(0, 1f);
            
            var completedCell = CreateBorderlessCell();
            completedCell.AddElement(completedParagraph);
            completedCell.PaddingTop = 25f;
            mainTable.AddCell(completedCell);

            // Course title
            var courseParagraph = new Paragraph("FOUNDATIONS TRAINING", courseFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            courseParagraph.SetLeading(0, 1.5f);
            
            var courseCell = CreateBorderlessCell();
            courseCell.AddElement(courseParagraph);
            courseCell.PaddingTop = 15f;
            mainTable.AddCell(courseCell);

            // "and hereby awarded this certificate by" text
            var awardedParagraph = new Paragraph("and hereby awarded this certificate by", textFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            awardedParagraph.SetLeading(0, 1.5f);
            
            var awardedCell = CreateBorderlessCell();
            awardedCell.AddElement(awardedParagraph);
            awardedCell.PaddingTop = 25f;
            mainTable.AddCell(awardedCell);

            // Organization name
            var orgParagraph = new Paragraph("Living Word Christian Center", textFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            orgParagraph.SetLeading(0, 1.5f);
            
            var orgCell = CreateBorderlessCell();
            orgCell.AddElement(orgParagraph);
            orgCell.PaddingTop = 15f;
            mainTable.AddCell(orgCell);

            // Completion date
            var dateParagraph = new Paragraph(certificateData.FormattedCompletionDate, dateFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            dateParagraph.SetLeading(0, 1.5f);
            
            var dateCell = CreateBorderlessCell();
            dateCell.AddElement(dateParagraph);
            dateCell.PaddingTop = 25f;
            mainTable.AddCell(dateCell);
        }

        private PdfPCell CreateBorderlessCell()
        {
            var cell = new PdfPCell
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            return cell;
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