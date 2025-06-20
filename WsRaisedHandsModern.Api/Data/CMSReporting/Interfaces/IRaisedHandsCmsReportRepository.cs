using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WsRaisedHandsModern.Api.Data.CMSReporting.Entities;

namespace WsRaisedHandsModern.Api.Data.CMSReporting.Interfaces
{
    public interface IRaisedHandsCmsReportRepository
    {
        Task<IEnumerable<tblFormSalvation>> GetAllRaisedHandsAsync();
        Task<IEnumerable<tblFormSalvation>> GetRaisedHandsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}