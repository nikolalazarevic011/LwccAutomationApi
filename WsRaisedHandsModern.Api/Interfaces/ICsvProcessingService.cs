using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WsRaisedHandsModern.Api.DTOs;
using WsRaisedHandsModern.Api.Models;

namespace WsRaisedHandsModern.Api.Interfaces
{
    public interface ICsvProcessingService
    {
        /// <summary>
        /// Parses CSV file stream and returns foundation certificate data
        /// </summary>
        /// <param name="csvStream">CSV file stream</param>
        /// <returns>List of parsed certificate data</returns>
        Task<List<FoundationsCertificateDTO>> ParseFoundationsCsvAsync(Stream csvStream);

        /// <summary>
        /// Parses CSV file from file path
        /// </summary>
        /// <param name="filePath">Path to CSV file</param>
        /// <returns>List of parsed certificate data</returns>
        Task<List<FoundationsCertificateDTO>> ParseFoundationsCsvAsync(string filePath);

        /// <summary>
        /// Converts raw CSV import models to validated certificate DTOs
        /// </summary>
        /// <param name="csvData">Raw CSV import data</param>
        /// <returns>Tuple of valid certificates and validation errors</returns>
        (List<FoundationsCertificateDTO> ValidCertificates, List<CertificateProcessingError> Errors) 
            ConvertAndValidateCsvData(IEnumerable<FoundationsCsvImportModel> csvData);

        /// <summary>
        /// Checks if the CSV has the expected headers
        /// </summary>
        /// <param name="csvStream">CSV file stream</param>
        /// <returns>True if headers match expected format</returns>
        Task<bool> ValidateCsvHeadersAsync(Stream csvStream);

        /// <summary>
        /// Gets sample data from CSV for preview (first 5 rows)
        /// </summary>
        /// <param name="csvStream">CSV file stream</param>
        /// <returns>Sample data for preview</returns>
        Task<List<FoundationsCsvImportModel>> GetCsvPreviewAsync(Stream csvStream);
    }

   
}