using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using WsRaisedHandsModern.Api.DTOs;
using WsRaisedHandsModern.Api.Models;
using WsRaisedHandsModern.Api.Interfaces;

namespace WsRaisedHandsModern.Api.Services
{
    public class CsvProcessingService : ICsvProcessingService
    {
        private readonly ILogger<CsvProcessingService> _logger;
        private readonly string[] _expectedHeaders = { "Email", "FirstName", "LastName", "CompletionDate" };

        public CsvProcessingService(ILogger<CsvProcessingService> logger)
        {
            _logger = logger;
        }

        public async Task<List<FoundationsCertificateDTO>> ParseFoundationsCsvAsync(Stream csvStream)
        {
            try
            {
                using var reader = new StreamReader(csvStream);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null
                });

                // Read raw CSV data first
                var rawRecords = await Task.Run(() => csv.GetRecords<FoundationsCsvImportModel>().ToList());

                // Convert and validate
                var (validCertificates, errors) = ConvertAndValidateCsvData(rawRecords);

                if (errors.Any())
                {
                    _logger.LogWarning("Found {ErrorCount} validation errors while parsing CSV", errors.Count);
                    foreach (var error in errors.Take(5)) // Log first 5 errors
                    {
                        _logger.LogWarning("Row {RowNumber}: {ErrorMessage}", error.RowNumber, error.ErrorMessage);
                    }
                }

                _logger.LogInformation("Successfully parsed {ValidCount} valid certificates from CSV", validCertificates.Count);
                return validCertificates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing CSV stream");
                throw new InvalidOperationException("Failed to parse CSV file", ex);
            }
        }

        public async Task<List<FoundationsCertificateDTO>> ParseFoundationsCsvAsync(string filePath)
        {
            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                return await ParseFoundationsCsvAsync(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing CSV file at {FilePath}", filePath);
                throw;
            }
        }

        public CsvValidationResult ValidateCsvData(IEnumerable<FoundationsCsvImportModel> csvData)
        {
            var result = new CsvValidationResult
            {
                TotalRows = csvData.Count()
            };

            var errors = new List<CertificateProcessingError>();
            int rowNumber = 1; // Start at 1 (excluding header)

            foreach (var row in csvData)
            {
                rowNumber++;
                var rowErrors = ValidateRow(row, rowNumber);
                errors.AddRange(rowErrors);
            }

            result.DataErrors = errors;
            result.ValidRows = result.TotalRows - errors.Count;
            result.IsValid = errors.Count == 0;

            return result;
        }

        public (List<FoundationsCertificateDTO> ValidCertificates, List<CertificateProcessingError> Errors)
            ConvertAndValidateCsvData(IEnumerable<FoundationsCsvImportModel> csvData)
        {
            var validCertificates = new List<FoundationsCertificateDTO>();
            var errors = new List<CertificateProcessingError>();
            int rowNumber = 1; // Start at 1 (excluding header)

            foreach (var row in csvData)
            {
                rowNumber++;
                var rowErrors = ValidateRow(row, rowNumber);

                if (rowErrors.Any())
                {
                    errors.AddRange(rowErrors);
                }
                else
                {
                    // Convert to DTO if valid
                    var certificate = new FoundationsCertificateDTO
                    {
                        Email = row.Email.Trim(),
                        FirstName = row.FirstName.Trim(),
                        LastName = row.LastName.Trim(),
                        CompletionDate = row.GetParsedCompletionDate()!.Value
                    };
                    validCertificates.Add(certificate);
                }
            }

            return (validCertificates, errors);
        }

        public async Task<bool> ValidateCsvHeadersAsync(Stream csvStream)
        {
            try
            {
                using var reader = new StreamReader(csvStream);
                var headerLine = await reader.ReadLineAsync();

                if (string.IsNullOrEmpty(headerLine))
                    return false;

                var headers = headerLine.Split(',').Select(h => h.Trim().Trim('"')).ToArray();

                // Check if all expected headers are present (case-insensitive)
                foreach (var expectedHeader in _expectedHeaders)
                {
                    if (!headers.Any(h => string.Equals(h, expectedHeader, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.LogWarning("Missing expected header: {ExpectedHeader}", expectedHeader);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating CSV headers");
                return false;
            }
        }

        public async Task<List<FoundationsCsvImportModel>> GetCsvPreviewAsync(Stream csvStream)
        {
            try
            {
                using var reader = new StreamReader(csvStream);
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null
                });

                var records = new List<FoundationsCsvImportModel>();
                var recordsRead = 0;
                const int maxPreviewRecords = 5;

                await foreach (var record in csv.GetRecordsAsync<FoundationsCsvImportModel>())
                {
                    records.Add(record);
                    recordsRead++;

                    if (recordsRead >= maxPreviewRecords)
                        break;
                }

                return records;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CSV preview");
                throw;
            }
        }

        private List<CertificateProcessingError> ValidateRow(FoundationsCsvImportModel row, int rowNumber)
        {
            var errors = new List<CertificateProcessingError>();

            // Check if all required fields are present
            if (!row.IsValid())
            {
                var missingFields = new List<string>();
                if (string.IsNullOrWhiteSpace(row.Email)) missingFields.Add("Email");
                if (string.IsNullOrWhiteSpace(row.FirstName)) missingFields.Add("FirstName");
                if (string.IsNullOrWhiteSpace(row.LastName)) missingFields.Add("LastName");
                if (string.IsNullOrWhiteSpace(row.CompletionDate)) missingFields.Add("CompletionDate");

                errors.Add(new CertificateProcessingError
                {
                    RowNumber = rowNumber,
                    Email = row.Email ?? "",
                    FirstName = row.FirstName ?? "",
                    LastName = row.LastName ?? "",
                    ErrorType = "ValidationError",
                    ErrorMessage = $"Missing required fields: {string.Join(", ", missingFields)}"
                });
            }

            // Validate email format
            if (!string.IsNullOrWhiteSpace(row.Email) && !row.IsValidEmail())
            {
                errors.Add(new CertificateProcessingError
                {
                    RowNumber = rowNumber,
                    Email = row.Email,
                    FirstName = row.FirstName ?? "",
                    LastName = row.LastName ?? "",
                    ErrorType = "ValidationError",
                    ErrorMessage = "Invalid email format"
                });
            }

            // Validate completion date
            if (!string.IsNullOrWhiteSpace(row.CompletionDate) && row.GetParsedCompletionDate() == null)
            {
                errors.Add(new CertificateProcessingError
                {
                    RowNumber = rowNumber,
                    Email = row.Email ?? "",
                    FirstName = row.FirstName ?? "",
                    LastName = row.LastName ?? "",
                    ErrorType = "ValidationError",
                    ErrorMessage = $"Invalid date format: {row.CompletionDate}"
                });
            }

            // Check for future dates
            var parsedDate = row.GetParsedCompletionDate();
            if (parsedDate.HasValue && parsedDate.Value > DateTime.Now)
            {
                errors.Add(new CertificateProcessingError
                {
                    RowNumber = rowNumber,
                    Email = row.Email ?? "",
                    FirstName = row.FirstName ?? "",
                    LastName = row.LastName ?? "",
                    ErrorType = "ValidationError",
                    ErrorMessage = "Completion date cannot be in the future"
                });
            }

            return errors;
        }
    }
}