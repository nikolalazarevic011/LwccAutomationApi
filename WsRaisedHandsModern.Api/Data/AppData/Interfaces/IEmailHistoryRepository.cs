using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WsRaisedHandsModern.Api.Data.AppData.Entities;

namespace WsRaisedHandsModern.Api.Data.AppData.Interfaces
{
    public interface IEmailHistoryRepository
    {
        Task<EmailHistory> CreateEmailHistoryAsync(EmailHistory emailHistory);
        Task<EmailHistory> UpdateEmailHistoryAsync(EmailHistory emailHistory);
        Task<EmailHistory?> GetEmailHistoryByIdAsync(int id);
        Task<IEnumerable<EmailHistory>> GetEmailHistoryByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<EmailHistory>> GetEmailHistoryByTypeAsync(string emailType, int? limit = null);
        Task<IEnumerable<EmailHistory>> GetEmailHistoryByTypeAndDateRangeAsync(string emailType, DateTime startDate, DateTime endDate, int? limit = null);
        Task<EmailHistory?> GetLastEmailByTypeAsync(string emailType);
        Task<bool> WasEmailSentTodayAsync(string emailType);
    }
}