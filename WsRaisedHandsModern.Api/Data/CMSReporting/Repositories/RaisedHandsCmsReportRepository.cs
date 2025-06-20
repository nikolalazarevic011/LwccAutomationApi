using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WsRaisedHandsModern.Api.Data.CMSReporting.Entities;
using WsRaisedHandsModern.Api.Data.CMSReporting.Interfaces;

namespace WsRaisedHandsModern.Api.Data.CMSReporting.Repositories
{
    public class RaisedHandsCmsReportRepository : IRaisedHandsCmsReportRepository
    {
        private readonly CmsReportingDbContext _context;
        public RaisedHandsCmsReportRepository(CmsReportingDbContext context)
        {
            _context = context;
        }

        // 1. Get all records
        public async Task<IEnumerable<tblFormSalvation>> GetAllRaisedHandsAsync()
        {
            return await _context.TblFormSalvation.AsNoTracking().ToListAsync();
        }

        // 2. Get records by date range
        public async Task<IEnumerable<tblFormSalvation>> GetRaisedHandsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.TblFormSalvation
                .AsNoTracking()
                .Where(x => x.DateCreated >= startDate && x.DateCreated <= endDate)
                .ToListAsync();
        }


    }
}