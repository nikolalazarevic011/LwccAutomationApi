using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WsRaisedHandsModern.Api.DTOs;

namespace WsRaisedHandsModern.Api.Interfaces
{
    public interface ICertificateService
    {
        /// <summary>
        /// Generates a single certificate PDF for the given participant
        /// </summary>
        /// <param name="certificateData">Certificate data including name and completion date</param>
        /// <returns>PDF certificate as byte array</returns>
        Task<byte[]> GenerateCertificateAsync(FoundationsCertificateDTO certificateData);

        /// <summary>
        /// Generates a certificate and saves it to a file path, WITHOUT EMAILING, 
        /// </summary>
        /// <param name="certificateData">Certificate data including name and completion date</param>
        /// <param name="outputPath">Full path where the PDF should be saved</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> GenerateCertificateToFileAsync(FoundationsCertificateDTO certificateData, string outputPath);

        /// <summary>
        /// Processes a batch of certificates from CSV data
        /// </summary>
        /// <param name="certificates">List of certificate data to process</param>
        /// <returns>Processing result with success/failure statistics</returns>
        Task<CertificateProcessingResult> ProcessFoundationsCertificatesAsync(IEnumerable<FoundationsCertificateDTO> certificates);

        /// <summary>
        /// Generates a single certificate and emails it to the recipient
        /// </summary>
        /// <param name="certificateData">Certificate data including name and completion date</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> GenerateAndEmailCertificateAsync(FoundationsCertificateDTO certificateData);

        /// <summary>
        /// Validates certificate template and configuration
        /// </summary>
        /// <returns>True if template exists and configuration is valid</returns>
        bool ValidateCertificateConfiguration();
    }
}