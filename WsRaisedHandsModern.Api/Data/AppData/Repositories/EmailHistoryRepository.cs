using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WsRaisedHandsModern.Api.Data.AppData.Entities;
using WsRaisedHandsModern.Api.Data.AppData.Interfaces;
using WsRaisedHandsModern.Api.Data.AppData;


namespace WsRaisedHandsModern.Api.Data.AppData.Repositories
{
    public class EmailHistoryRepository : IEmailHistoryRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmailHistoryRepository> _logger;

        public EmailHistoryRepository(ApplicationDbContext context, ILogger<EmailHistoryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EmailHistory> CreateEmailHistoryAsync(EmailHistory emailHistory)
        {
            try
            {
                _context.EmailHistory.Add(emailHistory);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Email history record created with ID: {EmailHistoryId}", emailHistory.Id);
                return emailHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating email history record");
                throw;
            }
        }

        public async Task<EmailHistory> UpdateEmailHistoryAsync(EmailHistory emailHistory)
        {
            try
            {
                _context.EmailHistory.Update(emailHistory);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Email history record updated with ID: {EmailHistoryId}", emailHistory.Id);
                return emailHistory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating email history record with ID: {EmailHistoryId}", emailHistory.Id);
                throw;
            }
        }

        public async Task<EmailHistory?> GetEmailHistoryByIdAsync(int id)
        {
            try
            {
                return await _context.EmailHistory
                    .FirstOrDefaultAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email history record with ID: {EmailHistoryId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<EmailHistory>> GetEmailHistoryByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.EmailHistory
                    .Where(e => e.DateSent >= startDate && e.DateSent <= endDate)
                    .OrderByDescending(e => e.DateSent)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email history for date range: {StartDate} to {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<IEnumerable<EmailHistory>> GetEmailHistoryByTypeAsync(string emailType, int? limit = null)
        {
            try
            {
                var query = _context.EmailHistory
                    .Where(e => e.EmailType == emailType)
                    .OrderByDescending(e => e.DateSent);

                if (limit.HasValue)
                {
                    query = (IOrderedQueryable<EmailHistory>)query.Take(limit.Value);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email history for type: {EmailType}", emailType);
                throw;
            }
        }

        public async Task<EmailHistory?> GetLastEmailByTypeAsync(string emailType)
        {
            try
            {
                return await _context.EmailHistory
                    .Where(e => e.EmailType == emailType)
                    .OrderByDescending(e => e.DateSent)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving last email history for type: {EmailType}", emailType);
                throw;
            }
        }

        public async Task<bool> WasEmailSentTodayAsync(string emailType)
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                return await _context.EmailHistory
                    .AnyAsync(e => e.EmailType == emailType && 
                                  e.DateSent >= today && 
                                  e.DateSent < tomorrow &&
                                  e.Status == "Sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if email was sent today for type: {EmailType}", emailType);
                throw;
            }
        }

        public async Task<IEnumerable<EmailHistory>> GetEmailHistoryByTypeAndDateRangeAsync(string emailType, DateTime startDate, DateTime endDate, int? limit = null)
        {
            try
            {
                var query = _context.EmailHistory
                    .Where(e => e.EmailType == emailType && 
                               e.DateSent >= startDate && 
                               e.DateSent <= endDate)
                    .OrderByDescending(e => e.DateSent);

                if (limit.HasValue)
                {
                    query = (IOrderedQueryable<EmailHistory>)query.Take(limit.Value);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving email history for type: {EmailType} and date range: {StartDate} to {EndDate}", 
                    emailType, startDate, endDate);
                throw;
            }
        }
    }
}