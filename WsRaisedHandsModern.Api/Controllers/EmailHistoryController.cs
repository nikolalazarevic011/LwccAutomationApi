using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WsRaisedHandsModern.Api.Data.CMSReporting.Entities;
using WsRaisedHandsModern.Api.Data.CMSReporting.Interfaces;
using WsRaisedHandsModern.Api.Data.AppData.Interfaces;
using WsRaisedHandsModern.Api.Data.AppData.Entities;
using Microsoft.Extensions.Logging;
using WsRaisedHandsModern.Api.Interfaces;
using WsRaisedHandsModern.Api.Helpers;
using Microsoft.Extensions.Options;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using WsRaisedHandsModern.Api.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace WsRaisedHandsModern.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailHistoryController : BaseApiController
    {
        private readonly IRaisedHandsCmsReportRepository _cmsReportRepo; //uses CMS Reporting repository for raised hands data
        private readonly IEmailHistoryRepository _emailHistoryRepo; //uses AppData repository for email history
        private readonly new ILogger<RaisedHandsSalvationController> _logger;

        public EmailHistoryController(
            IRaisedHandsCmsReportRepository cmsReportRepo,
            IEmailHistoryRepository emailHistoryRepo,
            IEmailService emailService,
            IOptions<EmailSettings> emailSettings,
            IWebHostEnvironment env,
            ILogger<RaisedHandsSalvationController> logger,
            IExcelService excelService) : base(logger)
        {
            _cmsReportRepo = cmsReportRepo;
            _emailHistoryRepo = emailHistoryRepo;
            _logger = logger;
        }

        // GET: api/EmailHistory/email-history
        [HttpGet("email-history")]
        public async Task<ActionResult<IEnumerable<EmailHistoryDto>>> GetEmailHistory([FromQuery] EmailHistoryQueryDto query)
        {
            try
            {
                IEnumerable<EmailHistory> emailHistories;
                // GET /EmailHistory/email-history?EmailType=RaisedHandsSalvation&StartDate=2022-06-01&EndDate=2022-06-30&Limit=10
                if (query.StartDate.HasValue && query.EndDate.HasValue && !string.IsNullOrEmpty(query.EmailType))
                {
                    // Combined filter: email type AND date range
                    emailHistories = await _emailHistoryRepo.GetEmailHistoryByTypeAndDateRangeAsync(
                        query.EmailType, query.StartDate.Value, query.EndDate.Value, query.Limit);
                }
                   // GET /EmailHistory/email-history?StartDate=2022-06-01&EndDate=2022-06-30
                else if (query.StartDate.HasValue && query.EndDate.HasValue)
                {
                    // Date range only
                    emailHistories = await _emailHistoryRepo.GetEmailHistoryByDateRangeAsync(query.StartDate.Value, query.EndDate.Value);
                }
                  // GET /EmailHistory/email-history?EmailType=RaisedHandsSalvation&Limit=5
                else if (!string.IsNullOrEmpty(query.EmailType))
                {
                    // Email type only
                    emailHistories = await _emailHistoryRepo.GetEmailHistoryByTypeAsync(query.EmailType, query.Limit);
                }
                else
                {
                    // Get recent email history if no specific filters
                    var defaultEndDate = DateTime.UtcNow;
                    var defaultStartDate = defaultEndDate.AddDays(-30); // Last 30 days
                    emailHistories = await _emailHistoryRepo.GetEmailHistoryByDateRangeAsync(defaultStartDate, defaultEndDate);
                }

                  // Map to DTO
                var emailHistoryDtos = MapEmailHistoryResultsToList(emailHistories, query.Limit);
                
                return Ok(emailHistoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email history");
                return StatusCode(500, "Internal server error while retrieving email history.");
            }
        }

        // GET: api/RaisedHandsSalvation/email-history/{id}
        [HttpGet("email-history/{id}")]
        public async Task<ActionResult<EmailHistoryDto>> GetEmailHistoryById(int id)
        {
            try
            {
                var emailHistory = await _emailHistoryRepo.GetEmailHistoryByIdAsync(id);
                if (emailHistory == null)
                {
                    return NotFound($"Email history with ID {id} not found.");
                }

                var emailHistoryDto = MapEmailHistoryResults(emailHistory);

                return Ok(emailHistoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email history with ID: {EmailHistoryId}", id);
                return StatusCode(500, "Internal server error while retrieving email history.");
            }
        }

        // GET: api/RaisedHandsSalvation/email-history/last-sent/{emailType}
        [HttpGet("email-history/last-sent/{emailType}")]
        public async Task<ActionResult<EmailHistoryDto>> GetLastEmailByType(string emailType)
        {
            try
            {
                var emailHistory = await _emailHistoryRepo.GetLastEmailByTypeAsync(emailType);
                if (emailHistory == null)
                {
                    return NotFound($"No email history found for email type: {emailType}");
                }

                var emailHistoryDto = MapEmailHistoryResults(emailHistory);

                return Ok(emailHistoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving last email history for type: {EmailType}", emailType);
                return StatusCode(500, "Internal server error while retrieving email history.");
            }
        }

        // GET: api/RaisedHandsSalvation/email-history/sent-today/{emailType}
        [HttpGet("email-history/sent-today/{emailType}")]
        public async Task<ActionResult<bool>> WasEmailSentToday(string emailType)
        {
            try
            {
                var wasSentToday = await _emailHistoryRepo.WasEmailSentTodayAsync(emailType);
                return Ok(wasSentToday);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if email was sent today for type: {EmailType}", emailType);
                return StatusCode(500, "Internal server error while checking email status.");
            }
        }

        public EmailHistoryDto MapEmailHistoryResults(EmailHistory result)
        {
            var emailHistoryResults = new EmailHistoryDto
            {
                Id = result.Id,
                Subject = result.Subject,
                ToEmail = result.ToEmail,
                CcEmail = result.CcEmail,
                BccEmail = result.BccEmail,
                DateSent = result.DateSent,
                EmailType = result.EmailType,
                ReportDateRange = result.ReportDateRange,
                RecordCount = result.RecordCount,
                AttachmentFilenames = result.AttachmentFilenames,
                Status = result.Status,
                ErrorMessage = result.ErrorMessage,
                SentBy = result.SentBy,
                CreatedAt = result.CreatedAt
                // Add other property mappings as needed
            };
    
          return emailHistoryResults;
        }

        public IEnumerable<EmailHistoryDto> MapEmailHistoryResultsToList(IEnumerable<EmailHistory> result, int limit)
        {
            var emailHistoryResults = result.Select(r => new EmailHistoryDto
            {
                Id = r.Id,
                Subject = r.Subject,
                ToEmail = r.ToEmail,
                CcEmail = r.CcEmail,
                BccEmail = r.BccEmail,
                DateSent = r.DateSent,
                EmailType = r.EmailType,
                ReportDateRange = r.ReportDateRange,
                RecordCount = r.RecordCount,
                AttachmentFilenames = r.AttachmentFilenames,
                Status = r.Status,
                ErrorMessage = r.ErrorMessage,
                SentBy = r.SentBy,
                CreatedAt = r.CreatedAt
                // Add other property mappings as needed
            }).Take(limit);

            return emailHistoryResults;
        }
    }
}